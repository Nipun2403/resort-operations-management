import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequestDTO, RegisterRequestDTO, LoginResponse } from '../models/auth.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl;

  login(credentials: LoginRequestDTO): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, credentials);
  }

  register(data: RegisterRequestDTO): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/register`, data);
  }
}
