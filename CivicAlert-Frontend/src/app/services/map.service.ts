import { Injectable, signal, inject, NgZone } from '@angular/core';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class MapService {
  private zone = inject(NgZone);
  isApiLoaded = signal<boolean>(false);
  private geocoder?: google.maps.Geocoder;

  public map?: google.maps.Map;
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

      script.src = `https://maps.googleapis.com/maps/api/js?key=${environment.googleMapsApiKey}&libraries=places&callback=Function.prototype`;
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

  getCurrentLocation(): Promise<google.maps.LatLngLiteral> {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject('Geolocația nu este suportată de acest browser.');
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            lat: position.coords.latitude,
            lng: position.coords.longitude
          });
        },
        (error) => {
          reject(error);
        },
        {
          enableHighAccuracy: true,
          timeout: 15000,
          maximumAge: 0
        }
      );
    });
  }

  // În map.service.ts

  async setupAutocomplete(inputElement: HTMLInputElement, onSelect: (lat: number, lng: number, address: string) => void) {
    // Importăm biblioteca "places" în mod dinamic (metoda recomandată de Google acum)
    const { Autocomplete } = await google.maps.importLibrary("places") as google.maps.PlacesLibrary;

    const autocomplete = new Autocomplete(inputElement, {
      componentRestrictions: { country: 'ro' },
      // 'location' este acum inclus în 'geometry' sau cerut separat în versiunile noi
      fields: ['geometry', 'formatted_address', 'name'] 
    });

    // Această funcție se declanșează când alegi o sugestie SAU când dai Enter pe o sugestie selectată
    autocomplete.addListener('place_changed', () => {
      this.zone.run(() => {
        const place = autocomplete.getPlace();
        
        if (!place.geometry || !place.geometry.location) {
          console.warn("Locația nu a putut fi găsită pentru acest input.");
          return;
        }

        const lat = place.geometry.location.lat();
        const lng = place.geometry.location.lng();
        const address = place.formatted_address || place.name || '';
        
        onSelect(lat, lng, address);
      });
    });
  }
}