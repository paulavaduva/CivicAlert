export interface Category {
  id: number;
  name: string;
  departmentId: number;
}

export interface CategoryCreateDto {
  name: string;
  departmentId: number;
}