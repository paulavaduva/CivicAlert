import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Issue, IssueSeverity, IssueStatus } from '../models/issue';
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

  startIssue(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/start`, {});
  }

  completeIssue(id: number, formData: FormData): Observable<Issue> {
    return this.http.put<Issue>(`${this.apiUrl}/${id}/complete`, formData);
  }

  getStaffInbox(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/staff-inbox`);
  }

  getMyIssues(): Observable<Issue[]> {
    return this.http.get<Issue[]>(`${this.apiUrl}/my-issues`);
  }

  getStatusText(status: IssueStatus | string | undefined | null): string {
    if (!status) return 'N/A';
    const mapping: Record<string, string> = {
      [IssueStatus.Pending]: 'Pending',
      [IssueStatus.Validated]: 'Validated',
      [IssueStatus.Rejected]: 'Rejected',
      [IssueStatus.Assigned]: 'Assigned',
      [IssueStatus.InProgress]: 'In Progress',
      [IssueStatus.Solved]: 'Solved'
    };
    return mapping[status] || 'Unknown';
  }

  getStatusClass(status: string | undefined): string {
    switch (status) {
      case 'Validated': return 'bg-warning text-dark'; 
      case 'Assigned': return 'bg-info text-white';    
      case 'InProgress': return 'bg-primary text-white'; 
      case 'Solved': return 'bg-success text-white';   
      default: return 'bg-secondary text-white';
    }
  }

  getSeverityText(severity: IssueSeverity | string | undefined | null): string {
    if (!severity) return 'N/A';
    const mapping: Record<string, string> = {
      [IssueSeverity.Urgent]: 'Urgent',
      [IssueSeverity.High]: 'High',
      [IssueSeverity.Medium]: 'Medium',
      [IssueSeverity.Low]: 'Low'
    };
    return mapping[severity] || 'N/A';
  }

  getSeverityClass(severity: IssueSeverity | string | undefined | null): string {
    if (!severity) return 'bg-secondary';
    switch (severity) {
      case IssueSeverity.Urgent: return 'bg-danger text-white';
      case IssueSeverity.High: return 'bg-orange text-white';
      case IssueSeverity.Medium: return 'bg-warning text-dark';
      case IssueSeverity.Low: return 'bg-info text-dark';
      default: return 'bg-secondary';
    }
  }
}