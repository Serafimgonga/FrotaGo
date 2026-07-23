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
  editingId = signal<string | null>(null);
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
      if (!this.editingId()) {
        const selected = this.vehicles().find(v => v.id === vehicleId);
        if (selected) {
          this.maintenanceForm.patchValue({ odometer: selected.odometer });
        }
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

  openCreateForm(): void {
    this.editingId.set(null);
    this.maintenanceForm.reset({ cost: 0, odometer: 0, type: 1, status: 1 });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(m: MaintenanceRecord): void {
    if (!m.id) return;
    this.editingId.set(m.id);
    const dateFormatted = m.maintenanceDate ? new Date(m.maintenanceDate).toISOString().split('T')[0] : '';
    this.maintenanceForm.patchValue({
      vehicleId: m.vehicleId,
      description: m.description,
      cost: m.cost,
      maintenanceDate: dateFormatted,
      type: m.type,
      status: m.status,
      odometer: m.odometer
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.maintenanceForm.reset({ cost: 0, odometer: 0, type: 1, status: 1 });
  }

  onSubmit(): void {
    if (this.maintenanceForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formData = this.maintenanceForm.value;
    const currentId = this.editingId();

    if (currentId) {
      this.maintenanceService.updateMaintenance(currentId, { ...formData, id: currentId }).subscribe({
        next: () => {
          this.successMessage.set('Manutenção atualizada com sucesso!');
          this.closeForm();
          this.loadMaintenances();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao atualizar manutenção.');
        }
      });
    } else {
      this.maintenanceService.createMaintenance(formData).subscribe({
        next: () => {
          this.successMessage.set('Manutenção registada com sucesso!');
          this.closeForm();
          this.loadMaintenances();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao criar manutenção.');
        }
      });
    }
  }

  deleteMaintenance(m: MaintenanceRecord): void {
    if (!m.id) return;
    if (confirm(`Tem certeza que deseja eliminar esta manutenção (${m.description})?`)) {
      this.maintenanceService.deleteMaintenance(m.id).subscribe({
        next: () => {
          this.successMessage.set('Manutenção eliminada com sucesso!');
          this.loadMaintenances();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao eliminar manutenção.');
        }
      });
    }
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
