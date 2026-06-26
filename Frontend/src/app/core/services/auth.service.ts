import { Injectable, signal, computed } from '@angular/core';
import { jwtDecode } from '../utils/jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  token = signal<string | null>(null);
  role = signal<string | null>(null);

  isAuthenticated = computed(() => !!this.token() && !this.isTokenExpired());

  constructor() {
    const savedToken = localStorage.getItem('token');
    if (savedToken) {
      this.token.set(savedToken);
      const decoded = jwtDecode(savedToken);
      if (decoded && decoded.role) {
        this.role.set(decoded.role);
      }
    }
  }

  handleLogin(token: string): void {
    localStorage.setItem('token', token);
    this.token.set(token);
    const decoded = jwtDecode(token);
    if (decoded && decoded.role) {
      this.role.set(decoded.role);
    } else {
      this.role.set(null);
    }
  }

  logout(): void {
    localStorage.removeItem('token');
    this.token.set(null);
    this.role.set(null);
  }

  private isTokenExpired(): boolean {
    const currentToken = this.token();
    if (!currentToken) {
      return true;
    }
    const decoded = jwtDecode(currentToken);
    if (!decoded || !decoded.exp) {
      return true;
    }
    return Date.now() >= decoded.exp * 1000;
  }
}
