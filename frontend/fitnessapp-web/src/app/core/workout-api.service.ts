import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { CreateWorkoutRequest, Workout } from './models/workout.model';

@Injectable({ providedIn: 'root' })
export class WorkoutApiService {
 private readonly http = inject(HttpClient);
 private readonly config = inject(AppConfigService);

 getWorkouts(): Observable<Workout[]> {
 return this.http.get<Workout[]>(`${this.config.apiBaseUrl}/api/workouts`);
 }

 createWorkout(request: CreateWorkoutRequest): Observable<Workout> {
 return this.http.post<Workout>(`${this.config.apiBaseUrl}/api/workouts`, request);
 }

 deleteWorkout(id: string): Observable<void> {
 return this.http.delete<void>(`${this.config.apiBaseUrl}/api/workouts/${id}`);
 }

 sendRandomWorkout(): Observable<unknown> {
 return this.http.post(`${this.config.apiBaseUrl}/api/workouts/send-random`, {});
 }
}