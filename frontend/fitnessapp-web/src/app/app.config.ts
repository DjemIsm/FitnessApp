import { APP_INITIALIZER, ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';
import { AppConfigService } from './core/app-config.service';

export const appConfig: ApplicationConfig = {
 providers: [
 provideRouter(routes),
 provideHttpClient(),
 {
 provide: APP_INITIALIZER,
 multi: true,
 deps: [AppConfigService],
 useFactory: (config: AppConfigService) => () => config.load()
 }
 ]
};