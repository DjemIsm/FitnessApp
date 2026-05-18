# FitnessApp

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![Render](https://img.shields.io/badge/Render-deployment-46E3B7?logo=render&logoColor=black)
![Supabase](https://img.shields.io/badge/Supabase-database-3ECF8E?logo=supabase&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

FitnessApp is a fullstack workout application built with Angular, ASP.NET Core, PostgreSQL and Docker.

The app allows users to save YouTube workout videos, automatically load video metadata, watch saved workouts in the browser and send a random workout by email.

## Screenshot

![FitnessApp UI Overview](docs/screenshots/fitnessapp-ui-overview.png)

The screenshot shows the main user interface with the add-workout form, saved workout cards, selected workout player, test email action and delete/watch buttons.

## Features

- Save YouTube workout videos by URL
- Automatically fetch title, channel, thumbnail and video ID from the YouTube Data API
- Display all saved workouts in an Angular frontend
- Watch workouts directly inside the app using YouTube embeds
- Delete saved workouts
- Send a random workout by email
- Daily scheduled workout email with Hangfire
- PostgreSQL database integration
- Docker-based backend deployment
- Runtime frontend configuration through `config.json`

## Tech Stack

### Frontend

- Angular 21
- TypeScript
- Reactive Forms
- HttpClient
- Standalone Components
- CSS

### Backend

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- Npgsql PostgreSQL Provider
- Hangfire
- MailKit
- YouTube Data API v3

### Database

- PostgreSQL
- Local development with Docker Compose

## Project Structure

```txt
FitnessApp/
├── backend/
│   └── FitnessApp.Api/
├── frontend/
│   └── fitnessapp-web/
├── docs/
│   ├── api-tests.http
│   └── screenshots/
├── docker-compose.yml
├── .gitignore
└── README.md

```

## How the App Works

1. A user enters a YouTube workout URL in the Angular frontend.
2. The frontend sends the URL to the ASP.NET Core backend.
3. The backend extracts the YouTube video ID.
4. The backend requests metadata from the YouTube Data API.
5. The workout is saved in PostgreSQL.
6. The frontend displays the saved workout with title, channel and thumbnail.
7. The user can watch the video directly inside the app.
8. Hangfire can send a random workout by email.

## Local Setup

### Requirements

- Git
- Docker
- .NET 10 SDK
- Node.js 24 LTS
- Angular CLI 21
- PostgreSQL via Docker
- Gmail app password
- YouTube Data API key

## Start PostgreSQL

```bash
docker compose up -d
```

## Start Backend

```bash
cd backend/FitnessApp.Api
dotnet restore
dotnet ef database update
dotnet run
```

Backend URL:

```txt
http://localhost:5000
```

Health check:

```bash
curl http://localhost:5000/health
```

## Start Frontend

```bash
cd frontend/fitnessapp-web
npm install
npm start
```

Frontend URL:

```txt
http://localhost:4200
```

## Local Configuration

Create this file locally:

```txt
backend/FitnessApp.Api/appsettings.Development.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fitnessapp;Username=fitnessapp;Password=fitnessapp_dev_password"
  },
  "YouTube": {
    "ApiKey": "YOUR_YOUTUBE_API_KEY"
  },
  "Mail": {
    "UserName": "your.gmail.account@gmail.com",
    "Password": "YOUR_GMAIL_APP_PASSWORD",
    "FromEmail": "your.gmail.account@gmail.com",
    "ToEmail": "recipient@example.com"
  }
}
```


## Frontend Runtime Config

Local frontend config:

```txt
frontend/fitnessapp-web/public/config.json
```

Example:

```json
{
  "apiBaseUrl": "http://localhost:5000"
}
```

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/health` | Health check |
| GET | `/api/workouts` | Get all workouts |
| POST | `/api/workouts` | Add a YouTube workout |
| DELETE | `/api/workouts/{id}` | Delete a workout |
| POST | `/api/workouts/send-random` | Send a random workout email |

## Example Request

```http
POST http://localhost:5000/api/workouts
Content-Type: application/json

{
  "youtubeUrl": "https://www.youtube.com/watch?v=VIDEO_ID"
}
```


## Author

Created by [DjemIsm](https://github.com/DjemIsm)