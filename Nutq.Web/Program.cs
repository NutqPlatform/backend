using Microsoft.EntityFrameworkCore;
using Nutq.Infrastructure.Data;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Repositories;
using Nutq.Core.Services;
using Microsoft.OpenApi.Models;


// Allow Npgsql to accept DateTime with any Kind (not just UTC)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IPlanExerciseRepository, PlanExerciseRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IDifficultyLevelRepository, DifficultyLevelRepository>();
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IVocabularyExerciseRepository, VocabularyExerciseRepository>();
builder.Services.AddScoped<IWeeklyReportRepository, WeeklyReportRepository>();
builder.Services.AddScoped<IInvitationCodeRepository, InvitationCodeRepository>();
builder.Services.AddScoped<IExerciseProgressRepository, ExerciseProgressRepository>();
builder.Services.AddScoped<ITherapyPlanRepository, TherapyPlanRepository>();
builder.Services.AddScoped<IDoctorReviewRepository, DoctorReviewRepository>();
builder.Services.AddScoped<IDoctorPatientRelationshipRepository, DoctorPatientRelationshipRepository>();
builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();

builder.Services.AddScoped<IExerciseProgressRepository, ExerciseProgressRepository>();
builder.Services.AddScoped<IExerciseProgressService, ExerciseProgressService>();

builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();

builder.Services.AddScoped<ITherapyPlanService, TherapyPlanService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDoctorReviewService, DoctorReviewService>();
builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddScoped<IPatientDashboardService, PatientDashboardService>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddScoped<IDoctorAnalyticsService, DoctorAnalyticsService>();
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI with info
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nutq API",
        Version = "v1",
        Description = "API for Nutq app - Registration and Authentication"
    });

    // كل الـ actions هتظهر تحت tag واحد اسمه "API"
    c.TagActionsBy(api => new[] { "API" });
    c.DocInclusionPredicate((name, api) => true);
});

var app = builder.Build();
app.UseStaticFiles(); 
// Swagger configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nutq API v1");
        c.RoutePrefix = string.Empty; // Swagger UI على root
    });
}

// Disable HTTPS redirection for local development to avoid SSL issues
// app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
