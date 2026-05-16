import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { WorkoutApiService } from './core/workout-api.service';
import { Workout } from './core/models/workout.model';

@Component({
 selector: 'app-root',
 imports: [ReactiveFormsModule],
 templateUrl: './app.html',
 styleUrl: './app.css'
})
export class App implements OnInit {
 private readonly api = inject(WorkoutApiService);
 private readonly sanitizer = inject(DomSanitizer);

 readonly workouts = signal<Workout[]>([]);
 readonly loading = signal(false);
 readonly error = signal<string | null>(null);
 readonly selected = signal<Workout | null>(null);

 readonly youtubeUrl = new FormControl('', {
 nonNullable: true,
 validators: [Validators.required]
 });

 readonly hasWorkouts = computed(() => this.workouts().length > 0);

 ngOnInit(): void {
 this.loadWorkouts();
 }

 loadWorkouts(): void {
 this.loading.set(true);
 this.error.set(null);

 this.api.getWorkouts().subscribe({
 next: workouts => {
 this.workouts.set(workouts);
 if (!this.selected() && workouts.length > 0) this.selected.set(workouts[0]);
 this.loading.set(false);
 },
 error: () => {
 this.error.set('Workouts konnten nicht geladen werden.');
 this.loading.set(false);
 }
 });
 }

 addWorkout(): void {
 if (this.youtubeUrl.invalid) return;

 this.loading.set(true);
 this.error.set(null);

 this.api.createWorkout({ youtubeUrl: this.youtubeUrl.value }).subscribe({
 next: created => {
 this.youtubeUrl.reset('');
 this.workouts.update(items => [created, ...items]);
 this.selected.set(created);
 this.loading.set(false);
 },
 error: err => {
 this.error.set(err?.error?.error ?? 'Workout konnte nicht gespeichert werden.');
 this.loading.set(false);
 }
 });
 }

 deleteWorkout(workout: Workout): void {
 this.api.deleteWorkout(workout.id).subscribe({
 next: () => {
 this.workouts.update(items => items.filter(x => x.id !== workout.id));
 if (this.selected()?.id === workout.id) this.selected.set(this.workouts()[0] ?? null);
 },
 error: () => this.error.set('Workout konnte nicht geloescht werden.')
 });
 }

 selectWorkout(workout: Workout): void {
 this.selected.set(workout);
 }

 sendRandomWorkout(): void {
 this.api.sendRandomWorkout().subscribe({
 next: () => this.error.set('Test-Mail wurde ausgeloest.'),
 error: err => this.error.set(err?.error?.error ?? 'Test-Mail konnte nicht gesendet werden.')
 });
 }

 embedUrl(workout: Workout): SafeResourceUrl {
 return this.sanitizer.bypassSecurityTrustResourceUrl(
 `https://www.youtube.com/embed/${workout.youtubeVideoId}`);
 }
}