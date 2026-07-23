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
import { AuthService } from '../authentication/services/auth.service';

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

  availableInstructors = signal<Instructor[]>([]);
  availableVehicles = signal<Vehicle[]>([]);

  lessonForm: FormGroup;
  autoDispatchForm: FormGroup;
  completeForm: FormGroup;

  showForm = signal(false);
  showAutoDispatch = signal(false);
  showCompleteModal = signal(false);

  editingId = signal<string | null>(null);
  selectedLessonForCompletion = signal<Lesson | null>(null);
  selectedExercises = signal<string[]>(['Arranque', 'Mudança de velocidade']);

  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalLessons = signal(0);
  scheduledLessons = signal(0);
  activeLessons = signal(0);
  completedLessons = signal(0);

  constructor(
    private fb: FormBuilder,
    private lessonService: LessonService,
    private instructorService: InstructorService,
    private studentService: StudentService,
    private vehicleService: VehicleService,
    public authService: AuthService
  ) {
    this.lessonForm = this.fb.group({
      studentId: ['', Validators.required],
      instructorId: ['', Validators.required],
      vehicleId: ['', Validators.required],
      scheduledDate: ['', Validators.required],
      durationMinutes: [60, [Validators.required, Validators.min(15)]],
      topic: ['', Validators.required],
      status: [1, Validators.required],
      observations: ['']
    });

    this.autoDispatchForm = this.fb.group({
      studentId: ['', Validators.required],
      scheduledDate: ['', Validators.required],
      durationMinutes: [60, [Validators.required, Validators.min(15)]],
      topic: ['Condução Urbana / Aula Prática'],
      observations: ['']
    });

    this.completeForm = this.fb.group({
      evaluation: [1, Validators.required],
      observations: ['']
    });
  }

  ngOnInit(): void {
    this.loadLessons();
    this.loadDropdowns();
  }

  loadDropdowns(): void {
    this.instructorService.getInstructors().subscribe({
      next: (data) => {
        this.instructors.set(data);
        this.availableInstructors.set(data);
      }
    });
    this.studentService.getStudents().subscribe({
      next: (data) => this.students.set(data)
    });
    this.vehicleService.getVehicles().subscribe({
      next: (data) => {
        this.vehicles.set(data);
        this.availableVehicles.set(data);
      }
    });
  }

  loadLessons(): void {
    this.lessonService.getLessons().subscribe({
      next: (data) => {
        this.lessons.set(data);
        this.totalLessons.set(data.length);
        this.scheduledLessons.set(data.filter(l => l.status === 1).length);
        this.activeLessons.set(data.filter(l => l.status === 5).length);
        this.completedLessons.set(data.filter(l => l.status === 2).length);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar aulas.');
      }
    });
  }

  onDateOrDurationChange(): void {
    const scheduledDate = this.lessonForm.get('scheduledDate')?.value;
    const durationMinutes = this.lessonForm.get('durationMinutes')?.value || 60;
    if (!scheduledDate) return;

    this.lessonService.getAvailableResources(scheduledDate, durationMinutes).subscribe({
      next: (res) => {
        if (res.availableInstructors) this.availableInstructors.set(res.availableInstructors);
        if (res.availableVehicles) this.availableVehicles.set(res.availableVehicles);
      }
    });
  }

  openCreateForm(): void {
    this.editingId.set(null);
    this.lessonForm.reset({ durationMinutes: 60, status: 1 });
    this.availableInstructors.set(this.instructors());
    this.availableVehicles.set(this.vehicles());
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(lesson: Lesson): void {
    if (!lesson.id) return;
    this.editingId.set(lesson.id);
    const dateFormatted = lesson.scheduledDate ? new Date(lesson.scheduledDate).toISOString().slice(0, 16) : '';
    this.lessonForm.patchValue({
      studentId: lesson.studentId,
      instructorId: lesson.instructorId,
      vehicleId: lesson.vehicleId,
      scheduledDate: dateFormatted,
      durationMinutes: lesson.durationMinutes,
      topic: lesson.topic,
      status: lesson.status,
      observations: lesson.observations
    });
    this.availableInstructors.set(this.instructors());
    this.availableVehicles.set(this.vehicles());
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.lessonForm.reset({ durationMinutes: 60, status: 1 });
  }

  onSubmit(): void {
    if (this.lessonForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formData = this.lessonForm.value;
    const currentId = this.editingId();

    if (currentId) {
      this.lessonService.updateLesson(currentId, { ...formData, id: currentId }).subscribe({
        next: () => {
          this.successMessage.set('Aula prática atualizada com sucesso!');
          this.closeForm();
          this.loadLessons();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para editar aulas práticas.'
            : (err.error?.message || 'Erro ao atualizar aula.'));
        }
      });
    } else {
      this.lessonService.createLesson(formData).subscribe({
        next: () => {
          this.successMessage.set('Aula prática registada com sucesso!');
          this.closeForm();
          this.loadLessons();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para agendar aulas práticas.'
            : (err.error?.message || 'Erro ao criar aula.'));
        }
      });
    }
  }

  // --- Despacho Automático ---
  openAutoDispatchModal(): void {
    const defaultDate = new Date(Date.now() + 3600000).toISOString().slice(0, 16);
    this.autoDispatchForm.reset({
      scheduledDate: defaultDate,
      durationMinutes: 60,
      topic: 'Aula Prática - Despacho FrotaGo',
      observations: 'Alocação Inteligente'
    });
    this.showAutoDispatch.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeAutoDispatch(): void {
    this.showAutoDispatch.set(false);
  }

  submitAutoDispatch(): void {
    if (this.autoDispatchForm.invalid) return;
    this.errorMessage.set(null);

    this.lessonService.autoDispatch(this.autoDispatchForm.value).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Aula despachada com sucesso!');
        this.closeAutoDispatch();
        this.loadLessons();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao executar despacho automático.');
      }
    });
  }

  // --- Fluxo de Início e Término da Aula (Instrutor) ---
  startLesson(lesson: Lesson): void {
    if (!lesson.id) return;
    if (confirm(`Iniciar aula prática para o aluno "${lesson.student?.name || 'Aluno'}" e ativar rastreamento GPS?`)) {
      this.lessonService.startLesson(lesson.id).subscribe({
        next: (res) => {
          this.successMessage.set(res.message || 'Aula iniciada com sucesso. GPS Ativo!');
          this.loadLessons();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao iniciar aula.');
        }
      });
    }
  }

  openCompleteModal(lesson: Lesson): void {
    this.selectedLessonForCompletion.set(lesson);
    this.completeForm.reset({
      evaluation: 1,
      observations: 'Aula executada com sucesso.'
    });
    this.selectedExercises.set(['Arranque', 'Mudança de velocidade']);
    this.showCompleteModal.set(true);
  }

  closeCompleteModal(): void {
    this.showCompleteModal.set(false);
    this.selectedLessonForCompletion.set(null);
  }

  toggleExercise(exercise: string): void {
    const current = this.selectedExercises();
    if (current.includes(exercise)) {
      this.selectedExercises.set(current.filter(e => e !== exercise));
    } else {
      this.selectedExercises.set([...current, exercise]);
    }
  }

  hasExercise(exercise: string): boolean {
    return this.selectedExercises().includes(exercise);
  }

  submitCompleteLesson(): void {
    const lesson = this.selectedLessonForCompletion();
    if (!lesson?.id) return;

    const payload = {
      lessonId: lesson.id,
      evaluation: Number(this.completeForm.value.evaluation),
      exercisesCompletedJson: JSON.stringify(this.selectedExercises()),
      observations: this.completeForm.value.observations || ''
    };

    this.lessonService.completeLesson(lesson.id, payload).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Aula finalizada e progresso do aluno registado!');
        this.closeCompleteModal();
        this.loadLessons();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao finalizar aula.');
      }
    });
  }

  deleteLesson(lesson: Lesson): void {
    if (!lesson.id) return;
    if (confirm(`Tem certeza que deseja eliminar a aula "${lesson.topic}"?`)) {
      this.lessonService.deleteLesson(lesson.id).subscribe({
        next: () => {
          this.successMessage.set('Aula eliminada com sucesso!');
          this.loadLessons();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para eliminar aulas práticas.'
            : (err.error?.message || 'Erro ao eliminar aula.'));
        }
      });
    }
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Agendada';
      case 2: return 'Realizada';
      case 3: return 'Cancelada';
      case 4: return 'Faltou';
      case 5: return 'Em Curso 🟢';
      default: return 'N/A';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-scheduled';
      case 2: return 'status-completed';
      case 3: return 'status-cancelled';
      case 4: return 'status-missed';
      case 5: return 'status-in-use';
      default: return '';
    }
  }

  getCategoryLabel(cat?: number): string {
    switch (cat) {
      case 1: return 'Categoria A';
      case 2: return 'Categoria B';
      case 3: return 'Categoria C';
      case 4: return 'Categoria D';
      default: return 'Categoria B';
    }
  }
}
