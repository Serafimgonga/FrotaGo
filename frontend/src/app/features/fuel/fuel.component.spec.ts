import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { FuelComponent } from './fuel.component';
import { FuelService } from './services/fuel.service';
import { VehicleService } from '../vehicles/services/vehicle.service';

describe('FuelComponent', () => {
  let component: FuelComponent;
  let fixture: ComponentFixture<FuelComponent>;
  let fuelService: any;
  let vehicleService: any;

  beforeEach(async () => {
    fuelService = {
    getFuelRecords: vi.fn()
  } as any;
    vehicleService = {
    getVehicles: vi.fn()
  } as any;
    fuelService.getFuelRecords.mockReturnValue(of([]));
    vehicleService.getVehicles.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [FuelComponent],
      providers: [
        { provide: FuelService, useValue: fuelService },
        { provide: VehicleService, useValue: vehicleService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(FuelComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load fuel records on init', () => {
    component.ngOnInit();
    expect(fuelService.getFuelRecords).toHaveBeenCalled();
  });
});
