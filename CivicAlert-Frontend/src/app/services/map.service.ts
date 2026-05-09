import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class MapService {
  isApiLoaded = signal<boolean>(false);
  private geocoder?: google.maps.Geocoder;

  constructor() {}

  loadGoogleMaps(): Promise<void> {
    return new Promise((resolve) => {
      if (this.isApiLoaded() || typeof google !== 'undefined') {
        this.isApiLoaded.set(true);
        this.geocoder = new google.maps.Geocoder();
        resolve();
        return;
      }

      const script = document.createElement('script');
      script.src = `https://maps.googleapis.com/maps/api/js?key=${environment.googleMapsApiKey}&loading=async&callback=Function.prototype`;
      script.async = true;
      script.defer = true;

      script.onload = () => {
        this.isApiLoaded.set(true);
        this.geocoder = new google.maps.Geocoder();
        resolve();
      };

      document.head.appendChild(script);
    });
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