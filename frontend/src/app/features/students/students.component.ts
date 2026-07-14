import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { StudentService, Student } from './services/student.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

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
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalStudents = signal(0);
  activeStudents = signal(0);

  constructor(
    private fb: FormBuilder,
    private studentService: StudentService
  ) {
    this.studentForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      identityCardNumber: ['', Validators.required],
      category: [2, Validators.required]
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

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    if (!this.showForm()) {
      this.studentForm.reset({ category: 2 });
    }
  }

  onSubmit(): void {
    if (this.studentForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.studentService.createStudent(this.studentForm.value).subscribe({
      next: () => {
        this.successMessage.set('Aluno registado com sucesso!');
        this.toggleForm();
        this.loadStudents();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao criar aluno.');
      }
    });
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
}
