import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Lesson {
  id?: string;
  studentId: string;
  studentName?: string;
  student?: { name: string; category?: number };
  instructorId: string;
  instructorName?: string;
  instructor?: { name: string };
  vehicleId: string;
  vehiclePlate?: string;
  vehicle?: { licensePlate: string; brand: string; model: string };
  scheduledDate: string;
  durationMinutes: number;
  topic: string;
  status: number; // 1: Agendada, 2: Realizada, 3: Cancelada, 4: Faltou, 5: EmCurso
  observations: string;

  startedAt?: string;
  completedAt?: string;
  evaluation?: number; // 0: N/A, 1: Excelente, 2: Boa, 3: Precisa Melhorar
  exercisesCompletedJson?: string;
  trackingSessionId?: string;

  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class LessonService {
  private readonly apiUrl = '/api/lessons';

  constructor(private http: HttpClient) {}

  getLessons(): Observable<Lesson[]> {
    return this.http.get<Lesson[]>(this.apiUrl);
  }

  getLessonById(id: string): Observable<Lesson> {
    return this.http.get<Lesson>(`${this.apiUrl}/${id}`);
  }

  getAvailableResources(scheduledDate: string, durationMinutes: number = 60): Observable<{ availableInstructors: any[]; availableVehicles: any[] }> {
    return this.http.get<{ availableInstructors: any[]; availableVehicles: any[] }>(
      `${this.apiUrl}/available?scheduledDate=${encodeURIComponent(scheduledDate)}&durationMinutes=${durationMinutes}`
    );
  }

  autoDispatch(data: { studentId: string; scheduledDate: string; durationMinutes: number; topic?: string; observations?: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/auto-dispatch`, data);
  }

  startLesson(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/start`, {});
  }

  completeLesson(id: string, completionData: { lessonId: string; evaluation: number; exercisesCompletedJson: string; observations: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/complete`, completionData);
  }

  createLesson(lesson: any): Observable<any> {
    return this.http.post(this.apiUrl, lesson);
  }

  updateLesson(id: string, lesson: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, lesson);
  }

  deleteLesson(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
