import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface UploadSasRequest {
  entityType: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  entityId?: number;
}

export interface UploadSasResponse {
  sessionId: string;
  uploadUrl: string;
  blobUrl: string;
  expiresOn: string;
}

@Injectable({
  providedIn: 'root',
})
export class ImageApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/images`;

  requestUploadSas(dto: UploadSasRequest): Observable<UploadSasResponse> {
    return this.http.post<UploadSasResponse>(`${this.baseUrl}/upload-sas`, dto);
  }

  confirmUpload(sessionId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${sessionId}/confirm`, {});
  }

  getStatus(sessionId: string): Observable<{ status: string; rejectionReason?: string }> {
    return this.http.get<{ status: string; rejectionReason?: string }>(`${this.baseUrl}/${sessionId}/status`);
  }
}
