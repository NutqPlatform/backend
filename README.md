# Nutq API (Backend)

Nutq is a speech-therapy platform that connects doctors (speech therapists) and patients. The backend is a **.NET 9 Web API** that handles authentication, therapy-plan management, exercise delivery, speech-attempt analytics, clinical insight generation, and doctor/patient relationship & transfer workflows.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core Web API |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL (via `Npgsql.EntityFrameworkCore.PostgreSQL`) |
| Auth | Custom JWT (HMAC-SHA256), role-based (`doctor`, `patient`, `admin`) |
| API Docs | Swagger / Swashbuckle (`/` in Development) |

## Solution Structure

```
Nutq.sln
├── Nutq.Core            # Domain entities, interfaces, business logic (services), DTAs-free
│   ├── Entities/         # EF entities (Doctor, Patient, TherapyPlan, TrainingSession, ...)
│   ├── Interfaces/       # Repository & service contracts
│   ├── Services/         # Business logic (AuthService, TherapyPlanService, Analytics engines, ...)
│   ├── Commands/         # Input commands for services
│   ├── Models/           # Session data models, analytics projections, options
│   └── Auth/             # JWT generation, AuthResult
│
├── Nutq.Infrastructure   # EF Core DbContext + repository implementations (PostgreSQL)
│   ├── Data/             # ApplicationDbContext, design-time factory
│   └── Repositories/     # Concrete repository implementations
│
└── Nutq.Web              # ASP.NET Core Web API host
    ├── Controllers/       # REST endpoints
    ├── DTOs/               # Request/response contracts
    └── Program.cs          # DI, CORS, Swagger, middleware pipeline
```

This follows a **layered architecture**: `Nutq.Web` → `Nutq.Core` (interfaces + services) → `Nutq.Infrastructure` (EF Core implementations), keeping domain logic independent of persistence and transport concerns.

---

## Core Domains & Features

### 1. Identity & Access
- **Auth** (`AuthController`, `AuthService`): separate login endpoints for doctors and patients (`/api/auth/login/doctor`, `/api/auth/login/patient`), issuing JWTs with `id` and `role` claims.
- **Registration** (`RegistrationController`): invitation-code-gated registration for doctors (admin-issued codes) and patients (doctor-issued codes).
- **Admin** (`AdminController`, `AdminService`): admin login, doctor/patient blocking, invitation-code generation.
- `JwtAuthorizationHelper` centralizes token parsing/role checks across controllers (custom lightweight auth, not `[Authorize]` middleware-based).

### 2. Doctor–Patient Relationship & Transfers
- `DoctorPatientRelationship` tracks active/former assignments, including diagnosis snapshots at the time a relationship ends.
- `TransferService` / `TransferController` support: patient leaving a doctor, doctor releasing a patient, doctor-initiated transfer, patient-requested transfer, accept/reject workflows, and former-patient history.
- Transferring/releasing a patient **archives** their active therapy plans (read-only afterward).

### 3. Therapy Plans & Exercises
- `TherapyPlanService` / `TherapyPlanController`: create plans (with one or more exercises, supporting per-exercise repetition), add/remove exercises, update status/end date, list active/ongoing/all plans per doctor.
- Only one plan can be `Active` per patient at a time (others auto-paused).
- `PatientExerciseController`: patient-facing endpoints to list a plan's exercises, fetch vocabulary for an exercise, start/complete exercises, complete individual repetitions, and fetch session analytics.
- `ExerciseProgressController` / `ExerciseProgressService`: tracks `ExerciseProgress` (start/end time, score, repetition count, raw session JSON).

### 4. Speech Analytics Pipeline
1. Patient completes an exercise → raw `SessionData` JSON (from `SessionDataModels`) is stored on `ExerciseProgress`.
2. `PatientAnalyticsIngestionService` parses this JSON (`SessionDataParser`), creates a `TrainingSession`, individual `SpeechAttempt` rows, a `ProgressSnapshot`, `CategoryPerformanceSnapshot`s, updates recurring `PronunciationPattern`s, and generates a `SessionClinicalReport`.
3. `PatientAnalyticsService` exposes summaries, session lists, progress trends, category analysis, chart data, and clinical report detail per patient (`PatientAnalyticsController`).
4. `DoctorAnalyticsService` + `PlanAnalyticsEngine` compute plan-level analytics: word/category performance, strengths/weaknesses, progress comparisons (vs. previous session/plan/7d/30d), recurring-difficulty detection, and a weighted **Plan Outcome Score** (`DoctorAnalyticsController`).
5. **Clinical Insight Generators** (`IClinicalInsightGenerator`) are pluggable: `RuleBasedClinicalInsightGenerator` (deterministic) and `FutureAiClinicalInsightGenerator` (AI-oriented scaffold), selected via `PlanAnalyticsOptions.ActiveGenerator`.

