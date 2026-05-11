import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Issue } from '../models/issue';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class IssueService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/Issues`;

  getIssues(): Observable<Issue[]> {
    return this.http.get<Issue[]>(this.apiUrl);
  }

  getIssueById(id: number): Observable<Issue> {
    return this.http.get<Issue>(`${this.apiUrl}/${id}`);
  }

  createIssue(formData: FormData): Observable<Issue> {
    return this.http.post<Issue>(this.apiUrl, formData);
  }

  updateIssue(id: number, dto: any): Observable<Issue> {
    return this.http.put<Issue>(`${this.apiUrl}/${id}`, dto);
  }

  validateIssue(id: number, isApproved: boolean): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/validate`, isApproved);
  }

  assignIssue(id: number, teamLeaderId: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/assign`, JSON.stringify(teamLeaderId), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  completeIssue(id: number, formData: FormData): Observable<Issue> {
    return this.http.patch<Issue>(`${this.apiUrl}/${id}/complete`, formData);
  }

  getStaffInbox(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/staff-inbox`);
  }
}