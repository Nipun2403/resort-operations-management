import { Component, AfterViewInit, OnDestroy, ElementRef, Renderer2, inject, PLATFORM_ID, ViewChild } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-custom-cursor',
  standalone: true,
  imports: [],
  templateUrl: './custom-cursor.component.html',
  styleUrls: ['./custom-cursor.component.scss'],
})
export class CustomCursorComponent implements AfterViewInit, OnDestroy {
  @ViewChild('cursor', { static: true }) cursorRef!: ElementRef<HTMLElement>;
  private cursorEl!: HTMLElement;
  private renderer = inject(Renderer2);
  private document = inject(DOCUMENT);
  private platformId = inject(PLATFORM_ID);

  private readonly INTERACTIVE_SELECTOR = 'a, button, .cursor-hover, mat-slide-toggle, mat-icon-button, [role="button"]';
  private readonly INPUT_SELECTOR = 'input, textarea, select, mat-select, .mat-mdc-input-element';

  private rafId: number | null = null;
  private mouseX = 0;
  private mouseY = 0;

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.cursorEl = this.cursorRef.nativeElement;
    this.document.addEventListener('mousemove', this.onMouseMove);
    this.document.addEventListener('mouseover', this.onMouseOver);
    this.document.addEventListener('mouseout', this.onMouseOut);
    this.updatePosition();
  }

  ngOnDestroy(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.document.removeEventListener('mousemove', this.onMouseMove);
    this.document.removeEventListener('mouseover', this.onMouseOver);
    this.document.removeEventListener('mouseout', this.onMouseOut);
    if (this.rafId) cancelAnimationFrame(this.rafId);
  }

  private onMouseMove = (e: MouseEvent): void => {
    this.mouseX = e.clientX;
    this.mouseY = e.clientY;
  };

  private onMouseOver = (e: MouseEvent): void => {
    const target = e.target as HTMLElement;
    if (!target) return;
    if (target.matches(this.INTERACTIVE_SELECTOR) || target.closest(this.INTERACTIVE_SELECTOR)) {
      this.renderer.addClass(this.cursorEl, 'enlarged');
    } else if (target.matches(this.INPUT_SELECTOR) || target.closest(this.INPUT_SELECTOR)) {
      this.renderer.addClass(this.cursorEl, 'oval');
    }
  };

  private onMouseOut = (e: MouseEvent): void => {
    const target = e.target as HTMLElement;
    if (!target) return;
    if (target.matches(this.INTERACTIVE_SELECTOR) || target.closest(this.INTERACTIVE_SELECTOR)) {
      this.renderer.removeClass(this.cursorEl, 'enlarged');
    }
    if (target.matches(this.INPUT_SELECTOR) || target.closest(this.INPUT_SELECTOR)) {
      this.renderer.removeClass(this.cursorEl, 'oval');
    }
  };

  private updatePosition(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.rafId = requestAnimationFrame(() => {
        this.renderer.setStyle(this.cursorEl, 'left', `${this.mouseX}px`);
        this.renderer.setStyle(this.cursorEl, 'top', `${this.mouseY}px`);
        this.updatePosition();
      });
    }
  }
}
