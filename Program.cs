using Shotter;
using Shotter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ShotterOptions>(builder.Configuration);

builder.Services.AddSingleton<IScreenshotQueue, ScreenshotQueue>();
builder.Services.AddHostedService<ScreenshotBackgroundService>();

builder.Services
    .AddTransient<IJellyfinService, JellyfinService>()
    .AddTransient<IFfmpegService, FfmpegService>()
    .AddTransient<IFileNameResolver, FileNameResolver>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();