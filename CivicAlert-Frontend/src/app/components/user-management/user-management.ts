import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { DepartmentService } from '../../services/department.service';
import { Department } from '../../models/department';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './user-management.html',
  styleUrl: './user-management.scss'
})
export class UserManagementComponent implements OnInit {
  private authService = inject(AuthService);
  private deptService = inject(DepartmentService);

  users = signal<any[]>([]);
  departments = signal<Department[]>([]);
  activeFilter = signal<'all' | 'staff' | 'citizens'>('all');
  
  // Pentru editare
  editingUserId = signal<string | null>(null);
  editModel: any = {};

  ngOnInit() {
    this.loadUsers();
    this.deptService.getDepartments().subscribe(data => this.departments.set(data));
  }

  loadUsers() {
    const filterValue = this.activeFilter() === 'all' ? undefined : this.activeFilter();
    this.authService.getUsers(filterValue).subscribe(data => this.users.set(data));
  }

  setFilter(filter: 'all' | 'staff' | 'citizens') {
    this.activeFilter.set(filter);
    this.loadUsers();
  }

  onEdit(user: any) {
    this.editingUserId.set(user.id);
    this.editModel = { ...user }; // Facem o copie ca să nu modificăm direct în tabel
  }

  onSaveUpdate() {
    this.authService.updateUser(this.editModel.id, this.editModel).subscribe(() => {
      this.loadUsers();
      this.editingUserId.set(null);
    });
  }

  onDelete(id: string) {
    if(confirm('Ești sigur că vrei să ștergi acest cont?')) {
      this.authService.deleteUser(id).subscribe(() => this.loadUsers());
    }
  }
}