import { Injectable, signal, computed } from '@angular/core';
import { jwtDecode, JwtPayload } from '../utils/jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  token = signal<string | null>(null);
  role = signal<string | null>(null);
  private _decodedToken = signal<JwtPayload | null>(null);

  isAuthenticated = computed(() => !!this.token() && !this.isTokenExpired());
  fullName = computed(() => {
    const t = this._decodedToken();
    if (!t) {
      return 'Admin';
    }
    const name = `${t.firstName || ''} ${t.lastName || ''}`.trim();
    return name || t.role || 'Admin';
  });

  constructor() {
    const savedToken = localStorage.getItem('token');
    if (savedToken) {
      this.token.set(savedToken);
      this.decodeAndStore(savedToken);
    }
  }

  handleLogin(token: string): void {
    localStorage.setItem('token', token);
    this.token.set(token);
    this.decodeAndStore(token);
  }

  logout(): void {
    localStorage.removeItem('token');
    this.token.set(null);
    this.decodeAndStore(null);
  }

  private decodeAndStore(token: string | null): void {
    if (token) {
      const decoded = jwtDecode(token);
      this._decodedToken.set(decoded);
      if (decoded && decoded.role) {
        this.role.set(decoded.role);
      } else {
        this.role.set(null);
      }
    } else {
      this._decodedToken.set(null);
      this.role.set(null);
    }
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
