import { Component, OnInit, inject, signal, effect, AfterViewInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IssueService } from '../../services/issue';
import { AuthService } from '../../services/auth.service';
import { MapService } from '../../services/map.service';
import { Issue, IssueStatus, IssueSeverity } from '../../models/issue';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-issue-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './issue-management.html',
  styleUrl: './issue-management.scss'
})
export class IssueManagementComponent implements OnInit, AfterViewInit {
  public issueService = inject(IssueService);
  public authService = inject(AuthService);
  private mapService = inject(MapService);

  teamLeaders = signal<any[]>([]);
  selectedTeamLeaderId = '';

  issues = signal<Issue[]>([]);
  selectedIssue = signal<Issue | null>(null);
  isLoading = signal(true);
  isMapReady = signal(false);

  isImageModalOpen = signal(false);
  isResolvedImageModalOpen = signal(false);

  selectedFilter = signal<string>('All');

  selectedFile: File | null = null;

  constructor() {
    effect(() => {
      const issue = this.selectedIssue();
      if (issue && issue.latitude && issue.longitude && this.isMapReady()) {
        this.mapService.flyTo(issue.latitude, issue.longitude);
      }
    });
    effect(() => {
      const issues = this.filteredIssues(); 
      if (this.isMapReady()) {
        this.updateMapMarkers();
      }
    });
  }

  ngOnInit() {
    this.loadIssues();

    const deptId = this.authService.userDeptId(); 
    if (this.authService.userRole() === 'HOD' && deptId) {
      this.authService.getTeamLeaders(deptId).subscribe(data => {
        this.teamLeaders.set(data);
      });
    }
  }

  async ngAfterViewInit() {
    await this.mapService.loadGoogleMaps();
    this.mapService.initMap('adminMap');
    this.isMapReady.set(true);
  }

  loadIssues() {
    this.isLoading.set(true);
    this.issueService.getStaffInbox().subscribe({
      next: (data) => {
        
        let filteredData = data;

        this.issues.set(filteredData);
        this.isLoading.set(false);

        const current = this.selectedIssue();
        if (current) {
          const freshData = data.find((i: Issue) => i.id === current.id);
          if (freshData) {
            this.selectedIssue.set(freshData);
          }
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  async updateMapMarkers() {
    this.mapService.clearMarkers();
    for (const issue of this.filteredIssues()) {
      if (issue.latitude && issue.longitude) {
        await this.mapService.addMarker(issue.latitude, issue.longitude, issue.name, () => {
          this.selectedIssue.set(issue);
        });
      }
    }
  }

  onValidate(id: number, approved: boolean) {
    if (confirm(approved ? 'Aprobi sesizarea?' : 'Respingi sesizarea?')) {
      this.issueService.validateIssue(id, approved).subscribe({
        next: () => { this.selectedIssue.set(null); this.loadIssues(); }
      });
    }
  }



  onAssign() {
    const issueId = this.selectedIssue()?.id;
    const tlId = this.selectedTeamLeaderId;

    if (!issueId || !tlId) return;

    this.issueService.assignIssue(issueId, tlId).subscribe({
      next: () => {
        alert('Sesizarea a fost asignată!');
        this.selectedIssue.set(null);
        this.selectedTeamLeaderId = '';
        this.loadIssues();
      }
    });
  }

  filteredIssues = computed(() => {
    const filterValue = this.selectedFilter();
    const allIssues = this.issues();

    if (filterValue === 'All') {
      return allIssues;
    }
    
    return allIssues.filter(i => i.status === filterValue);
  });
  onFilterChange() {
    this.selectedIssue.set(null); 
  }

  onStartWork() {
    const currentIssue = this.selectedIssue();
    if (!currentIssue) return;

    this.issueService.startIssue(currentIssue.id).subscribe({
      next: () => {
        this.selectedIssue.set({
          ...currentIssue,
          status: IssueStatus.InProgress
        });

        this.loadIssues();

        console.log('Status actualizat local în InProgress');
      },
      error: (err) => console.error('Eroare la pornirea lucrării', err)
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onCompleteWork() {
    const issue = this.selectedIssue();
    if (!issue || !this.selectedFile) {
      alert('Te rugăm să selectezi o imagine dovadă înainte de finalizare.');
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.issueService.completeIssue(issue.id, formData).subscribe({
      next: () => {
        alert('Sesizare finalizată cu succes!');
        this.selectedFile = null;
        this.selectedIssue.set(null); 
        this.loadIssues();
      },
      error: (err) => alert('Eroare la finalizarea sesizării. Verifică dimensiunea imaginii.')
    });
  }
}