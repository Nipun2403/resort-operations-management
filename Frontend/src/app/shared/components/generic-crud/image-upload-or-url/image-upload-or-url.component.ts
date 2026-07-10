import { Component, inject, input, output, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';
import { ImageApiService } from '../../../../features/admin/services/image-api.service';

@Component({
  selector: 'app-image-upload-or-url',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="image-upload-or-url">
      <div class="upload-area">
        <input
          #fileInput
          type="file"
          accept="image/jpeg,image/png,image/webp"
          (change)="onFileSelected($event)"
          hidden
        />
        <button
          mat-stroked-button
          type="button"
          (click)="fileInput.click()"
          [disabled]="isUploading()"
          class="upload-btn"
        >
          <mat-icon aria-hidden="true">cloud_upload</mat-icon>
          {{ isUploading() ? 'Uploading...' : 'Choose Image' }}
        </button>
        @if (isUploading()) {
          <mat-spinner diameter="20" class="upload-spinner"></mat-spinner>
        }
      </div>

      <div class="or-divider">or</div>

      <mat-form-field appearance="outline" class="url-field">
        <mat-label>Image URL</mat-label>
        <input
          matInput
          [ngModel]="currentUrl()"
          (ngModelChange)="onUrlChange($event)"
          placeholder="https://..."
        />
        @if (currentUrl()) {
          <button
            mat-icon-button
            matSuffix
            type="button"
            (click)="clearUrl()"
            aria-label="Clear URL"
          >
            <mat-icon aria-hidden="true">close</mat-icon>
          </button>
        }
      </mat-form-field>

      @if (errorMessage()) {
        <div class="error-message">{{ errorMessage() }}</div>
      }

      @if (currentUrl()) {
        <div class="preview">
          <img [src]="currentUrl()" alt="Preview" (error)="onPreviewError()" />
        </div>
      }
    </div>
  `,
  styles: [`
    .image-upload-or-url {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 8px 0;
    }
    .upload-area {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .upload-spinner {
      display: inline-block;
    }
    .or-divider {
      text-align: center;
      color: #888;
      font-size: 0.9em;
    }
    .url-field {
      width: 100%;
    }
    .preview {
      max-width: 200px;
      border-radius: 4px;
      overflow: hidden;
      border: 1px solid #ddd;
    }
    .preview img {
      width: 100%;
      height: auto;
      display: block;
    }
    .error-message {
      color: #f44336;
      font-size: 0.85em;
    }
  `],
})
export class ImageUploadOrUrlComponent {
  readonly value = input<string>('');
  readonly valueChange = output<string>();
  readonly isUploadingChange = output<boolean>();

  readonly isUploading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly currentUrl = signal<string>('');

  private readonly http = inject(HttpClient);
  private readonly imageApi = inject(ImageApiService);

  constructor() {
    effect(() => {
      const v = this.value();
      if (v !== this.currentUrl()) {
        this.currentUrl.set(v ?? '');
      }
    });
  }

  onUrlChange(url: string): void {
    this.currentUrl.set(url);
    this.valueChange.emit(url);
    this.errorMessage.set(null);
  }

  clearUrl(): void {
    this.currentUrl.set('');
    this.valueChange.emit('');
    this.errorMessage.set(null);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.isUploading.set(true);
    this.isUploadingChange.emit(true);
    this.errorMessage.set(null);

    const entityType = this.detectEntityType();
    const request = {
      entityType,
      fileName: file.name,
      contentType: file.type || 'application/octet-stream',
      sizeBytes: file.size,
    };

    this.imageApi.requestUploadSas(request).subscribe({
      next: (sas) => {
        this.uploadToAzure(sas.uploadUrl, file).subscribe({
          next: () => {
            this.imageApi.confirmUpload(sas.sessionId).subscribe({
              next: () => {
                this.currentUrl.set(sas.blobUrl);
                this.valueChange.emit(sas.blobUrl);
                this.isUploading.set(false);
                this.isUploadingChange.emit(false);
              },
              error: () => {
                this.errorMessage.set('Failed to confirm upload.');
                this.isUploading.set(false);
                this.isUploadingChange.emit(false);
              },
            });
          },
          error: () => {
            this.errorMessage.set('Failed to upload file to storage.');
            this.isUploading.set(false);
            this.isUploadingChange.emit(false);
          },
        });
      },
      error: (err) => {
        this.errorMessage.set(err.error?.error || err.message || 'Failed to request upload.');
        this.isUploading.set(false);
        this.isUploadingChange.emit(false);
      },
    });
  }

  private uploadToAzure(url: string, file: File): Observable<void> {
    return this.http.put<void>(url, file, {
      headers: { 'x-ms-blob-type': 'BlockBlob' },
    });
  }

  private detectEntityType(): string {
    const path = window.location.pathname;
    if (path.includes('amenities')) return 'Amenity';
    if (path.includes('menu')) return 'MenuItem';
    if (path.includes('room-types')) return 'RoomType';
    return 'Amenity';
  }

  onPreviewError(): void {
    this.currentUrl.set('');
    this.errorMessage.set('Image failed to load.');
  }
}
