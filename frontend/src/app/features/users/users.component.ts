import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService, UserItem } from './services/user.service';
import { AuthService } from '../authentication/services/auth.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-users',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './users.html',
  styleUrl: './users.css'
})
export class UsersComponent implements OnInit {
  users = signal<UserItem[]>([]);
  inviteForm: FormGroup;
  showForm = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  invitationLink = signal<string | null>(null);

  // Stats
  totalUsers = signal(0);
  activeUsers = signal(0);
  pendingUsers = signal(0);

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    public authService: AuthService
  ) {
    this.inviteForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      role: [5, Validators.required] // Default: Instructor
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    const user = this.authService.currentUser();
    if (!user?.schoolId) return;

    this.userService.getUsers(user.schoolId).subscribe({
      next: (data) => {
        this.users.set(data);
        this.calculateStats(data);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar utilizadores.');
      }
    });
  }

  calculateStats(list: UserItem[]): void {
    this.totalUsers.set(list.length);
    this.activeUsers.set(list.filter(u => u.isActive).length);
    this.pendingUsers.set(list.filter(u => !u.isActive).length);
  }

  openInviteForm(): void {
    this.inviteForm.reset({ role: 5 });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.invitationLink.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.inviteForm.reset();
    this.invitationLink.set(null);
  }

  onSubmit(): void {
    if (this.inviteForm.invalid) return;

    const user = this.authService.currentUser();
    if (!user?.schoolId) return;

    this.errorMessage.set(null);
    this.successMessage.set(null);

    const request = {
      ...this.inviteForm.value,
      schoolId: user.schoolId
    };

    this.userService.inviteUser(request).subscribe({
      next: (res) => {
        const link = `${window.location.origin}/accept-invitation?token=${res.token}`;
        this.invitationLink.set(link);
        this.successMessage.set('Convite gerado com sucesso! Partilhe o link abaixo com o funcionário.');
        this.loadUsers();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao convidar utilizador.');
      }
    });
  }

  copyLink(): void {
    const link = this.invitationLink();
    if (link) {
      navigator.clipboard.writeText(link);
      this.successMessage.set('Link copiado para a área de transferência!');
    }
  }

  toggleStatus(userItem: UserItem): void {
    this.userService.toggleUserStatus(userItem.id).subscribe({
      next: () => {
        this.successMessage.set(`Estado do utilizador "${userItem.name}" alterado com sucesso.`);
        this.loadUsers();
      },
      error: () => {
        this.errorMessage.set('Erro ao alterar estado do utilizador.');
      }
    });
  }

  getRoleLabel(role: string): string {
    switch (role) {
      case 'SuperAdmin': return 'Super Admin';
      case 'SchoolOwner': return 'Proprietário';
      case 'SchoolAdmin': return 'Administrador';
      case 'Receptionist': return 'Rececionista';
      case 'Instructor': return 'Instrutor';
      case 'Financial': return 'Financeiro';
      default: return role;
    }
  }

  getRoleClass(role: string): string {
    switch (role) {
      case 'SchoolOwner': return 'role-owner';
      case 'SchoolAdmin': return 'role-admin';
      case 'Receptionist': return 'role-receptionist';
      case 'Instructor': return 'role-instructor';
      case 'Financial': return 'role-financial';
      default: return '';
    }
  }
}
