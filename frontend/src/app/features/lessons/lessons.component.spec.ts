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
  let lessonService: jasmine.SpyObj<LessonService>;
  let instructorService: jasmine.SpyObj<InstructorService>;
  let studentService: jasmine.SpyObj<StudentService>;
  let vehicleService: jasmine.SpyObj<VehicleService>;
  let authService: Partial<AuthService>;

  beforeEach(async () => {
    lessonService = jasmine.createSpyObj('LessonService', ['getLessons', 'getAvailableResources', 'autoDispatch', 'startLesson', 'createLesson', 'updateLesson', 'deleteLesson']);
    instructorService = jasmine.createSpyObj('InstructorService', ['getInstructors']);
    studentService = jasmine.createSpyObj('StudentService', ['getStudents']);
    vehicleService = jasmine.createSpyObj('VehicleService', ['getVehicles']);
    lessonService.getLessons.and.returnValue(of([]));
    instructorService.getInstructors.and.returnValue(of([]));
    studentService.getStudents.and.returnValue(of([]));
    vehicleService.getVehicles.and.returnValue(of([]));
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
