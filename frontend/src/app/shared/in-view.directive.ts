import { Directive, ElementRef, OnDestroy, OnInit, inject, output } from '@angular/core';

/** Emits whenever the host scrolls into view; used for infinite scrolling. */
@Directive({ selector: '[appInView]' })
export class InViewDirective implements OnInit, OnDestroy {
  readonly appInView = output<void>();
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);
  private observer?: IntersectionObserver;

  ngOnInit(): void {
    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) this.appInView.emit();
      },
      { rootMargin: '400px' },
    );
    this.observer.observe(this.el.nativeElement);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}
