import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Student {
  id?: string;
  name: string;
  email: string;
  phoneNumber: string;
  identityCardNumber: string;
  category: number;

  // Esteira de Progresso
  documentsSubmitted?: boolean;
  registrationFeePaid?: boolean;
  theoryCompleted?: boolean;
  practicalLessonsStarted?: boolean;
  examScheduled?: boolean;
  progressStatus?: number; // 1: Inscrição, 2: Teoria, 3: Prática, 4: Exame, 5: Concluído
  requiredLessonsCount?: number;
  completedLessonsCount?: number;

  isActive?: boolean;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private readonly apiUrl = '/api/students';

  constructor(private http: HttpClient) {}

  getStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(this.apiUrl);
  }

  getStudentById(id: string): Observable<Student> {
    return this.http.get<Student>(`${this.apiUrl}/${id}`);
  }

  createStudent(student: Student): Observable<any> {
    return this.http.post(this.apiUrl, student);
  }

  updateStudent(id: string, student: Student): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, student);
  }

  updateProgress(id: string, progressData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/progress`, { ...progressData, studentId: id });
  }

  deleteStudent(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
