// src/app/components/login/login.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loginData = {
    email: '',
    password: ''
  };

  constructor(private authService: AuthService, private router: Router) {}

  onLogin() {
  this.authService.login(this.loginData).subscribe({
    next: (response) => {
      console.log('Login reușit! Redirecționăm către Home...');
      this.router.navigate(['/home']);
    },
    error: (err) => {
      console.error('Eroare la login:', err);
      alert(err.error?.message || 'Email sau parolă incorectă!');
    }
  });
}
}