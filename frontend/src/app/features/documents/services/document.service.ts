import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface VehicleDocument {
  id?: string;
  vehicleId: string;
  vehicle?: { licensePlate: string; brand: string; model: string };
  type: number;
  documentNumber: string;
  expiryDate: string;
  issueDate: string;
  fileUrl?: string;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly apiUrl = `http://${window.location.hostname}:5073/api/documents`;

  constructor(private http: HttpClient) {}

  getDocuments(): Observable<VehicleDocument[]> {
    return this.http.get<VehicleDocument[]>(this.apiUrl);
  }

  createDocument(document: any): Observable<any> {
    return this.http.post(this.apiUrl, document);
  }
}
