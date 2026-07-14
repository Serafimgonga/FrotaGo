import { Routes } from '@angular/router';
import { LoginComponent } from './features/authentication/pages/login/login.component';
import { DashboardLayoutComponent } from './layout/dashboard-layout/dashboard-layout.component';
import { VehiclesComponent } from './features/vehicles/vehicles.component';
import { InstructorsComponent } from './features/instructors/instructors.component';
import { StudentsComponent } from './features/students/students.component';
import { LessonsComponent } from './features/lessons/lessons.component';
import { MaintenanceComponent } from './features/maintenance/maintenance.component';
import { FuelComponent } from './features/fuel/fuel.component';
import { DocumentsComponent } from './features/documents/documents.component';
import { TrackingComponent } from './features/tracking/tracking.component';
import { LandingComponent } from './features/landing/landing.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'login', component: LoginComponent },
  {
    path: 'dashboard',
    component: DashboardLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'vehicles', component: VehiclesComponent },
      { path: 'instructors', component: InstructorsComponent },
      { path: 'students', component: StudentsComponent },
      { path: 'lessons', component: LessonsComponent },
      { path: 'maintenance', component: MaintenanceComponent },
      { path: 'fuel', component: FuelComponent },
      { path: 'documents', component: DocumentsComponent },
      { path: 'tracking', component: TrackingComponent },
      { path: '', redirectTo: 'vehicles', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '' }
];
