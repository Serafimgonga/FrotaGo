import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { of } from 'rxjs';
import { LessonsComponent } from './lessons.component';
import { LessonService } from './services/lesson.service';
import { InstructorService } from '../instructors/services/instructor.service';
import { StudentService } from '../students/services/student.service';
import { VehicleService } from '../vehicles/services/vehicle.service';
import { AuthService } from '../authentication/services/auth.service';

describe('LessonsComponent', () => {
  let component: LessonsComponent;
  let fixture: ComponentFixture<LessonsComponent>;
  let lessonService: any;
  let instructorService: any;
  let studentService: any;
  let vehicleService: any;
  let authService: any;

  beforeEach(async () => {
    lessonService = {
    getLessons: vi.fn(),
    getAvailableResources: vi.fn(),
    autoDispatch: vi.fn(),
    startLesson: vi.fn(),
    createLesson: vi.fn(),
    updateLesson: vi.fn(),
    deleteLesson: vi.fn()
  } as any;
    instructorService = {
    getInstructors: vi.fn()
  } as any;
    studentService = {
    getStudents: vi.fn()
  } as any;
    vehicleService = {
    getVehicles: vi.fn()
  } as any;
    lessonService.getLessons.mockReturnValue(of([]));
    instructorService.getInstructors.mockReturnValue(of([]));
    studentService.getStudents.mockReturnValue(of([]));
    vehicleService.getVehicles.mockReturnValue(of([]));
    authService = { currentUser: () => ({ schoolId: 'school-1' }) } as any;

    await TestBed.configureTestingModule({
      imports: [LessonsComponent],
      providers: [
        FormBuilder,
        { provide: LessonService, useValue: lessonService },
        { provide: InstructorService, useValue: instructorService },
        { provide: StudentService, useValue: studentService },
        { provide: VehicleService, useValue: vehicleService },
        { provide: AuthService, useValue: authService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(LessonsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load lessons and dropdowns on init', () => {
    component.ngOnInit();
    expect(lessonService.getLessons).toHaveBeenCalled();
    expect(instructorService.getInstructors).toHaveBeenCalled();
    expect(studentService.getStudents).toHaveBeenCalled();
    expect(vehicleService.getVehicles).toHaveBeenCalled();
  });
});
