using EDM.Infrastructure.Configuration;
using EDM.Infrastructure.Jobs;
using EDM.Persistence.Configuration;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — Next.js admin dashboard için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AdminDashboard", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AdminOrigin"] ?? "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services
    .UsePersistence(builder.Configuration)
    .UseInfrastructure();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(
        builder.Configuration.GetConnectionString("Hangfire") ??
        builder.Configuration.GetConnectionString("Default"))));
builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AdminDashboard");
app.UseHangfireDashboard("/hangfire");
app.UseAuthorization();
app.MapControllers();

// Sprint 3 — Gmail her 5 dakikada bir yokla
app.Lifetime.ApplicationStarted.Register(() =>
{
    RecurringJob.AddOrUpdate<GmailPollingJob>(
        "gmail-polling",
        job => job.ExecuteAsync(CancellationToken.None),
        "*/5 * * * *"); // her 5 dakika
});

app.Run();
