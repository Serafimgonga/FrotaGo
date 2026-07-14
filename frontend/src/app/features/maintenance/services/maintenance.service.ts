import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MaintenanceRecord {
  id?: string;
  vehicleId: string;
  vehicle?: { licensePlate: string; brand: string; model: string };
  description: string;
  cost: number;
  maintenanceDate: string;
  type: number;
  status: number;
  odometer: number;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MaintenanceService {
  private readonly apiUrl = '/api/maintenance';

  constructor(private http: HttpClient) {}

  getMaintenances(): Observable<MaintenanceRecord[]> {
    return this.http.get<MaintenanceRecord[]>(this.apiUrl);
  }

  createMaintenance(maintenance: any): Observable<any> {
    return this.http.post(this.apiUrl, maintenance);
  }
}
