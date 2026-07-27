import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { StudentsComponent } from './students.component';
import { StudentService } from './services/student.service';
import { AuthService, User } from '../authentication/services/auth.service';

describe('StudentsComponent', () => {
  let component: StudentsComponent;
  let fixture: ComponentFixture<StudentsComponent>;
  let studentService: any;
  let authService: any;

  beforeEach(async () => {
    studentService = {
    getStudents: vi.fn(),
    createStudent: vi.fn(),
    updateStudent: vi.fn(),
    updateProgress: vi.fn(),
    deleteStudent: vi.fn()
  } as any;
    studentService.getStudents.mockReturnValue(of([]));
    authService = { currentUser: () => ({ schoolId: 'school-1' } as User) } as any;

    await TestBed.configureTestingModule({
      imports: [StudentsComponent],
      providers: [
        { provide: StudentService, useValue: studentService },
        { provide: AuthService, useValue: authService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(StudentsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load students on init', () => {
    component.ngOnInit();
    expect(studentService.getStudents).toHaveBeenCalled();
  });
});
