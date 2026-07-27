import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { TrackingComponent } from './tracking.component';
import { VehicleService } from '../vehicles/services/vehicle.service';
import { AuthService } from '../authentication/services/auth.service';

describe('TrackingComponent', () => {
  let component: TrackingComponent;
  let fixture: ComponentFixture<TrackingComponent>;
  let vehicleService: any;
  let authService: any;

  beforeEach(async () => {
    vehicleService = {
    getVehicles: vi.fn()
  } as any;
    vehicleService.getVehicles.mockReturnValue(of([]));
    authService = {
    token: vi.fn()
  } as any;
    authService.token.mockReturnValue(null);

    await TestBed.configureTestingModule({
      imports: [TrackingComponent, HttpClientTestingModule],
      providers: [
        { provide: VehicleService, useValue: vehicleService },
        { provide: AuthService, useValue: authService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(TrackingComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have no transmitting vehicle initially', () => {
    expect(component.getTransmittingVehicleName()).toBe('');
    expect(component.getTransmittingVehicleState()).toBeUndefined();
  });
});
