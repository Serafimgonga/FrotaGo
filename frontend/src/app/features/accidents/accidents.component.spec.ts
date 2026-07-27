import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { of } from 'rxjs';
import { AccidentsComponent } from './accidents.component';
import { AccidentService } from './services/accidents.service';
import { VehicleService } from '../vehicles/services/vehicle.service';

describe('AccidentsComponent', () => {
  let component: AccidentsComponent;
  let fixture: ComponentFixture<AccidentsComponent>;
  let accidentService: jasmine.SpyObj<AccidentService>;
  let vehicleService: jasmine.SpyObj<VehicleService>;

  beforeEach(async () => {
    accidentService = jasmine.createSpyObj('AccidentService', [
      'getAccidents',
      'createAccident',
      'updateAccident',
      'deleteAccident'
    ]);
    vehicleService = jasmine.createSpyObj('VehicleService', ['getVehicles']);

    accidentService.getAccidents.and.returnValue(of([]));
    vehicleService.getVehicles.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [AccidentsComponent],
      providers: [
        FormBuilder,
        { provide: AccidentService, useValue: accidentService },
        { provide: VehicleService, useValue: vehicleService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(AccidentsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load accidents on init', () => {
    component.ngOnInit();
    expect(accidentService.getAccidents).toHaveBeenCalled();
  });

  it('should return the correct status label', () => {
    expect(component.getStatusLabel(2)).toBe('Em Resolução');
  });
});
