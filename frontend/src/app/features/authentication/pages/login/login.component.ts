import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  loginForm: FormGroup;
  registerForm: FormGroup;
  isRegister = signal(false);
  hidePassword = signal(true);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: [1, Validators.required] // 1 = Administrador
    });
  }

  toggleMode(): void {
    this.isRegister.update(val => !val);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  onSubmit(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (this.isRegister()) {
      if (this.registerForm.invalid) return;
      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          this.successMessage.set('Registo efetuado com sucesso! Faça login.');
          this.isRegister.set(false);
          this.loginForm.patchValue({
            email: this.registerForm.value.email,
            password: ''
          });
        },
        error: err => {
          this.errorMessage.set(err.error?.message || 'Erro ao efetuar registo.');
        }
      });
    } else {
      if (this.loginForm.invalid) return;
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: err => {
          this.errorMessage.set(err.error?.message || 'Credenciais inválidas.');
        }
      });
    }
  }
}
