import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { VehicleService, Vehicle } from './services/vehicle.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { AuthService } from '../authentication/services/auth.service';

@Component({
  selector: 'app-vehicles',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './vehicles.html',
  styleUrl: './vehicles.css'
})
export class VehiclesComponent implements OnInit {
  vehicles = signal<Vehicle[]>([]);
  vehicleForm: FormGroup;
  showForm = signal(false);
  editingVehicleId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Statistics signals
  totalVehicles = signal(0);
  availableVehicles = signal(0);
  maintenanceVehicles = signal(0);

  constructor(
    private fb: FormBuilder,
    private vehicleService: VehicleService,
    public authService: AuthService
  ) {
    this.vehicleForm = this.fb.group({
      licensePlate: ['', Validators.required],
      brand: ['', Validators.required],
      model: ['', Validators.required],
      chassis: ['', Validators.required],
      year: [new Date().getFullYear(), [Validators.required, Validators.min(1900)]],
      odometer: [0, [Validators.required, Validators.min(0)]],
      fuel: [1, Validators.required],
      transmission: [1, Validators.required],
      status: [1, Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadVehicles();
  }

  loadVehicles(): void {
    this.vehicleService.getVehicles().subscribe({
      next: (data) => {
        this.vehicles.set(data);
        this.calculateStats(data);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar veículos. Certifique-se de que o backend e a base de dados estão operacionais.');
      }
    });
  }

  calculateStats(list: Vehicle[]): void {
    this.totalVehicles.set(list.length);
    this.availableVehicles.set(list.filter(v => v.status === 1).length);
    this.maintenanceVehicles.set(list.filter(v => v.status === 3).length);
  }

  openCreateForm(): void {
    this.editingVehicleId.set(null);
    this.vehicleForm.reset({
      year: new Date().getFullYear(),
      odometer: 0,
      fuel: 1,
      transmission: 1,
      status: 1
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(vehicle: Vehicle): void {
    if (!vehicle.id) return;
    this.editingVehicleId.set(vehicle.id);
    this.vehicleForm.patchValue({
      licensePlate: vehicle.licensePlate,
      brand: vehicle.brand,
      model: vehicle.model,
      chassis: vehicle.chassis,
      year: vehicle.year,
      odometer: vehicle.odometer,
      fuel: vehicle.fuel,
      transmission: vehicle.transmission,
      status: vehicle.status
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingVehicleId.set(null);
    this.vehicleForm.reset();
  }

  onSubmit(): void {
    if (this.vehicleForm.invalid) return;

    this.errorMessage.set(null);
    this.successMessage.set(null);

    const vehicleData = this.vehicleForm.value;
    const currentEditId = this.editingVehicleId();

    if (currentEditId) {
      this.vehicleService.updateVehicle(currentEditId, { ...vehicleData, id: currentEditId }).subscribe({
        next: () => {
          this.successMessage.set('Veículo atualizado com sucesso!');
          this.closeForm();
          this.loadVehicles();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para editar veículos.'
            : (err.error?.message || 'Erro ao atualizar veículo.'));
        }
      });
    } else {
      this.vehicleService.createVehicle(vehicleData).subscribe({
        next: () => {
          this.successMessage.set('Veículo registado com sucesso!');
          this.closeForm();
          this.loadVehicles();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para registar veículos.'
            : (err.error?.message || 'Erro ao criar veículo.'));
        }
      });
    }
  }

  deleteVehicle(vehicle: Vehicle): void {
    if (!vehicle.id) return;
    if (confirm(`Tem certeza que deseja eliminar o veículo ${vehicle.brand} ${vehicle.model} (${vehicle.licensePlate})?`)) {
      this.vehicleService.deleteVehicle(vehicle.id).subscribe({
        next: () => {
          this.successMessage.set('Veículo eliminado com sucesso!');
          this.loadVehicles();
        },
        error: (err) => {
          this.errorMessage.set(err.status === 403
            ? 'Sem autorização. O seu perfil não tem permissão para eliminar veículos.'
            : (err.error?.message || 'Erro ao eliminar veículo.'));
        }
      });
    }
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Disponível';
      case 2: return 'Em Aula';
      case 3: return 'Em Manutenção';
      case 4: return 'Acidentado';
      case 5: return 'Fora de Serviço';
      default: return 'Desconhecido';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-available';
      case 2: return 'status-in-use';
      case 3: return 'status-maintenance';
      case 4: return 'status-accident';
      case 5: return 'status-out-service';
      default: return '';
    }
  }

  getFuelLabel(fuel: number): string {
    switch (fuel) {
      case 1: return 'Gasolina';
      case 2: return 'Gasóleo (Diesel)';
      case 3: return 'GPL';
      case 4: return 'Híbrido';
      case 5: return 'Elétrico';
      default: return 'Outro';
    }
  }

  getTransmissionLabel(trans: number): string {
    return trans === 1 ? 'Manual' : 'Automática';
  }
}