### 5. Weekly Reports & Reviews
- `WeeklyReportController`: doctors create/update weekly progress reports tied to a patient/plan; both doctor and patient can view.
- `DoctorReviewController` / `DoctorReviewService`: patients can rate/review a doctor they have (or previously had) a relationship with; doctor's `AverageRating` is recalculated on create/update/delete.

### 6. Vocabulary & Exercise Catalog
- `Exercise`, `Vocabulary`, `DifficultyLevel`, `VocabularyExercise` form the content catalog, exposed via `ExerciseController` and `VocabularyController`.

---

## Data Model Highlights

See `drawsql_schema.txt` for the full ER schema. Key relationships:

- `Doctor 1─N Patient` (current assignment) + `DoctorPatientRelationship` (full history)
- `TherapyPlan N─1 Doctor`, `TherapyPlan N─1 Patient`, `TherapyPlan 1─N PlanExercise`
- `ExerciseProgress 1─1 TrainingSession 1─N SpeechAttempt`
- `TrainingSession 1─1 ProgressSnapshot 1─N CategoryPerformanceSnapshot`
- `TrainingSession 1─1 SessionClinicalReport`

---

## Getting Started

### Prerequisites
- .NET 9 SDK
- PostgreSQL instance

### Configuration
Set your connection string (e.g. in `appsettings.Development.json` or environment variables):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=NutqDB;Username=postgres;Password=1234"
  }
}
```

> ⚠️ **Security note:** the current codebase stores plaintext passwords, a hard-coded JWT secret in `AuthService`/`JwtTokenGenerator`, and disables HTTPS redirection for local dev. These should be hardened (password hashing, secret management, HTTPS) before any production deployment.

### Run locally

```bash
cd Nutq.Web
dotnet restore
dotnet ef database update   # requires Nutq.Infrastructure design-time factory
dotnet run
```

- API base: `http://localhost:5246` (see `Properties/launchSettings.json`)
- Swagger UI: `http://localhost:5246/` (Development only)

### CORS
Configured in `Program.cs` to allow the local Vite dev server ports (`5173`–`5175`) and the deployed frontend origin. Update `AllowFrontend` policy if you add new frontend origins.

---

## API Surface (selected)

| Area | Base Route |
|---|---|
| Auth | `/api/auth/login/{doctor|patient}` |
| Registration | `/api/registration/{doctor|patient}` |
| Admin | `/api/admin/...` |
| Doctor profile & patients | `/api/doctor/...` |
| Patient profile | `/api/patient/{id}/...` |
| Therapy plans | `/api/therapyplan/...` |
| Patient exercises | `/api/patient-exercises/...` |
| Exercise progress | `/api/exercise-progress/...` |
| Patient dashboard | `/api/patient-dashboard/{patientId}` |
| Patient analytics | `/api/patient-analytics/{patientId}/...` |
| Doctor analytics / plan analytics | `/api/doctor-analytics/...`, `/api/doctors/{doctorId}/plans/{planId}/analytics` |
| Weekly reports | `/api/weekly-reports/...` |
| Doctor reviews | `/api/doctorreview/...` |
| Transfers | `/api/transfer/...` |
| Vocabulary / Exercises | `/api/vocabulary`, `/api/exercise` |

Full request/response contracts are documented via Swagger when running in Development.

---

## Known Gaps / Suggested Next Steps
- Move password storage to hashed (e.g. BCrypt/Argon2) instead of plaintext comparison.
- Externalize JWT secret + connection strings to configuration/secret store.
- Replace manual `JwtAuthorizationHelper` checks with ASP.NET Core's built-in `[Authorize]`/policy-based auth.
- Add integration/unit tests around `PlanAnalyticsEngine` and the analytics ingestion pipeline (currently the most complex business logic).
- Re-enable HTTPS redirection for non-local environments.
