import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { WorkoutApiService } from './workout-api.service';
import { AppConfigService } from './app-config.service';
import { Workout } from './models/workout.model';

describe('WorkoutApiService', () => {
  let service: WorkoutApiService;
  let httpMock: HttpTestingController;

  const configMock = {
    apiBaseUrl: 'http://localhost:5000',
  };

  const workout: Workout = {
    id: '1',
    youtubeVideoId: 'abc123',
    youtubeUrl: 'https://www.youtube.com/watch?v=abc123',
    title: 'Test Workout',
    channelTitle: 'Test Channel',
    thumbnailUrl: 'https://example.com/thumb.jpg',
    durationIso8601: 'PT10M',
    isActive: true,
    createdAtUtc: '2026-01-01T00:00:00Z',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: AppConfigService,
          useValue: configMock,
        },
      ],
    });

    service = TestBed.inject(WorkoutApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should load workouts', () => {
    service.getWorkouts().subscribe(result => {
      expect(result).toEqual([workout]);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/workouts');
    expect(req.request.method).toBe('GET');
    req.flush([workout]);
  });

  it('should create workout', () => {
    const request = {
      youtubeUrl: 'https://www.youtube.com/watch?v=abc123',
    };

    service.createWorkout(request).subscribe(result => {
      expect(result).toEqual(workout);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/workouts');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(workout);
  });

  it('should delete workout', () => {
    service.deleteWorkout('1').subscribe(result => {
      expect(result).toBeNull();
    });

    const req = httpMock.expectOne('http://localhost:5000/api/workouts/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should send random workout', () => {
    service.sendRandomWorkout().subscribe(result => {
      expect(result).toEqual({});
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/workouts/send-random'
    );
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush({});
  });
});