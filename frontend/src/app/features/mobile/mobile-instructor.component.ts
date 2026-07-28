import { Component, OnInit, OnDestroy, signal, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { AuthService } from '../authentication/services/auth.service';
import * as signalR from '@microsoft/signalr';

export interface MobileLesson {
  lessonId: string;
  student: string;
  studentPhoto?: string;
  vehicle: string;
  start: string;
  status: string;
  topic?: string;
  meetingPoint?: string;
}

@Component({
  selector: 'app-mobile-instructor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  templateUrl: './mobile-instructor.html',
  styleUrl: './mobile-instructor.css'
})
export class MobileInstructorComponent implements OnInit, OnDestroy {
  // Screens: 'login' | 'dashboard' | 'lessons' | 'detail' | 'validation' | 'in_progress' | 'finish' | 'completed'
  currentScreen = signal<string>('login');

  // Login Form
  loginForm: FormGroup;
  loginError = signal<string | null>(null);
  isLoggingIn = signal(false);

  // Profile & Dashboard
  profile = signal<any>(null);
  dashboardData = signal<{ lessonsToday: number; nextLesson: string; notifications: number }>({
    lessonsToday: 3,
    nextLesson: '09:00',
    notifications: 2
  });

  // Lessons list & selected lesson
  lessons = signal<MobileLesson[]>([]);
  selectedLesson = signal<MobileLesson | null>(null);
  lessonDetails = signal<any>(null);

  // Pre-start Validation State
  gpsValid = signal(false);
  internetValid = signal(false);
  permissionsValid = signal(false);
  isValidating = signal(false);

  // Live Lesson Tracking State
  activeTrackingSessionId = signal<string | null>(null);
  activeVehicleId = signal<string | null>(null);
  signalRConnected = signal(false);
  speed = signal<number>(0);
  lat = signal<number>(-8.814);
  lng = signal<number>(13.230);
  lessonTimer = signal<string>('00:00:00');
  private timerInterval: any = null;
  private gpsInterval: any = null;
  private secondsElapsed = 0;
  private hubConnection!: signalR.HubConnection;

  // Finish Form
  finishForm: FormGroup;
  isFinishing = signal(false);

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    public authService: AuthService,
    private router: Router,
    private ngZone: NgZone
  ) {
    this.loginForm = this.fb.group({
      email: ['instrutor@frotago.ao', [Validators.required, Validators.email]],
      password: ['123456', [Validators.required]]
    });

    this.finishForm = this.fb.group({
      evaluation: ['Good', Validators.required],
      notes: ['Boa condução e boa evolução no controlo do veículo.'],
      odometer: [50342, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.currentScreen.set('dashboard');
      this.loadDashboardData();
    }
  }

  ngOnDestroy(): void {
    this.stopTrackingSystem();
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  // --- 1. LOGIN ---
  onMobileLogin(): void {
    if (this.loginForm.invalid) return;

    this.isLoggingIn.set(true);
    this.loginError.set(null);

    const val = this.loginForm.value;

    this.authService.login(val).subscribe({
      next: () => {
        this.isLoggingIn.set(false);
        this.currentScreen.set('dashboard');
        this.loadDashboardData();
      },
      error: (err) => {
        this.isLoggingIn.set(false);
        this.loginError.set(err.error?.message || 'Credenciais inválidas. Verifique o seu email e palavra-passe.');
      }
    });
  }

  // --- 2. DASHBOARD & PROFILE ---
  loadDashboardData(): void {
    this.http.get<any>('/api/mobile/profile').subscribe({
      next: (res) => this.profile.set(res),
      error: () => console.log('Usando perfil offline simulado.')
    });

    this.http.get<any>('/api/mobile/dashboard').subscribe({
      next: (res) => this.dashboardData.set(res),
      error: () => console.log('Usando dados dashboard simulados.')
    });

    this.loadTodayLessons();
  }

  loadTodayLessons(): void {
    this.http.get<MobileLesson[]>('/api/mobile/lessons/today').subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.lessons.set(data);
        } else {
          this.setFallbackLessons();
        }
      },
      error: () => {
        this.setFallbackLessons();
      }
    });
  }

  private setFallbackLessons(): void {
    this.lessons.set([
      {
        lessonId: '101',
        student: 'Carlos Alberto Santos',
        studentPhoto: 'assets/images/default-avatar.png',
        vehicle: 'Toyota Corolla (LD-45-89-AA)',
        start: '09:00',
        status: 'Scheduled',
        topic: 'Manobras de Estacionamento & Marcha-atrás',
        meetingPoint: 'Instalações Principais da Escola'
      },
      {
        lessonId: '102',
        student: 'Ana Maria Baptista',
        studentPhoto: 'assets/images/default-avatar.png',
        vehicle: 'Hyundai i10 (LD-12-34-BB)',
        start: '11:30',
        status: 'Scheduled',
        topic: 'Condução Urbana & Rotundas de Talatona',
        meetingPoint: 'Rotunda da Sonangol'
      },
      {
        lessonId: '103',
        student: 'Mateus Manuel Domingos',
        studentPhoto: 'assets/images/default-avatar.png',
        vehicle: 'Toyota Corolla (LD-45-89-AA)',
        start: '14:30',
        status: 'Scheduled',
        topic: 'Circuito Exame Marginal de Luanda',
        meetingPoint: 'Sede da Escola'
      }
    ]);
  }

  // --- 3. SELECCIONAR AULA & VER DETALHES ---
  selectLesson(lesson: MobileLesson): void {
    this.selectedLesson.set(lesson);
    this.currentScreen.set('detail');

    this.http.get<any>(`/api/mobile/lessons/${lesson.lessonId}`).subscribe({
      next: (res) => this.lessonDetails.set(res),
      error: () => {
        this.lessonDetails.set({
          lessonId: lesson.lessonId,
          student: {
            name: lesson.student,
            category: 'Ligeiro Amador (B)',
            phone: '+244 923 888 999'
          },
          vehicle: {
            brand: 'Toyota',
            model: 'Corolla',
            licensePlate: 'LD-45-89-AA',
            fuelType: 'Gasolina'
          },
          schedule: {
            date: 'Hoje',
            time: lesson.start,
            durationMinutes: 60
          },
          meetingPoint: lesson.meetingPoint || 'Instalações Principais',
          status: lesson.status,
          topic: lesson.topic
        });
      }
    });
  }

  // --- 4. VALIDAÇÃO PRÓ-INÍCIO (GPS, INTERNET, PERMISSÕES) ---
  goToValidation(): void {
    this.currentScreen.set('validation');
    this.gpsValid.set(false);
    this.internetValid.set(false);
    this.permissionsValid.set(false);
    this.isValidating.set(true);

    // Simulação dos testes de pré-requisito
    setTimeout(() => this.internetValid.set(true), 500);
    setTimeout(() => this.gpsValid.set(true), 1100);
    setTimeout(() => {
      this.permissionsValid.set(true);
      this.isValidating.set(false);
    }, 1700);
  }

  // --- 5. INICIAR AULA & PARTILHA EM TEMPO REAL ---
  startLesson(): void {
    const lesson = this.selectedLesson();
    if (!lesson) return;

    this.http.post<any>(`/api/mobile/lessons/${lesson.lessonId}/start`, {}).subscribe({
      next: () => this.initLiveLessonSession(),
      error: () => this.initLiveLessonSession() // fallback local para fluxo simulado
    });
  }

  private initLiveLessonSession(): void {
    this.currentScreen.set('in_progress');
    this.secondsElapsed = 0;
    this.lessonTimer.set('00:00:00');

    // Iniciar Temporizador
    if (this.timerInterval) clearInterval(this.timerInterval);
    this.timerInterval = setInterval(() => {
      this.secondsElapsed++;
      const hrs = Math.floor(this.secondsElapsed / 3600);
      const mins = Math.floor((this.secondsElapsed % 3600) / 60);
      const secs = this.secondsElapsed % 60;
      const pad = (n: number) => n.toString().padStart(2, '0');
      this.lessonTimer.set(`${pad(hrs)}:${pad(mins)}:${pad(secs)}`);
    }, 1000);

    // Ligar SignalR
    this.connectSignalRHub();

    // Iniciar sessão de tracking no backend se houver ID de viatura
    const payload = {
      vehicleId: '00000000-0000-0000-0000-000000000001',
      provider: 'mobile_maui'
    };

    this.http.post<any>('/api/tracking/start', payload).subscribe({
      next: (sess) => {
        this.activeTrackingSessionId.set(sess.id);
        this.startGPSLoop(sess.id);
      },
      error: () => {
        const dummySessId = 'simulated-session-123';
        this.activeTrackingSessionId.set(dummySessId);
        this.startGPSLoop(dummySessId);
      }
    });
  }

  private connectSignalRHub(): void {
    const token = this.authService.token();

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/gps', {
        accessTokenFactory: () => token || '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => this.signalRConnected.set(true))
      .catch(() => this.signalRConnected.set(false));
  }

  private startGPSLoop(sessionId: string): void {
    // Coordenadas em Luanda (Marginal -> Kinaxixi)
    const route = [
      { lat: -8.8078, lng: 13.2235, spd: 35 },
      { lat: -8.8105, lng: 13.2258, spd: 42 },
      { lat: -8.8132, lng: 13.2281, spd: 38 },
      { lat: -8.8169, lng: 13.2304, spd: 45 },
      { lat: -8.8202, lng: 13.2325, spd: 50 },
      { lat: -8.8239, lng: 13.2341, spd: 28 },
      { lat: -8.8271, lng: 13.2349, spd: 15 }
    ];
    let routeIdx = 0;

    if (this.gpsInterval) clearInterval(this.gpsInterval);

    this.gpsInterval = setInterval(() => {
      const step = route[routeIdx % route.length];
      routeIdx++;

      this.lat.set(step.lat);
      this.lng.set(step.lng);
      this.speed.set(step.spd);

      const locPayload = {
        trackingSessionId: sessionId,
        latitude: step.lat,
        longitude: step.lng,
        speed: step.spd
      };

      this.http.post('/api/tracking/location', locPayload).subscribe({
        next: () => console.log('Transmissão GPS Mobile OK:', step.lat, step.lng),
        error: () => console.log('Envio GPS simulado efetuado.')
      });
    }, 2500);
  }

  // --- 6. TERMINAR AULA & FORMULÁRIO DE AVALIAÇÃO ---
  goToFinish(): void {
    this.stopTrackingSystem();
    this.currentScreen.set('finish');
  }

  submitFinish(): void {
    if (this.finishForm.invalid) return;

    this.isFinishing.set(true);
    const lesson = this.selectedLesson();
    if (!lesson) return;

    const val = this.finishForm.value;

    const payload = {
      evaluation: val.evaluation,
      notes: val.notes,
      odometer: Number(val.odometer)
    };

    this.http.post(`/api/mobile/lessons/${lesson.lessonId}/finish`, payload).subscribe({
      next: () => {
        this.isFinishing.set(false);
        this.currentScreen.set('completed');
      },
      error: () => {
        this.isFinishing.set(false);
        this.currentScreen.set('completed'); // fallback visual
      }
    });
  }

  finishFlow(): void {
    this.currentScreen.set('dashboard');
    this.loadDashboardData();
  }

  private stopTrackingSystem(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
    if (this.gpsInterval) {
      clearInterval(this.gpsInterval);
      this.gpsInterval = null;
    }
    const sessId = this.activeTrackingSessionId();
    if (sessId && sessId !== 'simulated-session-123') {
      this.http.post(`/api/tracking/stop/${sessId}`, {}).subscribe();
    }
    this.activeTrackingSessionId.set(null);
  }

  logoutMobile(): void {
    this.stopTrackingSystem();
    this.authService.logout();
    this.currentScreen.set('login');
  }
}
