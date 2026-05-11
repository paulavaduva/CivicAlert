import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { DepartmentService } from '../../services/department.service';
import { Department } from '../../models/department';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register-staff',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register-staff.html',
  styleUrl: './register-staff.scss'
})
export class RegisterStaffComponent implements OnInit {
  private authService = inject(AuthService);
  private deptService = inject(DepartmentService);

  departments = signal<Department[]>([]);
  roles = ['Dispatcher', 'HOD', 'TeamLeader']; // Rolurile posibile pentru staff
  
  staffModel = {
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    role: '',
    departmentId: 0
  };

  isSubmitting = false;
  message = signal({ text: '', type: '' });

  ngOnInit() {
    // Încărcăm departamentele pentru a le pune în dropdown
    this.deptService.getDepartments().subscribe(data => this.departments.set(data));
  }

  onSubmit() {
    this.isSubmitting = true;
    this.authService.registerStaff(this.staffModel).subscribe({
      next: (res) => {
        this.message.set({ text: 'Cont creat cu succes!', type: 'success' });
        this.resetForm();
        this.isSubmitting = false;
      },
      error: (err) => {
        const errorMsg = err.error?.errors ? err.error.errors.join(', ') : 'Eroare la crearea contului.';
        this.message.set({ text: errorMsg, type: 'danger' });
        this.isSubmitting = false;
      }
    });
  }

  resetForm() {
    this.staffModel = { email: '', password: '', firstName: '', lastName: '', phoneNumber: '', role: '', departmentId: 0 };
  }
}