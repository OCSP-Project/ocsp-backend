using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OCSP.Infrastructure.Data;
using OCSP.Application.Services;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.ExternalServices;
using OCSP.Infrastructure.ExternalServices.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OCSP.Application.DTOs.Supervisor;
using OCSP.API.Hubs;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

//────────────────────────────────────────────────────────
// 1) Database Connection
//────────────────────────────────────────────────────────
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=ocsp;Username=ocsp;Password=ocsp";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

//────────────────────────────────────────────────────────
// 2) Services Registration
//────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper
builder.Services.AddAutoMapper(typeof(OCSP.Application.Mappings.AutoMapperProfile));

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddSignalR();


// Infrastructure Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();

// File Service
builder.Services.AddScoped<IFileService, FileService>();

//────────────────────────────────────────────────────────
// 3) JWT Authentication
//────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "your-very-secure-secret-key-that-is-at-least-32-characters-long";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

//────────────────────────────────────────────────────────
// 4) CORS
//────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

app.MapHub<ChatHub>("/chathub");
//────────────────────────────────────────────────────────
// 5) Auto Migration
//────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

//────────────────────────────────────────────────────────
// 6) Middleware Pipeline
//────────────────────────────────────────────────────────
 if (app.Environment.IsDevelopment())
 {
    app.UseSwagger();
    app.UseSwaggerUI();
 }

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Serve static files from the local 'uploads' folder (for profile documents, images, ...)
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});
app.MapControllers();

app.Run();