import { Component, OnInit, OnDestroy, signal, AfterViewInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { VehicleService, Vehicle } from '../vehicles/services/vehicle.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../authentication/services/auth.service';
import * as signalR from '@microsoft/signalr';
import * as L from 'leaflet';

interface VehicleTrackerState {
  vehicle: Vehicle;
  latitude?: number;
  longitude?: number;
  speed?: number;
  lastUpdate?: Date;
  marker?: L.Marker;
  isSimulating: boolean;
  sessionId?: string;
  status: 'Offline' | 'Starting' | 'Active' | 'LostConnection' | 'Stopped';
}

@Component({
  selector: 'app-tracking',
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './tracking.html',
  styleUrl: './tracking.css'
})
export class TrackingComponent implements OnInit, OnDestroy, AfterViewInit {
  private map!: L.Map;
  private hubConnection!: signalR.HubConnection;
  private simulationIntervalId: any = null;
  private watchId: any = null;
  private simulationSessionId: string | null = null;

  vehiclesState = signal<VehicleTrackerState[]>([]);
  selectedVehicle = signal<VehicleTrackerState | null>(null);
  isLoading = signal(true);
  connectionStatus = signal<'Connected' | 'Disconnected' | 'Connecting'>('Disconnected');
  isTransmitting = signal<string | null>(null); // Armazena a matrícula do veículo em transmissão
  activeSessionId = signal<string | null>(null); // Armazena o ID da sessão de tracking ativa
  transmissionTime = signal<string>('00:00.000');
  private transmissionInterval: any = null;
  private startTime: number = 0;
  Math = Math;

  // Rota de Simulação em Luanda (Marginal -> Kinaxixi -> Maianga)
  private readonly luandaSimulationRoute = [
    { lat: -8.8078, lng: 13.2235, speed: 45 },
    { lat: -8.8105, lng: 13.2258, speed: 42 },
    { lat: -8.8132, lng: 13.2281, speed: 40 },
    { lat: -8.8169, lng: 13.2304, speed: 35 },
    { lat: -8.8202, lng: 13.2325, speed: 48 },
    { lat: -8.8239, lng: 13.2341, speed: 50 },
    { lat: -8.8271, lng: 13.2349, speed: 55 },
    { lat: -8.8312, lng: 13.2355, speed: 38 },
    { lat: -8.8348, lng: 13.2361, speed: 20 },
    { lat: -8.8384, lng: 13.2369, speed: 15 },
    { lat: -8.8415, lng: 13.2382, speed: 40 },
    { lat: -8.8441, lng: 13.2405, speed: 42 }
  ];
  private simulationIndex = 0;

  constructor(
    private vehicleService: VehicleService,
    private http: HttpClient,
    public authService: AuthService,
    private ngZone: NgZone
  ) {}

  getTransmittingVehicleName(): string {
    const plate = this.isTransmitting();
    if (!plate) return '';
    const state = this.vehiclesState().find(s => s.vehicle.licensePlate === plate);
    return state ? `${state.vehicle.brand} ${state.vehicle.model}` : '';
  }

  getTransmittingVehicleState(): VehicleTrackerState | undefined {
    const plate = this.isTransmitting();
    if (!plate) return undefined;
    return this.vehiclesState().find(s => s.vehicle.licensePlate === plate);
  }

  ngOnInit(): void {
    this.loadVehiclesAndConnect();
  }

  ngAfterViewInit(): void {
    this.initMap();
  }

  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
    this.stopSimulation();
    this.stopDeviceTracking();
  }

  private initMap(): void {
    // Coordenadas padrão de Luanda (Baixa/Marginal)
    this.map = L.map('map', {
      zoomControl: false
    }).setView([-8.815, 13.230], 13);

    L.control.zoom({
      position: 'bottomright'
    }).addTo(this.map);

    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
      subdomains: 'abcd',
      maxZoom: 20
    }).addTo(this.map);
  }

  private loadVehiclesAndConnect(): void {
    this.isLoading.set(true);
    this.vehicleService.getVehicles().subscribe({
      next: (data) => {
        const states = data.map(v => ({
          vehicle: v,
          isSimulating: false,
          status: 'Offline' as const
        }));
        this.vehiclesState.set(states);
        
        // Carrega as coordenadas recentes para repor estado no Admin (caso de F5)
        this.loadActiveSessions();

        this.isLoading.set(false);
        this.connectSignalR();

        // Se for o instrutor e deu F5, tenta restaurar a sessão de partilha
        this.restoreActiveTrackingSession();
      },
      error: (err) => {
        console.error('Erro ao carregar veículos para rastreio', err);
        this.isLoading.set(false);
      }
    });
  }

  private loadActiveSessions(): void {
    this.http.get<any[]>('/api/tracking/active').subscribe({
      next: (sessions) => {
        const states = this.vehiclesState();
        sessions.forEach(sess => {
          const index = states.findIndex(s => (s.vehicle.id || '').toLowerCase() === (sess.vehicleId || '').toLowerCase());
          if (index !== -1) {
            states[index].sessionId = sess.sessionId;
            states[index].status = sess.status as any;
            if (sess.latitude != null && sess.longitude != null) {
              states[index].latitude = sess.latitude;
              states[index].longitude = sess.longitude;
              states[index].speed = sess.speed;
              states[index].lastUpdate = new Date(sess.lastUpdate);
              
              // Desenhar no mapa se o mapa já estiver inicializado
              if (this.map) {
                this.updateMarkerOnMap(states[index]);
              }
            }
          }
        });
        this.vehiclesState.set([...states]);
      },
      error: (err) => {
        console.error('Erro ao carregar sessões de rastreio ativas', err);
      }
    });
  }

  private restoreActiveTrackingSession(): void {
    const savedSessionId = localStorage.getItem('frotago_active_tracking_session_id');
    if (savedSessionId) {
      this.http.get<{ isValid: boolean, status: string, vehicle: Vehicle }>(`/api/tracking/session/${savedSessionId}`).subscribe({
        next: (res) => {
          if (res.isValid && res.vehicle) {
            const states = this.vehiclesState();
            const state = states.find(s => (s.vehicle.id || '').toLowerCase() === (res.vehicle.id || '').toLowerCase());
            if (state) {
              console.log(`[Sessão] Restaurando partilha ativa do GPS para a viatura: ${state.vehicle.licensePlate}`);
              this.activeSessionId.set(savedSessionId);
              this.isTransmitting.set(state.vehicle.licensePlate);
              
              // Pequeno atraso para dar tempo de inicializar componentes/permissões se necessário
              setTimeout(() => {
                this.startDeviceTracking(state);
              }, 100);
            }
          } else {
            console.log('[Sessão] Sessão anterior expirada ou inválida. Limpando local storage.');
            localStorage.removeItem('frotago_active_tracking_session_id');
          }
        },
        error: (err) => {
          console.error('Erro ao validar sessão de tracking anterior', err);
          localStorage.removeItem('frotago_active_tracking_session_id');
        }
      });
    }
  }

  private connectSignalR(): void {
    this.connectionStatus.set('Connecting');
    const token = this.authService.token();

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/gps', {
        accessTokenFactory: () => token || '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 1000, 3000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection.on('LocationUpdated', (vehicleId: string, lat: number, lng: number, speed: number) => {
      console.log('[SignalR] LocationUpdated recebido:', { vehicleId, lat, lng, speed });
      this.ngZone.run(() => {
        this.handleLocationUpdate(vehicleId, lat, lng, speed);
      });
    });

    this.hubConnection.on('TrackingStopped', (vehicleId: string) => {
      console.log('[SignalR] TrackingStopped recebido para:', vehicleId);
      this.ngZone.run(() => {
        this.handleTrackingStopped(vehicleId);
      });
    });

    this.hubConnection.on('TrackingStatusChanged', (sessionId: string, vehicleId: string, status: string) => {
      console.log('[SignalR] TrackingStatusChanged recebido:', { sessionId, vehicleId, status });
      this.ngZone.run(() => {
        this.handleTrackingStatusChanged(sessionId, vehicleId, status);
      });
    });

    this.hubConnection.start()
      .then(() => {
        console.log('[SignalR] ✅ Conexão estabelecida com sucesso! ConnectionId:', this.hubConnection.connectionId);
        this.connectionStatus.set('Connected');
      })
      .catch((err) => {
        console.error('[SignalR] ❌ Erro ao conectar:', err);
        this.connectionStatus.set('Disconnected');
        // Tentar reconectar após 5 segundos em caso de falha inicial
        setTimeout(() => {
          if (this.connectionStatus() === 'Disconnected') {
            console.log('[SignalR] A tentar reconectar...');
            this.connectSignalR();
          }
        }, 5000);
      });

    this.hubConnection.onreconnecting((error) => {
      console.log('[SignalR] ⏳ A reconectar...', error?.message);
      this.connectionStatus.set('Connecting');
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('[SignalR] ✅ Reconectado! Novo ConnectionId:', connectionId);
      this.connectionStatus.set('Connected');
    });

    this.hubConnection.onclose((error) => {
      console.log('[SignalR] 🔴 Conexão fechada.', error?.message);
      this.connectionStatus.set('Disconnected');
    });
  }

  private updateMarkerOnMap(state: VehicleTrackerState): void {
    if (!this.map || state.latitude == null || state.longitude == null) return;

    const position: L.LatLngExpression = [state.latitude, state.longitude];
    const speed = state.speed || 0;

    if (state.marker) {
      state.marker.setLatLng(position);
      const popupContent = `
        <div style="font-family: 'Inter', sans-serif; color: #1e293b; padding: 4px;">
          <strong style="font-size: 13px;">${state.vehicle.brand} ${state.vehicle.model}</strong><br/>
          <span style="font-size: 11px; background: #e2e8f0; padding: 2px 6px; border-radius: 4px; font-family: monospace; font-weight: 600; display: inline-block; margin: 4px 0;">${state.vehicle.licensePlate}</span><br/>
          <span style="font-size: 11px;">Velocidade: <strong>${Math.round(speed)} km/h</strong></span>
        </div>
      `;
      state.marker.setPopupContent(popupContent);
    } else {
      const vehicleIcon = L.divIcon({
        html: `
          <div class="pulse-marker" style="
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            width: 38px;
            height: 38px;
            border-radius: 50%;
            border: 3px solid #111827;
            box-shadow: 0 0 15px rgba(59, 130, 246, 0.6);
            display: flex;
            align-items: center;
            justify-content: center;
          ">
            <span class="material-icons" style="font-size: 18px;">directions_car</span>
          </div>
        `,
        className: 'custom-leaflet-marker',
        iconSize: [38, 38],
        iconAnchor: [19, 19]
      });

      const popupContent = `
        <div style="font-family: 'Inter', sans-serif; color: #1e293b; padding: 4px;">
          <strong style="font-size: 13px;">${state.vehicle.brand} ${state.vehicle.model}</strong><br/>
          <span style="font-size: 11px; background: #e2e8f0; padding: 2px 6px; border-radius: 4px; font-family: monospace; font-weight: 600; display: inline-block; margin: 4px 0;">${state.vehicle.licensePlate}</span><br/>
          <span style="font-size: 11px;">Velocidade: <strong>${Math.round(speed)} km/h</strong></span>
        </div>
      `;

      state.marker = L.marker(position, { icon: vehicleIcon })
        .addTo(this.map)
        .bindPopup(popupContent);
    }
  }

  private handleTrackingStopped(vehicleId: string): void {
    const states = this.vehiclesState();
    const vid = vehicleId.toString().toLowerCase();
    const index = states.findIndex(s => (s.vehicle.id || '').toLowerCase() === vid);

    if (index !== -1) {
      const state = states[index];
      
      // Remover o marcador correspondente do mapa Leaflet
      if (state.marker) {
        state.marker.remove();
        state.marker = undefined;
      }

      state.latitude = undefined;
      state.longitude = undefined;
      state.speed = undefined;
      state.lastUpdate = undefined;
      state.isSimulating = false;
      state.sessionId = undefined;
      state.status = 'Offline';

      // Atualiza estado selecionado caso seja este
      const currentSelected = this.selectedVehicle();
      if (currentSelected && (currentSelected.vehicle.id || '').toLowerCase() === vid) {
        this.selectedVehicle.set({ ...state });
      }

      this.vehiclesState.set([...states]);
      console.log(`[Tracking] Conexão encerrada para veículo: ${state.vehicle.licensePlate}`);
    }
  }

  private handleTrackingStatusChanged(sessionId: string, vehicleId: string, status: string): void {
    const states = this.vehiclesState();
    const vid = vehicleId.toString().toLowerCase();
    const index = states.findIndex(s => (s.vehicle.id || '').toLowerCase() === vid);

    if (index !== -1) {
      const state = states[index];
      state.sessionId = sessionId;
      state.status = status as any;

      if (status === 'Stopped') {
        this.handleTrackingStopped(vehicleId);
        return;
      }

      // Atualiza estado selecionado caso seja este
      const currentSelected = this.selectedVehicle();
      if (currentSelected && (currentSelected.vehicle.id || '').toLowerCase() === vid) {
        this.selectedVehicle.set({ ...state });
      }

      this.vehiclesState.set([...states]);
      console.log(`[Tracking] Estado da sessão mudou para: ${status} para viatura ${state.vehicle.licensePlate}`);
    }
  }

  private handleLocationUpdate(vehicleId: string, lat: number, lng: number, speed: number): void {
    const states = this.vehiclesState();
    const vid = vehicleId.toString().toLowerCase();
    const index = states.findIndex(s => (s.vehicle.id || '').toLowerCase() === vid);

    console.log('[SignalR] Procurando veículo:', vid, '| Encontrado no índice:', index, '| Total veículos:', states.length);

    if (index !== -1) {
      const state = states[index];
      state.latitude = lat;
      state.longitude = lng;
      state.speed = speed;
      state.lastUpdate = new Date();
      state.status = 'Active';

      // Desenhar/atualizar no mapa
      this.updateMarkerOnMap(state);

      // Se o veículo for o selecionado, re-centra o mapa
      const currentSelected = this.selectedVehicle();
      if (currentSelected && (currentSelected.vehicle.id || '').toLowerCase() === vid) {
        if (this.map) {
          this.map.panTo([lat, lng]);
        }
        this.selectedVehicle.set({ ...state });
      }

      // Notifica o Signal da alteração
      this.vehiclesState.set([...states]);
    }
  }

  selectVehicle(state: VehicleTrackerState): void {
    this.selectedVehicle.set(state);
    if (state.latitude && state.longitude) {
      this.map.setView([state.latitude, state.longitude], 15);
      if (state.marker) {
        state.marker.openPopup();
      }
    } else {
      // Zoom padrão de Luanda se não houver coordenadas registradas
      this.map.setView([-8.815, 13.230], 13);
    }
  }

  startSimulation(state: VehicleTrackerState): void {
    this.stopSimulation(); // Limpa simulação anterior ativa
    this.stopDeviceTracking(); // Limpa rastreio de telemóvel ativo
    
    // Resetar flags de simulação
    const states = this.vehiclesState();
    states.forEach(s => s.isSimulating = false);
    
    state.isSimulating = true;
    this.vehiclesState.set([...states]);
    this.simulationIndex = 0;

    const payload = {
      vehicleId: state.vehicle.id,
      provider: 'simulation'
    };

    this.http.post<any>('/api/tracking/start', payload).subscribe({
      next: (sess) => {
        this.simulationSessionId = sess.id;
        state.sessionId = sess.id;
        state.status = 'Starting';
        this.vehiclesState.set([...this.vehiclesState()]);

        // Enviar primeira coordenada
        this.sendSimulatedLocation(state);

        // Loop a cada 2.5 segundos
        this.simulationIntervalId = setInterval(() => {
          this.sendSimulatedLocation(state);
        }, 2500);
      },
      error: (err) => {
        console.error('Erro ao iniciar sessão de simulação no backend', err);
        state.isSimulating = false;
        this.vehiclesState.set([...this.vehiclesState()]);
      }
    });
  }

  stopSimulation(): void {
    if (this.simulationIntervalId) {
      clearInterval(this.simulationIntervalId);
      this.simulationIntervalId = null;
    }

    const sessId = this.simulationSessionId;
    this.simulationSessionId = null;

    const states = this.vehiclesState();
    states.forEach(s => s.isSimulating = false);
    this.vehiclesState.set([...states]);
    
    const current = this.selectedVehicle();
    if (current) {
      current.isSimulating = false;
      this.selectedVehicle.set({ ...current });
    }

    if (sessId) {
      this.http.post(`/api/tracking/stop/${sessId}`, {}).subscribe({
        next: () => console.log('Sessão de simulação parada no backend.'),
        error: (err) => console.error('Erro ao parar sessão de simulação no backend', err)
      });
    }
  }

  startDeviceTracking(state: VehicleTrackerState): void {
    if (!navigator.geolocation) {
      alert('O seu dispositivo ou navegador não suporta geolocalização.');
      return;
    }

    this.stopSimulation();

    const activeVehiclePlate = this.isTransmitting();
    if (activeVehiclePlate && activeVehiclePlate !== state.vehicle.licensePlate) {
      this.stopDeviceTracking();
    }

    const startTrackingFn = (sessionId: string) => {
      this.activeSessionId.set(sessionId);
      this.isTransmitting.set(state.vehicle.licensePlate);
      localStorage.setItem('frotago_active_tracking_session_id', sessionId);
      
      this.startTime = Date.now();
      this.transmissionTime.set('00:00.000');

      if (this.transmissionInterval) clearInterval(this.transmissionInterval);
      this.transmissionInterval = setInterval(() => {
        const elapsed = Date.now() - this.startTime;
        const minutes = Math.floor(elapsed / 60000);
        const seconds = Math.floor((elapsed % 60000) / 1000);
        const milliseconds = elapsed % 1000;
        
        const padZero = (num: number, size = 2) => num.toString().padStart(size, '0');
        this.transmissionTime.set(
          `${padZero(minutes)}:${padZero(seconds)}.${padZero(milliseconds, 3)}`
        );
      }, 33);

      if (this.watchId !== null) navigator.geolocation.clearWatch(this.watchId);
      this.watchId = navigator.geolocation.watchPosition(
        (position) => {
          const speedKmh = position.coords.speed ? (position.coords.speed * 3.6) : 0;
          const payload = {
            trackingSessionId: sessionId,
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            speed: speedKmh
          };

          this.http.post('/api/tracking/location', payload).subscribe({
            next: () => {
              console.log('GPS do telemóvel enviado com sucesso:', payload);
            },
            error: (err) => {
              console.error('Erro ao enviar localização do telemóvel', err);
            }
          });
        },
        (err) => {
          console.error('Erro ao obter coordenadas do telemóvel', err);
          alert('Erro de GPS: ' + err.message);
          this.stopDeviceTracking();
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 0
        }
      );
    };

    const currentSessionId = this.activeSessionId();
    if (currentSessionId) {
      startTrackingFn(currentSessionId);
    } else {
      const payload = {
        vehicleId: state.vehicle.id,
        provider: 'mobile'
      };

      this.http.post<any>('/api/tracking/start', payload).subscribe({
        next: (sess) => {
          startTrackingFn(sess.id);
        },
        error: (err) => {
          console.error('Erro ao iniciar sessão de tracking', err);
          alert('Erro ao iniciar rastreamento no servidor: ' + (err.error?.message || err.message));
        }
      });
    }
  }

  stopDeviceTracking(): void {
    const activeVehiclePlate = this.isTransmitting();
    const sessionId = this.activeSessionId();
    
    if (this.watchId !== null) {
      navigator.geolocation.clearWatch(this.watchId);
      this.watchId = null;
    }
    if (this.transmissionInterval) {
      clearInterval(this.transmissionInterval);
      this.transmissionInterval = null;
    }
    this.transmissionTime.set('00:00.000');
    this.isTransmitting.set(null);
    this.activeSessionId.set(null);
    localStorage.removeItem('frotago_active_tracking_session_id');

    if (sessionId) {
      this.http.post(`/api/tracking/stop/${sessionId}`, {}).subscribe({
        next: () => console.log('Fim de transmissão notificado com sucesso ao backend.'),
        error: (err) => console.error('Erro ao notificar fim de transmissão', err)
      });
      
      if (activeVehiclePlate) {
        const state = this.vehiclesState().find(s => s.vehicle.licensePlate === activeVehiclePlate);
        if (state && state.vehicle.id) {
          this.handleTrackingStopped(state.vehicle.id);
        }
      }
    }
  }

  private sendSimulatedLocation(state: VehicleTrackerState): void {
    if (this.simulationIndex >= this.luandaSimulationRoute.length) {
      this.simulationIndex = 0; // recomeça trajeto
    }

    const coords = this.luandaSimulationRoute[this.simulationIndex];
    this.simulationIndex++;

    const sessId = this.simulationSessionId;
    if (!sessId) return;

    const payload = {
      trackingSessionId: sessId,
      latitude: coords.lat,
      longitude: coords.lng,
      speed: coords.speed
    };

    this.http.post('/api/tracking/location', payload).subscribe({
      next: () => {
        // Sucesso, a atualização virá pelo WebSocket Hub
      },
      error: (err) => {
        console.error('Erro ao enviar telemetria simulada', err);
      }
    });
  }

  isLocallyActive(state: VehicleTrackerState): boolean {
    if (state.isSimulating) return true;
    return state.status === 'Active' || state.status === 'Starting';
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Disponível';
      case 2: return 'Em Aula';
      case 3: return 'Em Manutenção';
      default: return 'Desconhecido';
    }
  }
}
