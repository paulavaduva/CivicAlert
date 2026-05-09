import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerData = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    phoneNumber: ''
  };

  constructor(private authService: AuthService, private router: Router) {}

  onRegister() {
    console.log('Date trimise la register:', this.registerData);

    this.authService.register(this.registerData).subscribe({
      next: (response) => {
        alert('Cont creat cu succes!');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('Eroare backend:', err);
        alert('Înregistrare eșuată. Verifică datele introduse.');
      }
    });
  }
}