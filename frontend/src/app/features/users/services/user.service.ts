import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserItem {
  id: string;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  schoolId?: string | null;
  createdAt: string;
}

export interface InviteUserRequest {
  schoolId: string;
  name: string;
  email: string;
  role: number; // 2: SchoolOwner, 3: SchoolAdmin, 4: Receptionist, 5: Instructor, 6: Financial
}

export interface AcceptInvitationRequest {
  token: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = '/api/users';

  constructor(private http: HttpClient) {}

  getUsers(schoolId: string): Observable<UserItem[]> {
    return this.http.get<UserItem[]>(`${this.apiUrl}/school/${schoolId}`);
  }

  inviteUser(request: InviteUserRequest): Observable<{ token: string; message: string }> {
    return this.http.post<{ token: string; message: string }>(`${this.apiUrl}/invite`, request);
  }

  acceptInvitation(request: AcceptInvitationRequest): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/accept-invitation`, request);
  }

  toggleUserStatus(userId: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/${userId}/toggle-status`, {});
  }
}
