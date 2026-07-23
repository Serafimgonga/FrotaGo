import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { FuelService, FuelRecord } from './services/fuel.service';
import { VehicleService, Vehicle } from '../vehicles/services/vehicle.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-fuel',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './fuel.html',
  styleUrl: './fuel.css'
})
export class FuelComponent implements OnInit {
  fuelRecords = signal<FuelRecord[]>([]);
  vehicles = signal<Vehicle[]>([]);
  fuelForm: FormGroup;
  showForm = signal(false);
  editingId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  totalRecords = signal(0);
  totalLitres = signal(0);
  totalCost = signal(0);

  constructor(
    private fb: FormBuilder,
    private fuelService: FuelService,
    private vehicleService: VehicleService
  ) {
    this.fuelForm = this.fb.group({
      vehicleId: ['', Validators.required],
      litres: [0, [Validators.required, Validators.min(0.1)]],
      costPerLitre: [0, [Validators.required, Validators.min(0.01)]],
      totalCost: [0, [Validators.required, Validators.min(0)]],
      odometer: [0, [Validators.required, Validators.min(0)]],
      date: ['', Validators.required],
      location: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadFuelRecords();
    this.vehicleService.getVehicles().subscribe({
      next: (data) => this.vehicles.set(data)
    });

    this.fuelForm.get('vehicleId')?.valueChanges.subscribe(vehicleId => {
      if (!this.editingId()) {
        const selected = this.vehicles().find(v => v.id === vehicleId);
        if (selected) {
          this.fuelForm.patchValue({ odometer: selected.odometer });
        }
      }
    });

    // Auto calculate totalCost if litres and costPerLitre change
    this.fuelForm.valueChanges.subscribe(val => {
      if (val.litres && val.costPerLitre && (!val.totalCost || val.totalCost === 0)) {
        const calculated = Math.round(val.litres * val.costPerLitre);
        this.fuelForm.patchValue({ totalCost: calculated }, { emitEvent: false });
      }
    });
  }

  loadFuelRecords(): void {
    this.fuelService.getFuelRecords().subscribe({
      next: (data) => {
        this.fuelRecords.set(data);
        this.totalRecords.set(data.length);
        this.totalLitres.set(data.reduce((sum, f) => sum + f.litres, 0));
        this.totalCost.set(data.reduce((sum, f) => sum + f.totalCost, 0));
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar registos de combustível.');
      }
    });
  }

  openCreateForm(): void {
    this.editingId.set(null);
    this.fuelForm.reset({ litres: 0, costPerLitre: 0, totalCost: 0, odometer: 0 });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  openEditForm(record: FuelRecord): void {
    if (!record.id) return;
    this.editingId.set(record.id);
    const dateFormatted = record.date ? new Date(record.date).toISOString().split('T')[0] : '';
    this.fuelForm.patchValue({
      vehicleId: record.vehicleId,
      litres: record.litres,
      costPerLitre: record.costPerLitre,
      totalCost: record.totalCost,
      odometer: record.odometer,
      date: dateFormatted,
      location: record.location
    });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.fuelForm.reset({ litres: 0, costPerLitre: 0, totalCost: 0, odometer: 0 });
  }

  onSubmit(): void {
    if (this.fuelForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formData = this.fuelForm.value;
    const currentId = this.editingId();

    if (currentId) {
      this.fuelService.updateFuelRecord(currentId, { ...formData, id: currentId }).subscribe({
        next: () => {
          this.successMessage.set('Abastecimento atualizado com sucesso!');
          this.closeForm();
          this.loadFuelRecords();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao atualizar abastecimento.');
        }
      });
    } else {
      this.fuelService.createFuelRecord(formData).subscribe({
        next: () => {
          this.successMessage.set('Abastecimento registado com sucesso!');
          this.closeForm();
          this.loadFuelRecords();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao registar abastecimento.');
        }
      });
    }
  }

  deleteRecord(record: FuelRecord): void {
    if (!record.id) return;
    if (confirm(`Tem certeza que deseja eliminar o registo de abastecimento de ${record.litres}L em ${record.location}?`)) {
      this.fuelService.deleteFuelRecord(record.id).subscribe({
        next: () => {
          this.successMessage.set('Abastecimento eliminado com sucesso!');
          this.loadFuelRecords();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Erro ao eliminar abastecimento.');
        }
      });
    }
  }
}
