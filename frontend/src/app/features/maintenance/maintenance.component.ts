import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MaintenanceService, MaintenanceRecord } from './services/maintenance.service';
import { VehicleService, Vehicle } from '../vehicles/services/vehicle.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-maintenance',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './maintenance.html',
  styleUrl: './maintenance.css'
})
export class MaintenanceComponent implements OnInit {
  maintenances = signal<MaintenanceRecord[]>([]);
  vehicles = signal<Vehicle[]>([]);
  maintenanceForm: FormGroup;
  showForm = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalMaintenances = signal(0);
  totalCost = signal(0);
  pendingMaintenances = signal(0);

  constructor(
    private fb: FormBuilder,
    private maintenanceService: MaintenanceService,
    private vehicleService: VehicleService
  ) {
    this.maintenanceForm = this.fb.group({
      vehicleId: ['', Validators.required],
      description: ['', Validators.required],
      cost: [0, [Validators.required, Validators.min(0)]],
      maintenanceDate: ['', Validators.required],
      type: [1, Validators.required],
      status: [1, Validators.required],
      odometer: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadMaintenances();
    this.vehicleService.getVehicles().subscribe({
      next: (data) => this.vehicles.set(data)
    });

    this.maintenanceForm.get('vehicleId')?.valueChanges.subscribe(vehicleId => {
      const selected = this.vehicles().find(v => v.id === vehicleId);
      if (selected) {
        this.maintenanceForm.patchValue({ odometer: selected.odometer });
      }
    });
  }

  loadMaintenances(): void {
    this.maintenanceService.getMaintenances().subscribe({
      next: (data) => {
        this.maintenances.set(data);
        this.totalMaintenances.set(data.length);
        this.totalCost.set(data.reduce((sum, m) => sum + m.cost, 0));
        this.pendingMaintenances.set(data.filter(m => m.status !== 3).length);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar manutenções.');
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    if (!this.showForm()) {
      this.maintenanceForm.reset({ cost: 0, odometer: 0, type: 1, status: 1 });
    }
  }

  onSubmit(): void {
    if (this.maintenanceForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.maintenanceService.createMaintenance(this.maintenanceForm.value).subscribe({
      next: () => {
        this.successMessage.set('Manutenção registada com sucesso!');
        this.toggleForm();
        this.loadMaintenances();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao criar manutenção.');
      }
    });
  }

  getTypeLabel(type: number): string {
    return type === 1 ? 'Preventiva' : 'Corretiva';
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Agendada';
      case 2: return 'Em Progresso';
      case 3: return 'Concluída';
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
