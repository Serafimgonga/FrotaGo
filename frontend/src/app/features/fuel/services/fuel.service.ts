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
  private readonly apiUrl = `http://${window.location.hostname}:5073/api/fuel`;

  constructor(private http: HttpClient) {}

  getFuelRecords(): Observable<FuelRecord[]> {
    return this.http.get<FuelRecord[]>(this.apiUrl);
  }

  createFuelRecord(record: any): Observable<any> {
    return this.http.post(this.apiUrl, record);
  }
}
