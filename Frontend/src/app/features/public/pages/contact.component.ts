import { Component, AfterViewInit, OnInit, ElementRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss']
})
export class ContactComponent implements AfterViewInit, OnInit {
  private el = inject(ElementRef);
  private fb = inject(FormBuilder);

  ngOnInit(): void {
    window.scrollTo(0, 0);
  }

  contactForm: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    subject: ['General Inquiry', [Validators.required]],
    message: ['', [Validators.required, Validators.minLength(10)]]
  });

  loading = signal(false);
  submitted = signal(false);

  ngAfterViewInit(): void {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.1 }
    );

    const sections = this.el.nativeElement.querySelectorAll('.content-section');
    sections.forEach((sec: HTMLElement) => observer.observe(sec));
  }

  onSubmit(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    // Simulate luxury concierge processing time
    setTimeout(() => {
      this.loading.set(false);
      this.submitted.set(true);
      this.contactForm.reset({
        fullName: '',
        email: '',
        subject: 'General Inquiry',
        message: ''
      });
    }, 1000);
  }

  resetForm(): void {
    this.submitted.set(false);
  }
}
