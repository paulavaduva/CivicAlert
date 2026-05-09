import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Department } from '../models/department';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Departments`;

  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(this.apiUrl);
  }

  createDepartment(name: string): Observable<Department> {
    return this.http.post<Department>(this.apiUrl, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}