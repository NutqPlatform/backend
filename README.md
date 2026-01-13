# Nutq Backend

This is the backend for the Nutq app.

## Overview
- Handles Doctor & Patient registration and login
- Manages Therapy Plans and Plan Exercises
- Built using .NET Core with Clean Architecture

## Features Implemented
- Doctor & Patient registration/login
- Creating therapy plans
- Adding exercises to therapy plans
- Fetching therapy plans for patients

## Tech Stack
- .NET 9.0.306
- Entity Framework Core
- PostgreSQL 

## How to Run
1. Clone the repo:  
   git clone https://github.com/NutqPlatform/backend.git
2. Open solution in Visual Studio / VS Code
3. Configure your database in appsettings.json
4. Run migrations:  
   dotnet ef database update
5. Run the project:  
   dotnet run

## Notes
- Frontend and Unity exercises/game integration are planned next
- AI constraints and exercise tracking will be implemented later