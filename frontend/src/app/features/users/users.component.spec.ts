import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { UsersComponent } from './users.component';
import { UserService } from './services/user.service';
import { AuthService, User } from '../authentication/services/auth.service';

describe('UsersComponent', () => {
  let component: UsersComponent;
  let fixture: ComponentFixture<UsersComponent>;
  let userService: jasmine.SpyObj<UserService>;
  let authService: Partial<AuthService>;

  beforeEach(async () => {
    userService = jasmine.createSpyObj('UserService', ['getUsers', 'inviteUser', 'toggleUserStatus']);
    userService.getUsers.and.returnValue(of([]));
    authService = { currentUser: () => ({ schoolId: 'school-1' } as User) } as any;

    await TestBed.configureTestingModule({
      imports: [UsersComponent],
      providers: [
        { provide: UserService, useValue: userService },
        { provide: AuthService, useValue: authService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load users on init', () => {
    component.ngOnInit();
    expect(userService.getUsers).toHaveBeenCalledWith('school-1');
  });
});
