import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { FuelComponent } from './fuel.component';
import { FuelService } from './services/fuel.service';

describe('FuelComponent', () => {
  let component: FuelComponent;
  let fixture: ComponentFixture<FuelComponent>;
  let fuelService: jasmine.SpyObj<FuelService>;

  beforeEach(async () => {
    fuelService = jasmine.createSpyObj('FuelService', ['getFuelRecords']);
    fuelService.getFuelRecords.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [FuelComponent],
      providers: [{ provide: FuelService, useValue: fuelService }],
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
