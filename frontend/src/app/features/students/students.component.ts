import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { StudentService, Student } from './services/student.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { AuthService } from '../authentication/services/auth.service';

@Component({
  selector: 'app-students',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './students.html',
  styleUrl: './students.css'
})
export class StudentsComponent implements OnInit {
  students = signal<Student[]>([]);
  studentForm: FormGroup;
  showForm = signal(false);
  editingStudentId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalStudents = signal(0);
  activeStudents = signal(0);

  constructor(
    private fb: FormBuilder,
    private studentService: StudentService,
    public authService: AuthService
  ) {
    this.studentForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      identityCardNumber: ['', Validators.required],
      category: [2, Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.studentService.getStudents().subscribe({
      next: (data) => {
        this.students.set(data);
        this.totalStudents.set(data.length);
        this.activeStudents.set(data.filter(s => s.isActive !== false).length);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar alunos.');
      }
    });
  }

  openCreateForm(): void {
    this.editingStudentId.set(null);
    this.studentForm.reset({ category: 2, isActive: true });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(student: Student): void {
    if (!student.id) return;
    this.editingStudentId.set(student.id);
    this.studentForm.patchValue({
      name: student.name,
      email: student.email,
      phoneNumber: student.phoneNumber,
      identityCardNumber: student.identityCardNumber,
      category: student.category,
      isActive: student.isActive !== false
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingStudentId.set(null);
    this.studentForm.reset({ category: 2, isActive: true });
  }

  onSubmit(): void {
    if (this.studentForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formData = this.studentForm.value;
    const currentId = this.editingStudentId();

    if (currentId) {
      this.studentService.updateStudent(currentId, { ...formData, id: currentId }).subscribe({
        next: () => {
          this.successMessage.set('Aluno atualizado com sucesso!');
          this.closeForm();
          this.loadStudents();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para editar alunos.'
            : (err.error?.message || 'Erro ao atualizar aluno.'));
        }
      });
    } else {
      this.studentService.createStudent(formData).subscribe({
        next: () => {
          this.successMessage.set('Aluno registado com sucesso!');
          this.closeForm();
          this.loadStudents();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para registar alunos.'
            : (err.error?.message || 'Erro ao criar aluno.'));
        }
      });
    }
  }

  deleteStudent(student: Student): void {
    if (!student.id) return;
    if (confirm(`Tem certeza que deseja eliminar o aluno ${student.name}?`)) {
      this.studentService.deleteStudent(student.id).subscribe({
        next: () => {
          this.successMessage.set('Aluno eliminado com sucesso!');
          this.loadStudents();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para eliminar alunos.'
            : (err.error?.message || 'Erro ao eliminar aluno.'));
        }
      });
    }
  }

  getCategoryLabel(cat: number): string {
    switch (cat) {
      case 1: return 'Categoria A';
      case 2: return 'Categoria B';
      case 3: return 'Categoria C';
      case 4: return 'Categoria D';
      default: return 'N/A';
    }
  }

  getProgressLabel(status?: number): string {
    switch (status) {
      case 1: return 'Inscrição';
      case 2: return 'Teoria';
      case 3: return 'Prática';
      case 4: return 'Pronto p/ Exame';
      case 5: return 'Encartado';
      default: return 'Inscrição';
    }
  }

  getProgressBadgeClass(status?: number): string {
    switch (status) {
      case 1: return 'status-maintenance';
      case 2: return 'status-in-use';
      case 3: return 'status-available';
      case 4: return 'status-available';
      case 5: return 'status-available';
      default: return 'status-maintenance';
    }
  }
}
