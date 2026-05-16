// src/app/core/app-config.service.ts
import { Injectable } from '@angular/core';

export interface AppConfig {
 apiBaseUrl: string;
}

@Injectable({ providedIn: 'root' })
export class AppConfigService {
 private config: AppConfig = { apiBaseUrl: 'http://localhost:5000' };

 async load(): Promise<void> {
 const response = await fetch('/config.json', { cache: 'no-store' });
 this.config = await response.json();
 }

 get apiBaseUrl(): string {
 return this.config.apiBaseUrl.replace(/\/$/, '');
 }
}