import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Instructor {
  id?: string;
  name: string;
  email: string;
  phoneNumber: string;
  licenseNumber: string;
  isActive?: boolean;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class InstructorService {
  private readonly apiUrl = '/api/instructors';

  constructor(private http: HttpClient) {}

  getInstructors(): Observable<Instructor[]> {
    return this.http.get<Instructor[]>(this.apiUrl);
  }

  getInstructorById(id: string): Observable<Instructor> {
    return this.http.get<Instructor>(`${this.apiUrl}/${id}`);
  }

  createInstructor(instructor: Instructor): Observable<any> {
    return this.http.post(this.apiUrl, instructor);
  }

  updateInstructor(id: string, instructor: Instructor): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, instructor);
  }

  deleteInstructor(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
