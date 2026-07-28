import { Injectable, signal, effect, Inject, PLATFORM_ID } from '@angular/core';
import { DOCUMENT, isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})

export class ThemeService {
  currentTheme = signal<string>('light');

  public readonly availableThemes = [
    "light",
    "dark",
    "cupcake",
    "bumblebee",
    "emerald",
    "corporate",
    "synthwave",
    "retro",
    "cyberpunk",
    "valentine",
    "halloween",
    "garden",
    "forest",
    "aqua",
    "lofi",
    "pastel",
    "fantasy",
    "wireframe",
    "black",
    "luxury",
    "dracula",
    "cmyk",
    "autumn",
    "business",
    "acid",
    "lemonade",
    "night",
    "coffee",
    "winter",
    "dim",
    "nord",
    "sunset"
  ];

  constructor(
    @Inject(DOCUMENT) private document: Document,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    if (isPlatformBrowser(this.platformId)) {
        const savedTheme = localStorage.getItem('theme');
        if (savedTheme) {
            this.setTheme(savedTheme);
        } else {
             // Check system preference
             if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                 this.setTheme('dark');
             }
        }
    }

    // Effect to update the DOM when the signal changes
    effect(() => {
        const theme = this.currentTheme();
        if (isPlatformBrowser(this.platformId)) {
             this.document.documentElement.setAttribute('data-theme', theme);
             localStorage.setItem('theme', theme);
        }
    });
  }

  setTheme(theme: string) {
    this.currentTheme.set(theme);
  }

  toggleTheme() {
    this.currentTheme.update(current => current === 'light' ? 'dark' : 'light');
  }
}
