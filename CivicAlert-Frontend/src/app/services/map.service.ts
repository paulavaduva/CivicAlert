import { Injectable, signal, inject, NgZone } from '@angular/core';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class MapService {
  private zone = inject(NgZone);
  isApiLoaded = signal<boolean>(false);
  private geocoder?: google.maps.Geocoder;

  private map?: google.maps.Map;
  private markers: any[] = [];

  constructor() {}

  loadGoogleMaps(): Promise<void> {
    return new Promise((resolve) => {
      if (typeof google !== 'undefined' && typeof google.maps !== 'undefined') {
        this.isApiLoaded.set(true);
        this.geocoder = new google.maps.Geocoder();
        resolve();
        return;
      }

      const script = document.createElement('script');
      script.src = `https://maps.googleapis.com/maps/api/js?key=${environment.googleMapsApiKey}&callback=Function.prototype`;
      script.async = true;
      script.defer = true;

      script.onload = () => {
       this.zone.run(() => {
          this.isApiLoaded.set(true);
          this.geocoder = new google.maps.Geocoder();
          console.log('Google Maps API loaded successfully');
          resolve();
        });
      };

      document.head.appendChild(script);
    });
  }

  initMap(elementId: string, lat: number = 44.318, lng: number = 23.800): void {
    const mapElement = document.getElementById(elementId);
    if (!mapElement) return;

    this.map = new google.maps.Map(mapElement, {
      center: { lat, lng },
      zoom: 13,
      mapId: '235d6d515b0d9cea3d45655c', 
      mapTypeControl: false,
      streetViewControl: false,
    });
  }

  async addMarker(lat: number, lng: number, title: string, onClick?: () => void) {
    if (!this.map) return;

    const { AdvancedMarkerElement } = await google.maps.importLibrary("marker") as google.maps.MarkerLibrary;

    const marker = new AdvancedMarkerElement({
      map: this.map,
      position: { lat, lng },
      title: title,
    });

    if (onClick) {
      marker.addListener('click', () => {
        this.zone.run(() => onClick());
      });
    }

    this.markers.push(marker);
  }

  clearMarkers(): void {
    this.markers.forEach(m => m.map = null); 
    this.markers = [];
  }

  flyTo(lat: number, lng: number): void {
    if (this.map) {
      this.map.panTo({ lat, lng });
      this.map.setZoom(16);
    }
  }

  async getAddress(lat: number, lng: number): Promise<string> {
    if (!this.geocoder) this.geocoder = new google.maps.Geocoder();

    try {
      const response = await this.geocoder.geocode({ location: { lat, lng } });
      if (response.results[0]) {
        return response.results[0].formatted_address.split(',')[0];
      }
      return 'Locație necunoscută';
    } catch {
      return 'Adresă indisponibilă';
    }
  }
}