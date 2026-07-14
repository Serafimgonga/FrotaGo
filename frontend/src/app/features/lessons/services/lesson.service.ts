import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Lesson {
  id?: string;
  studentId: string;
  studentName?: string;
  student?: { name: string };
  instructorId: string;
  instructorName?: string;
  instructor?: { name: string };
  vehicleId: string;
  vehiclePlate?: string;
  vehicle?: { licensePlate: string; brand: string; model: string };
  scheduledDate: string;
  durationMinutes: number;
  topic: string;
  status: number;
  observations: string;
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

  createLesson(lesson: any): Observable<any> {
    return this.http.post(this.apiUrl, lesson);
  }
}
