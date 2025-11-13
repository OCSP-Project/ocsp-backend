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
using OCSP.Application.Options;
using OCSP.Application.Services;
using OCSP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using OCSP.API.Extensions;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

//────────────────────────────────────────────────────────
// 1) Database Connection
//────────────────────────────────────────────────────────
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=db;Port=5432;Database=postgres;Username=postgres;Password=123";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


// var connectionString =
//     builder.Configuration.GetConnectionString("DefaultConnection")
//     ?? "Host=db;Port=5432;Database=postgres;Username=postgres;Password=root";

//────────────────────────────────────────────────────────
// 2) Services Registration
//────────────────────────────────────────────────────────
builder.Services.AddControllers();

// Configure request timeout and size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100_000_000; // 100MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100_000_000; // 100MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OCSP API", Version = "v1" });
    c.CustomSchemaIds(t => t.FullName!.Replace("+", "."));

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
builder.Services.AddAutoMapper(typeof(OCSP.Application.Mappings.ContractorMappingProfile).Assembly);

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ISupervisorContractService, SupervisorContractService>();
builder.Services.AddScoped<IContractorService, ContractorService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();
builder.Services.AddScoped<IContractMilestoneService, ContractMilestoneService>();
builder.Services.AddScoped<IEscrowService, EscrowService>();
builder.Services.AddScoped<OCSP.Infrastructure.ExternalServices.Interfaces.IPdfService, OCSP.Infrastructure.ExternalServices.PdfService>();
builder.Services.Configure<VnPayOptions>(builder.Configuration.GetSection("VnPay"));
builder.Services.Configure<PaymentOptions>(builder.Configuration.GetSection("Payments"));
builder.Services.AddScoped<IProgressMediaService, ProgressMediaService>();
builder.Services.AddScoped<IProjectTimelineService, ProjectTimelineService>();
builder.Services.AddScoped<IProjectDailyResourceService, ProjectDailyResourceService>();
// MoMo options + PaymentService
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var opt = new MomoOptions();
    cfg.GetSection("Momo").Bind(opt);
    return opt;
});
builder.Services.AddHttpClient<IPaymentService, PaymentService>();

// Project Document Services
builder.Services.AddScoped<IProjectDocumentService, ProjectDocumentService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
// Infrastructure Services
builder.Services.AddScoped<OCSP.Infrastructure.ExternalServices.Interfaces.IEmailService, OCSP.Infrastructure.ExternalServices.EmailService>();

// Project Invitation Service
builder.Services.AddScoped<OCSP.Application.Interfaces.IProjectInvitationService, ProjectInvitationService>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<IContractorRepository, ContractorRepository>();
builder.Services.AddScoped<ICommunicationRepository, CommunicationRepository>();
builder.Services.AddScoped<IContractMilestoneRepository, ContractMilestoneRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IProgressMediaRepository, ProgressMediaRepository>();
builder.Services.AddScoped<IProjectTimelineRepository, ProjectTimelineRepository>();
builder.Services.AddScoped<IProjectDailyResourceRepository, ProjectDailyResourceRepository>();

// Model Analysis services (3D Model Upload/Query + Building Elements + Tracking)
builder.Services.AddModelAnalysisServices();

// Configure upload size for GLB files (50MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800; // ~50MB
});

// File Service
builder.Services.AddScoped<IFileService, FileService>();

// Template Service
builder.Services.AddScoped<ITemplateService, TemplateService>();

// Budget System Services
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IPaymentRequestService, PaymentRequestService>();

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
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true));

    options.AddPolicy("AllowFrontend", policy => policy
        .WithOrigins(
            "http://localhost:3000",
            "http://localhost:3001",
            "https://your-frontend-domain.com")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetIsOriginAllowed(_ => true)); // Dev only
});

var app = builder.Build();

app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");
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

// ✅ THÊM: Configure static files
// Configure content type provider for 3D assets
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".glb"] = "model/gltf-binary";
contentTypeProvider.Mappings[".gltf"] = "model/gltf+json";

// Serve files từ wwwroot (with proper MIME types)
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

// ✅ THÊM: Serve files từ uploads folder ngoài wwwroot
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        // CORS headers for static assets (so <model-viewer> / three.js can fetch across origins)
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

        // Cache control
        const int durationInSeconds = 60 * 60 * 24 * 7; // 7 days
        ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
            "public,max-age=" + durationInSeconds;
    }
});

// Enable CORS (you can switch to "AllowFrontend" if you prefer explicit origins)
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();