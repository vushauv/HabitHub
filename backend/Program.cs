using backend.Configuration;
using backend.Data;
using backend.Repositories;
using backend.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

builder.Configuration.AddEnvironmentVariables(prefix: "BACKEND__");
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

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
    await sessionRepository.ExpirePastDueSessionsAsync();
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

app.UseHttpsRedirection();
app.UseMiddleware<SessionAuthenticationMiddleware>();
app.MapControllers();

app.Run();

public partial class Program {}
