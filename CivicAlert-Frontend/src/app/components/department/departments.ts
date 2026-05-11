import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentService } from '../../services/department.service';
import { Department } from '../../models/department';

@Component({
  selector: 'app-departments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './departments.html',
  styleUrl: './departments.scss'
})
export class DepartmentComponent implements OnInit {
  private deptService = inject(DepartmentService);

  departments = signal<Department[]>([]);
  newDeptName = '';
  isSubmitting = false;

  // Stare pentru Editare
  editingId = signal<number | null>(null);
  editName = '';

  ngOnInit() {
    this.loadDepartments();
  }

  loadDepartments() {
    this.deptService.getDepartments().subscribe({
      next: (data) => this.departments.set(data),
      error: (err) => console.error('Eroare load:', err)
    });
  }

  onCreate() {
    if (!this.newDeptName.trim()) return;
    this.isSubmitting = true;
    this.deptService.createDepartment(this.newDeptName).subscribe({
      next: (newDept) => {
        this.departments.update(list => [...list, newDept]);
        this.newDeptName = '';
        this.isSubmitting = false;
      },
      error: () => this.isSubmitting = false
    });
  }

  onEdit(dept: Department) {
    this.editingId.set(dept.id);
    this.editName = dept.name;
  }

  saveEdit() {
    const id = this.editingId();
    if (!id || !this.editName.trim()) return;

    this.deptService.updateDepartment(id, this.editName).subscribe({
      next: (updatedDept) => {
        this.departments.update(list => 
          list.map(d => d.id === id ? updatedDept : d)
        );
        this.cancelEdit();
      },
      error: (err) => console.error('Eroare update:', err)
    });
  }

  cancelEdit() {
    this.editingId.set(null);
    this.editName = '';
  }

  onDelete(id: number) {
    if (confirm('Sigur vrei să ștergi acest departament? Această acțiune este ireversibilă.')) {
      this.deptService.deleteDepartment(id).subscribe({
        next: () => {
          this.departments.update(list => list.filter(d => d.id !== id));
        },
        error: (err) => console.error('Eroare delete:', err)
      });
    }
  }
}