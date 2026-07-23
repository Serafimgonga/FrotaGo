import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface FuelRecord {
  id?: string;
  vehicleId: string;
  vehicle?: { licensePlate: string; brand: string; model: string };
  litres: number;
  costPerLitre: number;
  totalCost: number;
  odometer: number;
  date: string;
  location: string;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class FuelService {
  private readonly apiUrl = '/api/fuel';

  constructor(private http: HttpClient) {}

  getFuelRecords(): Observable<FuelRecord[]> {
    return this.http.get<FuelRecord[]>(this.apiUrl);
  }

  getFuelRecordById(id: string): Observable<FuelRecord> {
    return this.http.get<FuelRecord>(`${this.apiUrl}/${id}`);
  }

  createFuelRecord(record: any): Observable<any> {
    return this.http.post(this.apiUrl, record);
  }

  updateFuelRecord(id: string, record: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, record);
  }

  deleteFuelRecord(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
