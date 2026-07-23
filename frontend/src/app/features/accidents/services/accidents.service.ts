import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Accident {
  id?: string;
  vehicleId: string;
  vehicle?: { licensePlate: string; brand: string; model: string };
  date: string;
  description: string;
  severity: number;
  estimatedCost: number;
  location: string;
  status: number;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AccidentService {
  private readonly apiUrl = '/api/accidents';

  constructor(private http: HttpClient) {}

  getAccidents(): Observable<Accident[]> {
    return this.http.get<Accident[]>(this.apiUrl);
  }

  getAccidentById(id: string): Observable<Accident> {
    return this.http.get<Accident>(`${this.apiUrl}/${id}`);
  }

  createAccident(accident: any): Observable<any> {
    return this.http.post(this.apiUrl, accident);
  }

  updateAccident(id: string, accident: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, accident);
  }

  deleteAccident(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
