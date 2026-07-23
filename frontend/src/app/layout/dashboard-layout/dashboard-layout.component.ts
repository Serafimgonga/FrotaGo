import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../features/authentication/services/auth.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { PendingApprovalComponent } from '../../features/authentication/pages/pending-approval/pending-approval.component';

@Component({
  selector: 'app-dashboard-layout',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatIconModule,
    MatButtonModule,
    PendingApprovalComponent
  ],
  templateUrl: './dashboard-layout.html',
  styleUrl: './dashboard-layout.css'
})
export class DashboardLayoutComponent {
  authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}
