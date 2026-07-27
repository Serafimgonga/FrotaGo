import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { VehiclesComponent } from './vehicles.component';
import { VehicleService } from './services/vehicle.service';
import { AuthService } from '../authentication/services/auth.service';

describe('VehiclesComponent', () => {
  let component: VehiclesComponent;
  let fixture: ComponentFixture<VehiclesComponent>;
  let vehicleService: any;
  let authService: any;

  beforeEach(async () => {
    vehicleService = {
    getVehicles: vi.fn(),
    createVehicle: vi.fn(),
    updateVehicle: vi.fn(),
    deleteVehicle: vi.fn()
  } as any;
    vehicleService.getVehicles.mockReturnValue(of([]));
    authService = { currentUser: () => ({ schoolId: 'school-1' }) } as any;

    await TestBed.configureTestingModule({
      imports: [VehiclesComponent],
      providers: [
        { provide: VehicleService, useValue: vehicleService },
        { provide: AuthService, useValue: authService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(VehiclesComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load vehicles on init', () => {
    component.ngOnInit();
    expect(vehicleService.getVehicles).toHaveBeenCalled();
  });
});
