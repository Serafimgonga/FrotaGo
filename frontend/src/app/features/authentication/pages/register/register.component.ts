import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService, RegisterSchoolRequest } from '../../services/auth.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-register',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  currentStep = signal(1); // 1, 2, 3, 4
  hidePassword = signal(true);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  // Forms for each step
  step1Form: FormGroup;
  step2Form: FormGroup;
  step3Form: FormGroup;
  step4Form: FormGroup;

  selectedPlan = signal<string>('Gratuito'); // Default: Gratuito (Trial 30 dias)

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // Passo 1 — Proprietário / Responsável
    this.step1Form = this.fb.group({
      ownerName: ['', [Validators.required, Validators.minLength(3)]],
      gender: ['Masculino', Validators.required],
      phone: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      identityCardNumber: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    });

    // Passo 2 — Escola de Condução (Informação básica, legal, contactos, localização)
    this.step2Form = this.fb.group({
      schoolName: ['', [Validators.required, Validators.minLength(3)]],
      shortName: [''],
      slug: ['', [Validators.required, Validators.pattern('^[a-z0-9-]+$')]],
      nif: ['', [Validators.required, Validators.minLength(5)]],
      licenseNumber: ['', Validators.required],
      licenseIssuer: ['INATRO', Validators.required],
      licenseIssueDate: [''],
      schoolPhone: [''],
      schoolEmail: ['', Validators.email],
      website: [''],
      province: ['Luanda', Validators.required],
      municipality: ['Talatona', Validators.required],
      address: ['', Validators.required],
      latitude: [null],
      longitude: [null]
    });

    // Passo 3 — Primeira Unidade (Sede)
    this.step3Form = this.fb.group({
      branchName: ['Sede Talatona', Validators.required],
      branchPhone: [''],
      branchAddress: ['']
    });

    // Passo 4 — Escolha do Plano
    this.step4Form = this.fb.group({
      plan: ['Gratuito', Validators.required]
    });
  }

  onSchoolNameChange(): void {
    const name = this.step2Form.get('schoolName')?.value || '';
    if (!this.step2Form.get('slug')?.touched) {
      const generatedSlug = name
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/(^-|-$)+/g, '');
      this.step2Form.patchValue({ slug: generatedSlug });
    }
    if (!this.step2Form.get('shortName')?.touched && name.length > 0) {
      const initials = name.split(' ').map((w: string) => w[0]).join('').toUpperCase();
      if (initials.length <= 6) {
        this.step2Form.patchValue({ shortName: initials });
      }
    }
  }

  nextStep(): void {
    this.errorMessage.set(null);

    if (this.currentStep() === 1) {
      if (this.step1Form.invalid) {
        this.step1Form.markAllAsTouched();
        return;
      }
      if (this.step1Form.value.password !== this.step1Form.value.confirmPassword) {
        this.errorMessage.set('As palavras-passe não coincidem.');
        return;
      }
      this.currentStep.set(2);
    } else if (this.currentStep() === 2) {
      if (this.step2Form.invalid) {
        this.step2Form.markAllAsTouched();
        return;
      }
      // Replicar telefone/morada para o passo 3 se vazios
      if (!this.step3Form.value.branchPhone) {
        this.step3Form.patchValue({ branchPhone: this.step1Form.value.phone });
      }
      if (!this.step3Form.value.branchAddress) {
        this.step3Form.patchValue({ branchAddress: this.step2Form.value.address });
      }
      this.currentStep.set(3);
    } else if (this.currentStep() === 3) {
      if (this.step3Form.invalid) {
        this.step3Form.markAllAsTouched();
        return;
      }
      this.currentStep.set(4);
    }
  }

  prevStep(): void {
    this.errorMessage.set(null);
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  selectPlan(planName: string): void {
    this.selectedPlan.set(planName);
    this.step4Form.patchValue({ plan: planName });
  }

  onSubmit(): void {
    if (this.isLoading()) return;

    this.errorMessage.set(null);
    this.isLoading.set(true);

    const s1 = this.step1Form.value;
    const s2 = this.step2Form.value;
    const s3 = this.step3Form.value;
    const s4 = this.step4Form.value;

    const payload: RegisterSchoolRequest = {
      // Step 1
      ownerName: s1.ownerName,
      gender: s1.gender,
      phone: s1.phone,
      email: s1.email,
      identityCardNumber: s1.identityCardNumber,
      password: s1.password,

      // Step 2
      schoolName: s2.schoolName,
      shortName: s2.shortName || s2.schoolName,
      slug: s2.slug,
      nif: s2.nif,
      licenseNumber: s2.licenseNumber,
      licenseIssuer: s2.licenseIssuer,
      licenseIssueDate: s2.licenseIssueDate ? s2.licenseIssueDate : undefined,
      schoolPhone: s2.schoolPhone || s1.phone,
      schoolEmail: s2.schoolEmail || s1.email,
      website: s2.website,
      province: s2.province,
      municipality: s2.municipality,
      address: s2.address,
      latitude: s2.latitude ? Number(s2.latitude) : undefined,
      longitude: s2.longitude ? Number(s2.longitude) : undefined,

      // Step 3
      branchName: s3.branchName,
      branchPhone: s3.branchPhone,
      branchAddress: s3.branchAddress,

      // Step 4
      plan: s4.plan || this.selectedPlan()
    };

    this.authService.registerSchool(payload).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao registar escola. Verifique os dados introduzidos.');
      }
    });
  }
}
