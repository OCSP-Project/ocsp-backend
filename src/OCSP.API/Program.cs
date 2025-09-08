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
using OCSP.Infrastructure.Repositories.Interfaces;
using OCSP.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

//────────────────────────────────────────────────────────
// 1) Database Connection
//────────────────────────────────────────────────────────
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=db;Port=5432;Database=postgres;Username=postgres;Password=root";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


// var connectionString =
//     builder.Configuration.GetConnectionString("DefaultConnection")
//     ?? "Host=db;Port=5432;Database=postgres;Username=postgres;Password=root";

//────────────────────────────────────────────────────────
// 2) Services Registration
//────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OCSP API", Version = "v1" });

    // 🔐 Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Dán token vào đây. Nếu UI không tự thêm prefix, dùng: Bearer {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// AutoMapper
builder.Services.AddAutoMapper(typeof(OCSP.Application.Mappings.AutoMapperProfile));

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractorService, ContractorService>();

// Infrastructure Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<IContractorRepository, ContractorRepository>();
builder.Services.AddScoped<ICommunicationRepository, CommunicationRepository>();

// File Service
builder.Services.AddScoped<IFileService, FileService>();

// SignalR (required for MapHub)
builder.Services.AddSignalR();

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


builder.Services.AddHttpClient<AIRecommendationService>(); // HttpClient cho service
builder.Services.AddScoped<OCSP.Application.Services.Interfaces.IAIRecommendationService,
                           OCSP.Application.Services.AIRecommendationService>();

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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