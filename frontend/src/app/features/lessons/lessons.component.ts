import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { LessonService, Lesson } from './services/lesson.service';
import { InstructorService, Instructor } from '../instructors/services/instructor.service';
import { StudentService, Student } from '../students/services/student.service';
import { VehicleService, Vehicle } from '../vehicles/services/vehicle.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-lessons',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './lessons.html',
  styleUrl: './lessons.css'
})
export class LessonsComponent implements OnInit {
  lessons = signal<Lesson[]>([]);
  instructors = signal<Instructor[]>([]);
  students = signal<Student[]>([]);
  vehicles = signal<Vehicle[]>([]);
  lessonForm: FormGroup;
  showForm = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalLessons = signal(0);
  scheduledLessons = signal(0);
  completedLessons = signal(0);

  constructor(
    private fb: FormBuilder,
    private lessonService: LessonService,
    private instructorService: InstructorService,
    private studentService: StudentService,
    private vehicleService: VehicleService
  ) {
    this.lessonForm = this.fb.group({
      studentId: ['', Validators.required],
      instructorId: ['', Validators.required],
      vehicleId: ['', Validators.required],
      scheduledDate: ['', Validators.required],
      durationMinutes: [60, [Validators.required, Validators.min(15)]],
      topic: ['', Validators.required],
      observations: ['']
    });
  }

  ngOnInit(): void {
    this.loadLessons();
    this.loadDropdowns();
  }

  loadDropdowns(): void {
    this.instructorService.getInstructors().subscribe({
      next: (data) => this.instructors.set(data)
    });
    this.studentService.getStudents().subscribe({
      next: (data) => this.students.set(data)
    });
    this.vehicleService.getVehicles().subscribe({
      next: (data) => this.vehicles.set(data)
    });
  }

  loadLessons(): void {
    this.lessonService.getLessons().subscribe({
      next: (data) => {
        this.lessons.set(data);
        this.totalLessons.set(data.length);
        this.scheduledLessons.set(data.filter(l => l.status === 1).length);
        this.completedLessons.set(data.filter(l => l.status === 2).length);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar aulas.');
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    if (!this.showForm()) {
      this.lessonForm.reset({ durationMinutes: 60 });
    }
  }

  onSubmit(): void {
    if (this.lessonForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.lessonService.createLesson(this.lessonForm.value).subscribe({
      next: () => {
        this.successMessage.set('Aula registada com sucesso!');
        this.toggleForm();
        this.loadLessons();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao criar aula.');
      }
    });
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Agendada';
      case 2: return 'Realizada';
      case 3: return 'Cancelada';
      case 4: return 'Faltou';
      default: return 'N/A';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-scheduled';
      case 2: return 'status-completed';
      case 3: return 'status-cancelled';
      case 4: return 'status-missed';
      default: return '';
    }
  }
}
