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
  private readonly apiUrl = '/api/documents';

  constructor(private http: HttpClient) {}

  getDocuments(): Observable<VehicleDocument[]> {
    return this.http.get<VehicleDocument[]>(this.apiUrl);
  }

  getDocumentById(id: string): Observable<VehicleDocument> {
    return this.http.get<VehicleDocument>(`${this.apiUrl}/${id}`);
  }

  createDocument(document: any): Observable<any> {
    return this.http.post(this.apiUrl, document);
  }

  updateDocument(id: string, document: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, document);
  }

  deleteDocument(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
