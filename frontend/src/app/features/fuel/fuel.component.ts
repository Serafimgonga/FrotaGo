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
      const selected = this.vehicles().find(v => v.id === vehicleId);
      if (selected) {
        this.fuelForm.patchValue({ odometer: selected.odometer });
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

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    if (!this.showForm()) {
      this.fuelForm.reset({ litres: 0, costPerLitre: 0, totalCost: 0, odometer: 0 });
    }
  }

  onSubmit(): void {
    if (this.fuelForm.invalid) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.fuelService.createFuelRecord(this.fuelForm.value).subscribe({
      next: () => {
        this.successMessage.set('Abastecimento registado com sucesso!');
        this.toggleForm();
        this.loadFuelRecords();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Erro ao registar abastecimento.');
      }
    });
  }
}
