import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Vehicle {
  id?: string;
  licensePlate: string;
  brand: string;
  model: string;
  chassis: string;
  year: number;
  odometer: number;
  fuel: number;
  transmission: number;
  status: number;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  private readonly apiUrl = '/api/vehicles';

  constructor(private http: HttpClient) {}

  getVehicles(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(this.apiUrl);
  }

  createVehicle(vehicle: Vehicle): Observable<any> {
    return this.http.post(this.apiUrl, vehicle);
  }
}
