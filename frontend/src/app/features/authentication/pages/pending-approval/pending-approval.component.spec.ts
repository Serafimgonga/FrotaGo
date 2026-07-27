import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';
import { PendingApprovalComponent } from './pending-approval.component';
import { AuthService } from '../../services/auth.service';
import { of } from 'rxjs';

describe('PendingApprovalComponent', () => {
  let component: PendingApprovalComponent;
  let fixture: ComponentFixture<PendingApprovalComponent>;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('AuthService', ['logout', 'updateSchoolPlan']);
    authService.updateSchoolPlan.and.returnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [PendingApprovalComponent, RouterTestingModule],
      providers: [{ provide: AuthService, useValue: authService }],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(PendingApprovalComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call logout when logout is invoked', () => {
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
  });
});
