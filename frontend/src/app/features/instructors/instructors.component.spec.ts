import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { InstructorsComponent } from './instructors.component';
import { InstructorService } from './services/instructor.service';

describe('InstructorsComponent', () => {
  let component: InstructorsComponent;
  let fixture: ComponentFixture<InstructorsComponent>;
  let instructorService: any;

  beforeEach(async () => {
    instructorService = {
    getInstructors: vi.fn()
  } as any;
    instructorService.getInstructors.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [InstructorsComponent],
      providers: [{ provide: InstructorService, useValue: instructorService }],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(InstructorsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load instructors on init', () => {
    component.ngOnInit();
    expect(instructorService.getInstructors).toHaveBeenCalled();
  });
});
