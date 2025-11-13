using Microsoft.EntityFrameworkCore;
using ZoomMeetingAPI.Data;
using ZoomMeetingAPI.Repositories;
using ZoomMeetingAPI.Repositories.Interfaces;
using ZoomMeetingAPI.Services;
using ZoomMeetingAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Authentication Service
// builder.Services.AddScoped<ZoomAuthService>();
// Zoom Auth Service (Singleton - shared across app)
builder.Services.AddSingleton<ZoomAuthService>();

// Repository Pattern
builder.Services.AddScoped<IZoomMeetingRepository, ZoomMeetingRepository>();
builder.Services.AddScoped<IZoomRepository, ZoomRepository>();
builder.Services.AddScoped<ICloudRecordingRepository, CloudRecordingRepository>();

// Services
builder.Services.AddScoped<IMeetingService, MeetingService>();
// builder.Services.AddScoped<IZoomService, ZoomService>();
builder.Services.AddScoped<ICloudRecordingService, CloudRecordingService>();

// HttpClient for Zoom API
// builder.Services.AddHttpClient<IZoomService, ZoomService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zoom Meeting API V1");
        c.RoutePrefix = "swagger";
    });
}

// Configure CORS - must be called before other middleware
app.UseCors();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
