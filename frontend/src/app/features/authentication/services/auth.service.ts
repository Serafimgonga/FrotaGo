import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

export interface User {
  userId?: string;
  name: string;
  email: string;
  role: string;
  schoolId?: string | null;
  schoolName?: string | null;
  schoolStatus?: string | null;
  permissions?: string[];
}

export interface LoginResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
  role: string;
  schoolId?: string | null;
  schoolName?: string | null;
  schoolStatus?: string | null;
  permissions: string[];
}

export interface RegisterSchoolRequest {
  // Passo 1
  ownerName: string;
  gender?: string;
  phone: string;
  email: string;
  identityCardNumber?: string;
  password: string;

  // Passo 2
  schoolName: string;
  shortName: string;
  slug: string;
  nif: string;
  licenseNumber: string;
  licenseIssuer: string;
  licenseIssueDate?: string;
  schoolPhone?: string;
  schoolEmail?: string;
  website?: string;
  province: string;
  municipality: string;
  address: string;
  latitude?: number;
  longitude?: number;

  // Passo 3
  branchName: string;
  branchPhone?: string;
  branchAddress?: string;

  // Passo 4
  plan: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = '/api/auth';
  
  readonly token = signal<string | null>(localStorage.getItem('token'));
  readonly currentUser = signal<User | null>(this.getUserFromStorage());

  readonly isAuthenticated = computed(() => !!this.token());

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        const user: User = {
          userId: res.userId,
          name: res.name,
          email: res.email,
          role: res.role,
          schoolId: res.schoolId,
          schoolName: res.schoolName,
          schoolStatus: res.schoolStatus,
          permissions: res.permissions || []
        };
        localStorage.setItem('user', JSON.stringify(user));
        this.token.set(res.token);
        this.currentUser.set(user);
      })
    );
  }

  registerSchool(data: RegisterSchoolRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/register-school`, data).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        const user: User = {
          userId: res.userId,
          name: res.name,
          email: res.email,
          role: res.role,
          schoolId: res.schoolId,
          schoolName: res.schoolName,
          schoolStatus: res.schoolStatus,
          permissions: res.permissions || []
        };
        localStorage.setItem('user', JSON.stringify(user));
        this.token.set(res.token);
        this.currentUser.set(user);
      })
    );
  }

  register(user: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  updateSchoolPlan(schoolId: string, newPlan: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-plan`, { schoolId, newPlan }).pipe(
      tap(() => {
        if (newPlan === 'Gratuito') {
          const user = this.currentUser();
          if (user) {
            const updatedUser: User = { ...user, schoolStatus: 'Approved' };
            localStorage.setItem('user', JSON.stringify(updatedUser));
            this.currentUser.set(updatedUser);
          }
        }
      })
    );
  }

  hasPermission(permission: string): boolean {
    const u = this.currentUser();
    if (!u) return false;
    if (u.role === 'SuperAdmin' || u.role === 'SchoolOwner') return true;
    return u.permissions?.includes(permission) ?? false;
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.token.set(null);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  private getUserFromStorage(): User | null {
    const userJson = localStorage.getItem('user');
    if (!userJson) return null;
    try {
      return JSON.parse(userJson) as User;
    } catch {
      return null;
    }
  }
}
