import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { InstructorService, Instructor } from './services/instructor.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../authentication/services/auth.service';

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
  editingInstructorId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalInstructors = signal(0);
  activeInstructors = signal(0);

  constructor(
    private fb: FormBuilder,
    private instructorService: InstructorService,
    public authService: AuthService
  ) {
    this.instructorForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      licenseNumber: ['', Validators.required],
      isActive: [true]
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

  openCreateForm(): void {
    this.editingInstructorId.set(null);
    this.instructorForm.reset({ isActive: true });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(instructor: Instructor): void {
    if (!instructor.id) return;
    this.editingInstructorId.set(instructor.id);
    this.instructorForm.patchValue({
      name: instructor.name,
      email: instructor.email,
      phoneNumber: instructor.phoneNumber,
      licenseNumber: instructor.licenseNumber,
      isActive: instructor.isActive !== false
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingInstructorId.set(null);
    this.instructorForm.reset({ isActive: true });
  }

  onSubmit(): void {
    if (this.instructorForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formData = this.instructorForm.value;
    const currentId = this.editingInstructorId();

    if (currentId) {
      this.instructorService.updateInstructor(currentId, { ...formData, id: currentId }).subscribe({
        next: () => {
          this.successMessage.set('Instrutor atualizado com sucesso!');
          this.closeForm();
          this.loadInstructors();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para editar instrutores.'
            : (err.error?.message || 'Erro ao atualizar instrutor.'));
        }
      });
    } else {
      this.instructorService.createInstructor(formData).subscribe({
        next: () => {
          this.successMessage.set('Instrutor registado com sucesso!');
          this.closeForm();
          this.loadInstructors();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para registar instrutores.'
            : (err.error?.message || 'Erro ao criar instrutor.'));
        }
      });
    }
  }

  deleteInstructor(instructor: Instructor): void {
    if (!instructor.id) return;
    if (confirm(`Tem certeza que deseja eliminar o instrutor ${instructor.name}?`)) {
      this.instructorService.deleteInstructor(instructor.id).subscribe({
        next: () => {
          this.successMessage.set('Instrutor eliminado com sucesso!');
          this.loadInstructors();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para eliminar instrutores.'
            : (err.error?.message || 'Erro ao eliminar instrutor.'));
        }
      });
    }
  }
}
