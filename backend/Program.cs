using backend.Configuration;
using backend.Data;
using backend.Repositories;
using backend.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend.Auth;
using backend.BackgroundServices;
using Serilog;
using Serilog.Events;
using backend.Models;
using backend.Service.Interfaces;
using backend.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/backend-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        fileSizeLimitBytes: 50_000_000,
        rollOnFileSizeLimit: true));

builder.Configuration.AddEnvironmentVariables(prefix: "BACKEND__");
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);
builder.Services
    .AddOptions<AppSettings>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    options.UseNpgsql(settings.ConnectionString);
});

builder.Services.AddScoped<ITeamCreatorRepository, TeamCreatorRepository>();
builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IHabitTeamRepository, HabitTeamRepository>();
builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
builder.Services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IHabitEntryRepository, HabitEntryRepository>();
builder.Services.AddScoped<IHabitService, HabitService>();

builder.Services.AddHostedService<InviteCodeExpiryService>();

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    app.Logger.LogInformation("Applying database migrations");
    db.Database.Migrate();
    app.Logger.LogInformation("Database migrations applied");

    var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
    app.Logger.LogInformation("Expiring past-due sessions");
    await sessionRepository.ExpirePastDueSessionsAsync();

    var inviteCodeRepository = scope.ServiceProvider.GetRequiredService<IInviteCodeRepository>();
    app.Logger.LogInformation("Expiring past-due invite codes");
    await inviteCodeRepository.ExpirePastDueInviteCodesAsync();

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.Logger.LogInformation("Seeding {Environment} data", app.Environment.EnvironmentName);
        await SeedData.SeedUsersAsync(db, app.Logger);
        await SeedData.SeedTeamsAsync(db, app.Logger);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var settings = app.Services.GetRequiredService<IOptions<AppSettings>>().Value;
app.UseCors(policy => policy
    .WithOrigins(settings.CorsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
    .AllowAnyHeader()
    .AllowAnyMethod());

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseMiddleware<SessionAuthenticationMiddleware>();
app.MapControllers();

app.Logger.LogInformation("Starting backend on {Environment}", app.Environment.EnvironmentName);
app.Run();

public partial class Program {}
