import { Component, OnInit, OnDestroy, signal, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { VehicleService, Vehicle } from '../vehicles/services/vehicle.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
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

  vehiclesState = signal<VehicleTrackerState[]>([]);
  selectedVehicle = signal<VehicleTrackerState | null>(null);
  isLoading = signal(true);
  connectionStatus = signal<'Connected' | 'Disconnected' | 'Connecting'>('Disconnected');
  isTransmitting = signal<string | null>(null); // Armazena a matrícula do veículo em transmissão
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
    private http: HttpClient
  ) {}

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
          isSimulating: false
        }));
        this.vehiclesState.set(states);
        this.isLoading.set(false);
        this.connectSignalR();
      },
      error: (err) => {
        console.error('Erro ao carregar veículos para rastreio', err);
        this.isLoading.set(false);
      }
    });
  }

  private connectSignalR(): void {
    this.connectionStatus.set('Connecting');
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`http://${window.location.hostname}:5073/hubs/gps`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('LocationUpdated', (vehicleId: string, lat: number, lng: number, speed: number) => {
      this.handleLocationUpdate(vehicleId, lat, lng, speed);
    });

    this.hubConnection.start()
      .then(() => {
        this.connectionStatus.set('Connected');
      })
      .catch((err) => {
        console.error('Erro ao conectar ao SignalR', err);
        this.connectionStatus.set('Disconnected');
      });
  }

  private handleLocationUpdate(vehicleId: string, lat: number, lng: number, speed: number): void {
    const states = this.vehiclesState();
    const index = states.findIndex(s => s.vehicle.id === vehicleId);

    if (index !== -1) {
      const state = states[index];
      state.latitude = lat;
      state.longitude = lng;
      state.speed = speed;
      state.lastUpdate = new Date();

      // Criar ou atualizar marcador no Leaflet
      const position: L.LatLngExpression = [lat, lng];

      if (state.marker) {
        state.marker.setLatLng(position);
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

      // Se o veículo for o selecionado, re-centra o mapa
      const currentSelected = this.selectedVehicle();
      if (currentSelected && currentSelected.vehicle.id === vehicleId) {
        this.map.panTo(position);
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

    // Enviar primeira coordenada
    this.sendSimulatedLocation(state);

    // Loop a cada 2.5 segundos
    this.simulationIntervalId = setInterval(() => {
      this.sendSimulatedLocation(state);
    }, 2500);
  }

  stopSimulation(): void {
    if (this.simulationIntervalId) {
      clearInterval(this.simulationIntervalId);
      this.simulationIntervalId = null;
    }
    const states = this.vehiclesState();
    states.forEach(s => s.isSimulating = false);
    this.vehiclesState.set([...states]);
    
    const current = this.selectedVehicle();
    if (current) {
      current.isSimulating = false;
      this.selectedVehicle.set({ ...current });
    }
  }

  startDeviceTracking(state: VehicleTrackerState): void {
    if (!navigator.geolocation) {
      alert('O seu dispositivo ou navegador não suporta geolocalização.');
      return;
    }

    this.stopSimulation();
    this.stopDeviceTracking();

    this.isTransmitting.set(state.vehicle.licensePlate);

    this.watchId = navigator.geolocation.watchPosition(
      (position) => {
        const speedKmh = position.coords.speed ? (position.coords.speed * 3.6) : 0;
        const payload = {
          licensePlate: state.vehicle.licensePlate,
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          speed: speedKmh
        };

        this.http.post(`http://${window.location.hostname}:5073/api/gps/track`, payload).subscribe({
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
  }

  stopDeviceTracking(): void {
    if (this.watchId !== null) {
      navigator.geolocation.clearWatch(this.watchId);
      this.watchId = null;
    }
    this.isTransmitting.set(null);
  }

  private sendSimulatedLocation(state: VehicleTrackerState): void {
    if (this.simulationIndex >= this.luandaSimulationRoute.length) {
      this.simulationIndex = 0; // recomeça trajeto
    }

    const coords = this.luandaSimulationRoute[this.simulationIndex];
    this.simulationIndex++;

    const payload = {
      licensePlate: state.vehicle.licensePlate,
      latitude: coords.lat,
      longitude: coords.lng,
      speed: coords.speed
    };

    // Submete via HTTP POST para o endpoint REST que faz o broadcast SignalR e salva histórico
    this.http.post(`http://${window.location.hostname}:5073/api/gps/track`, payload).subscribe({
      next: () => {
        // Sucesso, a atualização virá pelo WebSocket Hub
      },
      error: (err) => {
        console.error('Erro ao enviar telemetria simulada', err);
      }
    });
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
