import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DocumentService, VehicleDocument } from './services/document.service';
import { VehicleService, Vehicle } from '../vehicles/services/vehicle.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-documents',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './documents.html',
  styleUrl: './documents.css'
})
export class DocumentsComponent implements OnInit {
  documents = signal<VehicleDocument[]>([]);
  vehicles = signal<Vehicle[]>([]);
  documentForm: FormGroup;
  showForm = signal(false);
  editingId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalDocuments = signal(0);
  expiredDocuments = signal(0);
  expiringDocuments = signal(0);

  constructor(
    private fb: FormBuilder,
    private documentService: DocumentService,
    private vehicleService: VehicleService
  ) {
    this.documentForm = this.fb.group({
      vehicleId: ['', Validators.required],
      type: [1, Validators.required],
      documentNumber: ['', Validators.required],
      issueDate: ['', Validators.required],
      expiryDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadDocuments();
    this.vehicleService.getVehicles().subscribe({
      next: (data) => this.vehicles.set(data)
    });
  }

  loadDocuments(): void {
    this.documentService.getDocuments().subscribe({
      next: (data) => {
        this.documents.set(data);
        this.totalDocuments.set(data.length);
        const now = new Date();
        const in30Days = new Date();
        in30Days.setDate(in30Days.getDate() + 30);
        this.expiredDocuments.set(data.filter(d => new Date(d.expiryDate) < now).length);
        this.expiringDocuments.set(data.filter(d => {
          const exp = new Date(d.expiryDate);
          return exp >= now && exp <= in30Days;
        }).length);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar documentos.');
      }
    });
  }

  openCreateForm(): void {
    this.editingId.set(null);
    this.documentForm.reset({ type: 1 });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(doc: VehicleDocument): void {
    if (!doc.id) return;
    this.editingId.set(doc.id);
    const issueDateFormatted = doc.issueDate ? new Date(doc.issueDate).toISOString().split('T')[0] : '';
    const expiryDateFormatted = doc.expiryDate ? new Date(doc.expiryDate).toISOString().split('T')[0] : '';
    this.documentForm.patchValue({
      vehicleId: doc.vehicleId,
      type: doc.type,
      documentNumber: doc.documentNumber,
      issueDate: issueDateFormatted,
      expiryDate: expiryDateFormatted
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.documentForm.reset({ type: 1 });
  }

  onSubmit(): void {
    if (this.documentForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formData = this.documentForm.value;
    const currentId = this.editingId();

    if (currentId) {
      this.documentService.updateDocument(currentId, { ...formData, id: currentId }).subscribe({
        next: () => {
          this.successMessage.set('Documento atualizado com sucesso!');
          this.closeForm();
          this.loadDocuments();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao atualizar documento.');
        }
      });
    } else {
      this.documentService.createDocument(formData).subscribe({
        next: () => {
          this.successMessage.set('Documento registado com sucesso!');
          this.closeForm();
          this.loadDocuments();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao criar documento.');
        }
      });
    }
  }

  deleteDocument(doc: VehicleDocument): void {
    if (!doc.id) return;
    if (confirm(`Tem certeza que deseja eliminar o documento ${doc.documentNumber}?`)) {
      this.documentService.deleteDocument(doc.id).subscribe({
        next: () => {
          this.successMessage.set('Documento eliminado com sucesso!');
          this.loadDocuments();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao eliminar documento.');
        }
      });
    }
  }

  getTypeLabel(type: number): string {
    switch (type) {
      case 1: return 'Seguro';
      case 2: return 'Inspecção';
      case 3: return 'DUA';
      case 4: return 'Licença';
      default: return 'Outro';
    }
  }

  getExpiryStatus(expiryDate: string): string {
    const now = new Date();
    const exp = new Date(expiryDate);
    const in30Days = new Date();
    in30Days.setDate(in30Days.getDate() + 30);

    if (exp < now) return 'Expirado';
    if (exp <= in30Days) return 'A Expirar';
    return 'Válido';
  }

  getExpiryClass(expiryDate: string): string {
    const status = this.getExpiryStatus(expiryDate);
    switch (status) {
      case 'Expirado': return 'status-expired';
      case 'A Expirar': return 'status-expiring';
      case 'Válido': return 'status-valid';
      default: return '';
    }
  }
}
