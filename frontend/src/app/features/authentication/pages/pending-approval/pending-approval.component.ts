import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';

@Component({
  selector: 'app-pending-approval',
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './pending-approval.html',
  styleUrl: './pending-approval.css'
})
export class PendingApprovalComponent {
  authService = inject(AuthService);
  router = inject(Router);

  showPlanSelector = signal(false);
  selectedPlan = signal<string>('Gratuito');
  isUpdatingPlan = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  logout(): void {
    this.authService.logout();
  }

  checkStatus(): void {
    window.location.reload();
  }

  togglePlanSelector(): void {
    this.showPlanSelector.update(v => !v);
    this.successMessage.set(null);
    this.errorMessage.set(null);
  }

  selectPlan(planName: string): void {
    const schoolId = this.authService.currentUser()?.schoolId;
    if (!schoolId) {
      this.errorMessage.set('Identificador da escola não encontrado.');
      return;
    }

    this.isUpdatingPlan.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.authService.updateSchoolPlan(schoolId, planName).subscribe({
      next: () => {
        this.isUpdatingPlan.set(false);
        this.selectedPlan.set(planName);
        this.successMessage.set(`Plano alterado com sucesso para ${planName}!`);
      },
      error: (err) => {
        this.isUpdatingPlan.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao alterar o plano.');
      }
    });
  }
}
