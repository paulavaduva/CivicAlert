export interface Issue {
  id: number;
  name: string;
  description: string;
  latitude: number;
  longitude: number;
  address?: string; 
  severity: any;    
  status: any;
  createdAt: Date;
  imageUrl?: string;   
}