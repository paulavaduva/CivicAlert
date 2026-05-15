import { Component, OnInit, inject, signal, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { IssueService } from '../../services/issue';
import { CategoryService } from '../../services/category.service';
import { MapService } from '../../services/map.service';
import { Category } from '../../models/category';
import { IssueSeverity } from '../../models/issue';

@Component({
  selector: 'app-issue-report',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './issue-report.html',
  styleUrl: './issue-report.scss'
})
export class IssueReportComponent implements OnInit {
  private fb = inject(FormBuilder);
  private issueService = inject(IssueService);
  private categoryService = inject(CategoryService);
  private mapService = inject(MapService);
  private router = inject(Router);
  private zone = inject(NgZone);

  IssueSeverity = IssueSeverity;

  reportForm: FormGroup;
  categories = signal<Category[]>([]);
  imagePreview = signal<string | null>(null);
  isSubmitting = signal(false);
  selectedFile: File | null = null;

  analysisStatus = signal<string>('');

  constructor() {
    this.reportForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(5)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      // severity: [IssueSeverity.Medium, Validators.required],
      // categoryId: ['', Validators.required],
      latitude: [null, Validators.required],
      longitude: [null, Validators.required],
      address: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.categoryService.getCategories().subscribe(data => {
      this.categories.set(data);
    });

    this.setupMap();
  }

  async setupMap() {
    await this.mapService.loadGoogleMaps();
    this.mapService.initMap('reportMap');

    const nativeMap = (this.mapService as any).map; 

    if (nativeMap) {
      nativeMap.addListener('click', (event: google.maps.MapMouseEvent) => {
        if (event.latLng) {
          this.zone.run(() => {
            this.handleLocationSelect(event.latLng!.lat(), event.latLng!.lng());
          });
        }
      });
    }

    const input = document.getElementById('addressSearch') as HTMLInputElement;
    if (input) {

    await this.mapService.setupAutocomplete(input, (lat, lng, address) => {
        this.handleLocationSelect(lat, lng);
        this.mapService.flyTo(lat, lng);
        this.reportForm.patchValue({ address: address });
    });
    }
  }

  async handleLocationSelect(lat: number, lng: number) {
    this.reportForm.patchValue({ latitude: lat, longitude: lng });

    this.mapService.clearMarkers();
    await this.mapService.addMarker(lat, lng, "Locația incidentului");

    const address = await this.mapService.getAddress(lat, lng);
    this.reportForm.patchValue({ address: address });
  }

  async getUserLocation() {
    try {
      const coords = await this.mapService.getCurrentLocation();
      await this.handleLocationSelect(coords.lat, coords.lng);
      this.mapService.flyTo(coords.lat, coords.lng);
    } catch (error) {
      alert('Nu am putut accesa locația ta. Te rugăm să selectezi manual pe hartă.');
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      
      const reader = new FileReader();
      reader.onload = () => this.imagePreview.set(reader.result as string);
      reader.readAsDataURL(file);
    }
  }

  onSubmit() {
    if (this.reportForm.invalid || !this.selectedFile) {
      alert('Te rugăm să completezi toate câmpurile, să selectezi o locație și o imagine.');
      return;
    }

    this.isSubmitting.set(true);
    this.analysisStatus.set('AI-ul analizează imaginea și verifică duplicatele...');

    const formData = new FormData();
    formData.append('name', this.reportForm.value.name);
    formData.append('description', this.reportForm.value.description);
    formData.append('severity', '1');
    formData.append('categoryId', '0');
    formData.append('latitude', this.reportForm.value.latitude); 
    formData.append('longitude', this.reportForm.value.longitude);
    formData.append('address', this.reportForm.value.address);
    formData.append('image', this.selectedFile);

    this.issueService.createIssue(formData).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        alert('Sesizarea a fost procesată de AI și trimisă cu succes!');
        this.router.navigate(['/home']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.analysisStatus.set('');

        if (err.status === 409) {
          alert((err.error?.message || 'Această problemă a fost deja raportată în această locație. Îți mulțumim pentru implicare!'));
        } else {
          console.error('Eroare la trimitere:', err);
          alert('A apărut o eroare la procesarea AI. Te rugăm să încerci din nou.');
        }
      }
    });
  }
}