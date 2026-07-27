import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { AcceptInvitationComponent } from './accept-invitation.component';
import { UserService } from '../../../users/services/user.service';

describe('AcceptInvitationComponent', () => {
  let component: AcceptInvitationComponent;
  let fixture: ComponentFixture<AcceptInvitationComponent>;
  let userService: jasmine.SpyObj<UserService>;

  beforeEach(async () => {
    userService = jasmine.createSpyObj('UserService', ['acceptInvitation']);
    userService.acceptInvitation.and.returnValue(of({ success: true, message: 'Sucesso' }));

    await TestBed.configureTestingModule({
      imports: [AcceptInvitationComponent, RouterTestingModule],
      providers: [
        { provide: UserService, useValue: userService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParamMap: convertToParamMap({ token: 'abc' }) },
            queryParamMap: of(convertToParamMap({ token: 'abc' }))
          }
        }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(AcceptInvitationComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize token from query param', () => {
    component.ngOnInit();
    expect(component.token()).toBe('abc');
  });
});
