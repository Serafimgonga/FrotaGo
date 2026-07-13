import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { VehicleService, Vehicle } from './services/vehicle.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

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
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Statistics signals
  totalVehicles = signal(0);
  availableVehicles = signal(0);
  maintenanceVehicles = signal(0);

  constructor(
    private fb: FormBuilder,
    private vehicleService: VehicleService
  ) {
    this.vehicleForm = this.fb.group({
      licensePlate: ['', Validators.required],
      brand: ['', Validators.required],
      model: ['', Validators.required],
      chassis: ['', Validators.required],
      year: [new Date().getFullYear(), [Validators.required, Validators.min(1900)]],
      odometer: [0, [Validators.required, Validators.min(0)]],
      fuel: [1, Validators.required], // Gasolina
      transmission: [1, Validators.required] // Manual
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

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    if (!this.showForm()) {
      this.vehicleForm.reset({
        year: new Date().getFullYear(),
        odometer: 0,
        fuel: 1,
        transmission: 1
      });
    }
  }

  onSubmit(): void {
    if (this.vehicleForm.invalid) return;

    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.vehicleService.createVehicle(this.vehicleForm.value).subscribe({
      next: () => {
        this.successMessage.set('Veículo registado com sucesso!');
        this.toggleForm();
        this.loadVehicles();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao criar veículo.');
      }
    });
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
