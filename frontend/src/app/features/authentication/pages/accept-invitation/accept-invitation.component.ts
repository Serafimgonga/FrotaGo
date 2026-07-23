import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../users/services/user.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-accept-invitation',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './accept-invitation.html',
  styleUrl: './accept-invitation.css'
})
export class AcceptInvitationComponent implements OnInit {
  form: FormGroup;
  token = signal<string>('');
  hidePassword = signal(true);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService
  ) {
    this.form = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    const extractToken = (): string => {
      const fromRoute = this.route.snapshot.queryParamMap.get('token');
      if (fromRoute) return fromRoute;
      if (typeof window !== 'undefined') {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get('token') || '';
      }
      return '';
    };

    const tokenFound = extractToken();
    if (tokenFound) {
      this.token.set(tokenFound);
      this.errorMessage.set(null);
    } else {
      this.errorMessage.set('Token de convite inválido ou em falta. Contacte o administrador da sua escola.');
    }

    this.route.queryParamMap.subscribe(params => {
      const t = params.get('token') || extractToken();
      if (t) {
        this.token.set(t);
        this.errorMessage.set(null);
      }
    });
  }

  onSubmit(): void {
    const currentToken = this.token();
    if (!currentToken) {
      this.errorMessage.set('Token de convite não encontrado no URL. Verifique o link recebido.');
      return;
    }

    const password = this.form.get('password')?.value;
    const confirmPassword = this.form.get('confirmPassword')?.value;

    if (!password || password.trim().length === 0) {
      this.errorMessage.set('Por favor introduza a palavra-passe.');
      return;
    }

    if (password.length < 6) {
      this.errorMessage.set('A palavra-passe deve ter no mínimo 6 caracteres.');
      return;
    }

    if (!confirmPassword || confirmPassword.trim().length === 0) {
      this.errorMessage.set('Por favor confirme a palavra-passe.');
      return;
    }

    if (password !== confirmPassword) {
      this.errorMessage.set('As palavras-passe não coincidem. Verifique os campos.');
      return;
    }

    this.errorMessage.set(null);
    this.isLoading.set(true);

    this.userService.acceptInvitation({ token: currentToken, password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.successMessage.set('Conta ativada com sucesso! A redirecionar para o login...');
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao ativar conta. Convite inválido ou expirado.');
      }
    });
  }
}
