import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { MaintenanceComponent } from './maintenance.component';
import { MaintenanceService } from './services/maintenance.service';
import { VehicleService } from '../vehicles/services/vehicle.service';

describe('MaintenanceComponent', () => {
  let component: MaintenanceComponent;
  let fixture: ComponentFixture<MaintenanceComponent>;
  let maintenanceService: any;
  let vehicleService: any;

  beforeEach(async () => {
    maintenanceService = {
    getMaintenances: vi.fn(),
    createMaintenance: vi.fn(),
    updateMaintenance: vi.fn(),
    deleteMaintenance: vi.fn()
  } as any;
    vehicleService = {
    getVehicles: vi.fn()
  } as any;
    maintenanceService.getMaintenances.mockReturnValue(of([]));
    vehicleService.getVehicles.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [MaintenanceComponent],
      providers: [
        { provide: MaintenanceService, useValue: maintenanceService },
        { provide: VehicleService, useValue: vehicleService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(MaintenanceComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load maintenances on init', () => {
    component.ngOnInit();
    expect(maintenanceService.getMaintenances).toHaveBeenCalled();
  });
});
