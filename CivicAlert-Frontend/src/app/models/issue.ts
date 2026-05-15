import { UserDto } from "./user";

export enum IssueStatus {
  Pending = 'Pending',
  Validated = 'Validated',
  Rejected = 'Rejected',
  Assigned = 'Assigned',
  InProgress = 'InProgress',
  Solved = 'Solved'
}

export enum IssueSeverity {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Urgent = 'Urgent'
}

export interface Issue {
  id: number;
  name: string;
  description: string;
  latitude: number;
  longitude: number;
  address?: string; 
  severity: IssueSeverity; 
  status: IssueStatus;
  createdAt: Date;
  updatedAt: Date;
  imageUrl?: string;  
  categoryName?: string;
  reporter?: UserDto;
  assignedToUser?: UserDto; 
  resolvedImageUrl?: string;
  isValid: boolean;          
  aiConfidenceScore?: number; 
  aiValidationReason?: string;
}