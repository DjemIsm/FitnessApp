export interface Workout {
  id: string;
  youtubeVideoId: string;
  youtubeUrl: string;
  title: string;
  channelTitle?: string;
  thumbnailUrl?: string;
  durationIso8601?: string;
  isActive: boolean;
  createdAtUtc: string;
  lastSentAtUtc?: string;
}

export interface CreateWorkoutRequest {
  youtubeUrl: string;
}