import { Component, OnInit, inject, signal, effect, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IssueService } from '../../services/issue';
import { AuthService } from '../../services/auth.service';
import { MapService } from '../../services/map.service';

@Component({
  selector: 'app-issue-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './issue-management.html',
  styleUrl: './issue-management.scss'
})
export class IssueManagementComponent implements OnInit, AfterViewInit {
  private issueService = inject(IssueService);
  public authService = inject(AuthService);
  private mapService = inject(MapService);

  issues = signal<any[]>([]);
  selectedIssue = signal<any | null>(null);
  isLoading = signal(true);
  isMapReady = signal(false);

  constructor() {
    effect(() => {
      const issue = this.selectedIssue();
      if (issue && issue.latitude && issue.longitude && this.isMapReady()) {
        this.mapService.flyTo(issue.latitude, issue.longitude);
      }
    });
  }

  ngOnInit() {
    this.loadIssues();
  }

  async ngAfterViewInit() {
    await this.mapService.loadGoogleMaps();
    this.mapService.initMap('adminMap');
    this.isMapReady.set(true);
    if (this.issues().length > 0) this.updateMapMarkers();
  }

  loadIssues() {
  this.isLoading.set(true);
  this.issueService.getStaffInbox().subscribe({
    next: (data) => {
      const role = this.authService.userRole();
      
      let filteredData = data;

      if (role === 'Dispatcher') {
        filteredData = data.filter((issue: any) => issue.status === 0 || issue.status === 'Pending');
      } else if (role === 'HOD') {
        filteredData = data.filter((issue: any) => issue.status === 1 || issue.status === 'Validated');
      }

      this.issues.set(filteredData);
      this.isLoading.set(false);
      
      if (this.isMapReady()) {
        this.updateMapMarkers();
      }
    },
    error: () => this.isLoading.set(false)
  });
}

  async updateMapMarkers() {
    this.mapService.clearMarkers();
    for (const issue of this.issues()) {
      if (issue.latitude && issue.longitude) {
        await this.mapService.addMarker(issue.latitude, issue.longitude, issue.title, () => {
          this.selectedIssue.set(issue);
        });
      }
    }
  }

  getStatusText(status: number): string {
    const texts: any = { 0: 'Pending', 1: 'Validated', 2: 'Rejected', 3: 'In Progress', 4: 'Solved' };
    return texts[status] || 'Unknown';
  }

  getUrgencyText(severity: any): string {
    if (severity == 4 || severity === 'Urgent') return 'Urgenta';
    if (severity == 3 || severity === 'High') return 'Ridicată';
    if (severity == 2 || severity === 'Medium') return 'Medie';
    if (severity == 1 || severity === 'Low') return 'Scăzută';
    
    return 'N/A'; 
  }

  getUrgencyClass(severity: any): string {
    if (severity == 4 || severity === 'Urgent') return 'bg-danger text-white';
    if (severity == 3 || severity === 'High') return 'bg-orange text-white';
    if (severity == 2 || severity === 'Medium') return 'bg-warning text-dark';
    if (severity == 1 || severity === 'Low') return 'bg-info text-dark';
    
    return 'bg-secondary';
  }

  onValidate(id: number, approved: boolean) {
    if (confirm(approved ? 'Aprobi sesizarea?' : 'Respingi sesizarea?')) {
      this.issueService.validateIssue(id, approved).subscribe({
        next: () => { this.selectedIssue.set(null); this.loadIssues(); }
      });
    }
  }

  isImageModalOpen = signal(false);
}