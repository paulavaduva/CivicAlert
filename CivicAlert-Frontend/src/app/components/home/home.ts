import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleMapsModule } from '@angular/google-maps';
import { IssueService } from '../../services/issue';
import { MapService } from '../../services/map.service';
import { Issue, IssueSeverity, IssueStatus } from '../../models/issue';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, GoogleMapsModule], 
  templateUrl: './home.html', 
  styleUrl: './home.scss'    
})
export class HomeComponent implements OnInit {
  public issueService = inject(IssueService);
  private mapService = inject(MapService);
  private cdr = inject(ChangeDetectorRef);

  issues: Issue[] = []; 
  selectedIssue: Issue | null = null;

  center: google.maps.LatLngLiteral = { lat: 44.318, lng: 23.800 };
  zoom = 14;
  
  get apiLoaded() { return this.mapService.isApiLoaded(); }

  options: google.maps.MapOptions = {
    disableDefaultUI: true,
    zoomControl: true,
    styles: [
      {
        featureType: "poi",
        elementType: "labels",
        stylers: [{ visibility: "off" }] 
      }
    ]
  };

  ngOnInit() {
    this.mapService.loadGoogleMaps().then(() => {
      this.fetchIssues();
      this.cdr.detectChanges();
    });
  }

  fetchIssues() {
    this.issueService.getIssues().subscribe({
      next: (data) => {
        this.issues = data.filter(issue => issue.status !== 'Solved');
        // this.enrichIssuesWithAddresses();
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Eroare date:', err)
    });
  } 

  // async enrichIssuesWithAddresses() {
  //   for (const issue of this.issues) {
  //     if (!issue.locationName) { 
  //       issue.locationName = await this.mapService.getAddress(issue.latitude, issue.longitude);
  //       this.cdr.detectChanges(); 
  //     }
  //   }
  // }

  onIssueClick(issue: Issue) {
    this.selectedIssue = issue;
    this.center = { lat: issue.latitude, lng: issue.longitude };
    this.zoom = 17; 
    this.cdr.detectChanges();
  }
}