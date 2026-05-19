import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { App } from './app';
import { WorkoutApiService } from './core/workout-api.service';
import { Workout } from './core/models/workout.model';

describe('App', () => {
  let fixture: ComponentFixture<App>;
  let component: App;

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

  const apiMock = {
    getWorkouts: vi.fn(),
    createWorkout: vi.fn(),
    deleteWorkout: vi.fn(),
    sendRandomWorkout: vi.fn(),
  };

  beforeEach(async () => {
    apiMock.getWorkouts.mockReturnValue(of([]));
    apiMock.createWorkout.mockReturnValue(of(workout));
    apiMock.deleteWorkout.mockReturnValue(of(undefined));
    apiMock.sendRandomWorkout.mockReturnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        {
          provide: WorkoutApiService,
          useValue: apiMock,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it('should load workouts on init', () => {
    fixture.detectChanges();

    expect(apiMock.getWorkouts).toHaveBeenCalled();
  });

  it('should render title', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Deine Workout-Sammlung'
    );
  });

  it('should show empty state when no workouts exist', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Noch keine Workouts gespeichert.');
  });

  it('should set error when loading workouts fails', () => {
    apiMock.getWorkouts.mockReturnValueOnce(throwError(() => new Error('fail')));

    fixture.detectChanges();

    expect(component.error()).toBe('Workouts konnten nicht geladen werden.');
  });

  it('should not create workout when form is invalid', () => {
    fixture.detectChanges();

    component.youtubeUrl.setValue('');
    component.addWorkout();

    expect(apiMock.createWorkout).not.toHaveBeenCalled();
  });

  it('should create workout when form is valid', () => {
    fixture.detectChanges();

    component.youtubeUrl.setValue('https://www.youtube.com/watch?v=abc123');
    component.addWorkout();

    expect(apiMock.createWorkout).toHaveBeenCalledWith({
      youtubeUrl: 'https://www.youtube.com/watch?v=abc123',
    });

    expect(component.workouts()).toHaveLength(1);
    expect(component.selected()).toEqual(workout);
  });

  it('should delete workout', () => {
    fixture.detectChanges();

    component.workouts.set([workout]);
    component.selected.set(workout);

    component.deleteWorkout(workout);

    expect(apiMock.deleteWorkout).toHaveBeenCalledWith('1');
    expect(component.workouts()).toHaveLength(0);
    expect(component.selected()).toBeNull();
  });

  it('should select workout', () => {
    fixture.detectChanges();

    component.selectWorkout(workout);

    expect(component.selected()).toEqual(workout);
  });

  it('should send random workout mail', () => {
    fixture.detectChanges();

    component.sendRandomWorkout();

    expect(apiMock.sendRandomWorkout).toHaveBeenCalled();
    expect(component.error()).toBe('Test-Mail wurde ausgeloest.');
  });
});