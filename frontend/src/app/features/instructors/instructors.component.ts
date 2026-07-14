import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { InstructorService, Instructor } from './services/instructor.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-instructors',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './instructors.html',
  styleUrl: './instructors.css'
})
export class InstructorsComponent implements OnInit {
  instructors = signal<Instructor[]>([]);
  instructorForm: FormGroup;
  showForm = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalInstructors = signal(0);
  activeInstructors = signal(0);

  constructor(
    private fb: FormBuilder,
    private instructorService: InstructorService
  ) {
    this.instructorForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      licenseNumber: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadInstructors();
  }

  loadInstructors(): void {
    this.instructorService.getInstructors().subscribe({
      next: (data) => {
        this.instructors.set(data);
        this.totalInstructors.set(data.length);
        this.activeInstructors.set(data.filter(i => i.isActive !== false).length);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar instrutores.');
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    if (!this.showForm()) {
      this.instructorForm.reset();
    }
  }

  onSubmit(): void {
    if (this.instructorForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.instructorService.createInstructor(this.instructorForm.value).subscribe({
      next: () => {
        this.successMessage.set('Instrutor registado com sucesso!');
        this.toggleForm();
        this.loadInstructors();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao criar instrutor.');
      }
    });
  }
}
