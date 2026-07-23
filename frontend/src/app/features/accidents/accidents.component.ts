import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AccidentService, Accident } from './services/accidents.service';
import { VehicleService, Vehicle } from '../vehicles/services/vehicle.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-accidents',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './accidents.html',
  styleUrl: './accidents.css'
})
export class AccidentsComponent implements OnInit {
  accidents = signal<Accident[]>([]);
  vehicles = signal<Vehicle[]>([]);
  accidentForm: FormGroup;
  showForm = signal(false);
  editingId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalAccidents = signal(0);
  pendingAccidents = signal(0);
  totalEstimatedCost = signal(0);

  constructor(
    private fb: FormBuilder,
    private accidentService: AccidentService,
    private vehicleService: VehicleService
  ) {
    this.accidentForm = this.fb.group({
      vehicleId: ['', Validators.required],
      date: ['', Validators.required],
      description: ['', Validators.required],
      severity: [1, Validators.required],
      estimatedCost: [0, [Validators.required, Validators.min(0)]],
      location: ['', Validators.required],
      status: [1, Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadAccidents();
    this.vehicleService.getVehicles().subscribe({
      next: (data) => this.vehicles.set(data)
    });
  }

  loadAccidents(): void {
    this.accidentService.getAccidents().subscribe({
      next: (data) => {
        this.accidents.set(data);
        this.totalAccidents.set(data.length);
        this.pendingAccidents.set(data.filter(a => a.status !== 3).length);
        this.totalEstimatedCost.set(data.reduce((sum, a) => sum + a.estimatedCost, 0));
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar registos de acidentes.');
      }
    });
  }

  openCreateForm(): void {
    this.editingId.set(null);
    this.accidentForm.reset({ severity: 1, estimatedCost: 0, status: 1 });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(accident: Accident): void {
    if (!accident.id) return;
    this.editingId.set(accident.id);
    const dateFormatted = accident.date ? new Date(accident.date).toISOString().split('T')[0] : '';
    this.accidentForm.patchValue({
      vehicleId: accident.vehicleId,
      date: dateFormatted,
      description: accident.description,
      severity: accident.severity,
      estimatedCost: accident.estimatedCost,
      location: accident.location,
      status: accident.status
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.accidentForm.reset({ severity: 1, estimatedCost: 0, status: 1 });
  }

  onSubmit(): void {
    if (this.accidentForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formData = this.accidentForm.value;
    const currentId = this.editingId();

    if (currentId) {
      this.accidentService.updateAccident(currentId, { ...formData, id: currentId }).subscribe({
        next: () => {
          this.successMessage.set('Registo de acidente atualizado com sucesso!');
          this.closeForm();
          this.loadAccidents();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao atualizar acidente.');
        }
      });
    } else {
      this.accidentService.createAccident(formData).subscribe({
        next: () => {
          this.successMessage.set('Acidente registado com sucesso! O estado do veículo foi alterado para Acidentado.');
          this.closeForm();
          this.loadAccidents();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao registar acidente.');
        }
      });
    }
  }

  deleteAccident(accident: Accident): void {
    if (!accident.id) return;
    if (confirm(`Tem certeza que deseja eliminar o registo de acidente em ${accident.location}?`)) {
      this.accidentService.deleteAccident(accident.id).subscribe({
        next: () => {
          this.successMessage.set('Registo de acidente eliminado com sucesso!');
          this.loadAccidents();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao eliminar acidente.');
        }
      });
    }
  }

  getSeverityLabel(severity: number): string {
    switch (severity) {
      case 1: return 'Leve';
      case 2: return 'Moderada';
      case 3: return 'Grave';
      default: return 'N/A';
    }
  }

  getSeverityClass(severity: number): string {
    switch (severity) {
      case 1: return 'status-valid';
      case 2: return 'status-expiring';
      case 3: return 'status-expired';
      default: return '';
    }
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Pendente';
      case 2: return 'Em Resolução';
      case 3: return 'Resolvido';
      default: return 'N/A';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-scheduled';
      case 2: return 'status-in-progress';
      case 3: return 'status-completed';
      default: return '';
    }
  }
}
