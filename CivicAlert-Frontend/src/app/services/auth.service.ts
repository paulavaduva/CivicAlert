import { Injectable, signal, inject, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/account`; 

  isLoggedIn = signal<boolean>(!!localStorage.getItem('civic_token'));
  userRole = signal<string | null>(this.getFieldValueFromToken('role'));
  userName = signal<string | null>(this.getFieldValueFromToken('firstName'));
  userDeptId = signal<number | null>(Number(this.getFieldValueFromToken('deptId')) || null);

  isAdmin = computed(() => this.userRole() === 'Admin');
  isHOD = computed(() => this.userRole() === 'HOD');
  isStaff = computed(() => ['Admin', 'Dispatcher', 'HOD', 'TeamLeader'].includes(this.userRole() || ''));

  constructor() { }

  login(credentials: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response && response.accessToken) {
          localStorage.setItem('civic_token', response.accessToken);
          this.updateSignals();
        }
      })
    );
  }

  register(user: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  registerStaff(staffModel: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register-staff`, staffModel);
  }

  getCurrentUserInfo(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/user-info`);
  }

  logout() {
    localStorage.removeItem('civic_token');
    this.isLoggedIn.set(false);
    this.userRole.set(null);
    this.userName.set(null);
    this.userDeptId.set(null);
  }

  private updateSignals() {
    this.isLoggedIn.set(true);
    this.userRole.set(this.getFieldValueFromToken('role'));
    this.userName.set(this.getFieldValueFromToken('firstName'));
    const deptId = this.getFieldValueFromToken('deptId');
    this.userDeptId.set(deptId ? Number(deptId) : null);
  }

  private getFieldValueFromToken(field: string): string | null {
    const token = localStorage.getItem('civic_token');
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      
      return decoded[field] || null;
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    return localStorage.getItem('civic_token');
  }
}