import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { DocumentsComponent } from './documents.component';
import { DocumentService } from './services/document.service';
import { VehicleService } from '../vehicles/services/vehicle.service';

describe('DocumentsComponent', () => {
  let component: DocumentsComponent;
  let fixture: ComponentFixture<DocumentsComponent>;
  let documentService: jasmine.SpyObj<DocumentService>;
  let vehicleService: jasmine.SpyObj<VehicleService>;

  beforeEach(async () => {
    documentService = jasmine.createSpyObj('DocumentService', ['getDocuments', 'createDocument', 'updateDocument', 'deleteDocument']);
    vehicleService = jasmine.createSpyObj('VehicleService', ['getVehicles']);
    documentService.getDocuments.and.returnValue(of([]));
    vehicleService.getVehicles.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [DocumentsComponent],
      providers: [
        { provide: DocumentService, useValue: documentService },
        { provide: VehicleService, useValue: vehicleService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load documents on init', () => {
    component.ngOnInit();
    expect(documentService.getDocuments).toHaveBeenCalled();
  });
});
