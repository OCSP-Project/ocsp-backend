🏗️ OCSP Backend - Construction Management System

Clean Architecture với .NET 8, PostgreSQL, Docker và Authentication system hoàn chỉnh

🎯 Tổng quan
Hệ thống quản lý dự án xây dựng với kiến trúc Clean Architecture:

API Layer: Controllers, Middleware, Authentication
Application Layer: Services, DTOs, Business Logic
Domain Layer: Entities, Enums, Business Rules
Infrastructure Layer: Database, External Services
AI Layer: Smart recommendations và analysis

🚀 Quick Start
⚡ Docker (Khuyến nghị)
bash# Clone và navigate
git clone <repository-url>
cd OCSP.Backend

# Cấu hình email trong docker-compose.yml

# Thay your-email@gmail.com và your-app-password

# Start toàn bộ hệ thống

cd docker
docker-compose up -d

# Xem logs

docker-compose logs -f api
🌐 Truy cập:

API Swagger: http://localhost:8080/swagger
Health Check: http://localhost:8080/health
Database: localhost:5432 (ocsp/ocsp)

🔧 Manual Setup
bash# Cài đặt dependencies
dotnet restore

# Setup database

dotnet ef database update --project "src\OCSP.Infrastructure" --startup-project "src\OCSP.API"

# Chạy API

dotnet run --project src\OCSP.API\OCSP.API.csproj --urls http://localhost:8080

🛠️ Development Commands
🐳 Docker Commands
bash# Development
docker-compose up -d # Start services
docker-compose logs -f api # View logs
docker-compose restart api # Restart API only
docker-compose down # Stop all services

# Rebuild when code changes

docker-compose up -d --build

🗃️ Database Commands
bash# Create migration
dotnet ef migrations add <MigrationName> --project "src\OCSP.Infrastructure" --startup-project "src\OCSP.API"

# Update database

dotnet ef database update --project "src\OCSP.Infrastructure" --startup-project "src\OCSP.API"

# Remove last migration

dotnet ef migrations remove --project "src\OCSP.Infrastructure" --startup-project "src\OCSP.API"
🧪 Testing
bash# Run all tests
dotnet test

# Run specific test project

dotnet test tests/OCSP.UnitTests/

# With coverage

dotnet test --collect:"XPlat Code Coverage"

OCSP.Backend/
├── src/
│ ├── OCSP.API/ # Web API Layer
│ │ ├── Controllers/
│ │ │ ├── AuthController.cs
│ │ │ ├── ProjectController.cs
│ │ │ ├── ContractorController.cs
│ │ │ ├── SupervisorController.cs
│ │ │ ├── AdminController.cs
│ │ │ └── NotificationController.cs
│ │ ├── Middlewares/
│ │ │ ├── ErrorHandlingMiddleware.cs
│ │ │ ├── JwtMiddleware.cs
│ │ │ └── LoggingMiddleware.cs
│ │ ├── Hubs/ # SignalR Hubs
│ │ │ ├── ChatHub.cs
│ │ │ └── NotificationHub.cs
│ │ ├── Configurations/
│ │ │ ├── ServiceCollectionExtensions.cs
│ │ │ ├── SwaggerConfiguration.cs
│ │ │ └── CorsConfiguration.cs
│ │ ├── appsettings.json
│ │ ├── appsettings.Development.json
│ │ ├── Program.cs
│ │ └── OCSP.API.csproj
│ │
│ ├── OCSP.Application/ # Application Layer
│ │ ├── Services/
│ │ │ ├── Interfaces/
│ │ │ │ ├── IAuthService.cs
│ │ │ │ ├── IProjectService.cs
│ │ │ │ ├── IContractorService.cs
│ │ │ │ ├── ISupervisorService.cs
│ │ │ │ ├── INotificationService.cs
│ │ │ │ └── IFileService.cs
│ │ │ ├── AuthService.cs
│ │ │ ├── ProjectService.cs
│ │ │ ├── ContractorService.cs
│ │ │ ├── SupervisorService.cs
│ │ │ ├── NotificationService.cs
│ │ │ └── FileService.cs
│ │ ├── DTOs/
│ │ │ ├── Auth/
│ │ │ │ ├── LoginDto.cs
│ │ │ │ ├── RegisterDto.cs
│ │ │ │ └── TokenDto.cs
│ │ │ ├── Project/
│ │ │ │ ├── CreateProjectDto.cs
│ │ │ │ ├── UpdateProjectDto.cs
│ │ │ │ └── ProjectResponseDto.cs
│ │ │ ├── Contractor/
│ │ │ └── Supervisor/
│ │ ├── Validators/
│ │ │ ├── AuthValidators/
│ │ │ ├── ProjectValidators/
│ │ │ └── ContractorValidators/
│ │ ├── Mappings/
│ │ │ └── AutoMapperProfile.cs
│ │ ├── Common/
│ │ │ ├── Exceptions/
│ │ │ │ ├── BusinessException.cs
│ │ │ │ ├── ValidationException.cs
│ │ │ │ └── NotFoundException.cs
│ │ │ ├── Constants/
│ │ │ │ ├── AppConstants.cs
│ │ │ │ └── ErrorMessages.cs
│ │ │ └── Helpers/
│ │ │ ├── JwtHelper.cs
│ │ │ ├── PasswordHelper.cs
│ │ │ └── FileHelper.cs
│ │ └── OCSP.Application.csproj
│ │
│ ├── OCSP.Domain/ # Domain Layer
│ │ ├── Entities/
│ │ │ ├── User.cs
│ │ │ ├── Project.cs
│ │ │ ├── Contractor.cs
│ │ │ ├── Supervisor.cs
│ │ │ ├── Contract.cs
│ │ │ ├── ProgressReport.cs
│ │ │ ├── MaterialUsage.cs
│ │ │ ├── Payment.cs
│ │ │ ├── Notification.cs
│ │ │ ├── Review.cs
│ │ │ └── ChatMessage.cs
│ │ ├── Enums/
│ │ │ ├── UserRole.cs
│ │ │ ├── ProjectStatus.cs
│ │ │ ├── ContractStatus.cs
│ │ │ └── PaymentStatus.cs
│ │ ├── Common/
│ │ │ ├── BaseEntity.cs
│ │ │ └── AuditableEntity.cs
│ │ └── OCSP.Domain.csproj
│ │
│ ├── OCSP.Infrastructure/ # Infrastructure Layer
│ │ ├── Data/
│ │ │ ├── ApplicationDbContext.cs
│ │ │ ├── Configurations/
│ │ │ │ ├── UserConfiguration.cs
│ │ │ │ ├── ProjectConfiguration.cs
│ │ │ │ ├── ContractorConfiguration.cs
│ │ │ │ └── SupervisorConfiguration.cs
│ │ │ ├── Migrations/
│ │ │ └── Seeding/
│ │ │ ├── DatabaseSeeder.cs
│ │ │ └── SeedData/
│ │ ├── Repositories/
│ │ │ ├── Interfaces/
│ │ │ │ ├── IGenericRepository.cs
│ │ │ │ ├── IUserRepository.cs
│ │ │ │ ├── IProjectRepository.cs
│ │ │ │ ├── IContractorRepository.cs
│ │ │ │ └── ISupervisorRepository.cs
│ │ │ ├── GenericRepository.cs
│ │ │ ├── UserRepository.cs
│ │ │ ├── ProjectRepository.cs
│ │ │ ├── ContractorRepository.cs
│ │ │ └── SupervisorRepository.cs
│ │ ├── ExternalServices/
│ │ │ ├── Interfaces/
│ │ │ │ ├── IEmailService.cs
│ │ │ │ ├── ISmsService.cs
│ │ │ │ └── ICloudStorageService.cs
│ │ │ ├── EmailService.cs
│ │ │ ├── SmsService.cs
│ │ │ └── CloudStorageService.cs
│ │ ├── Identity/
│ │ │ ├── ApplicationUser.cs
│ │ │ └── ApplicationRole.cs
│ │ ├── Configurations/
│ │ │ ├── DatabaseConfiguration.cs
│ │ │ └── JwtConfiguration.cs
│ │ └── OCSP.Infrastructure.csproj
│ │
│ └── OCSP.AI/ # AI Services (tách riêng)
│ ├── Services/
│ │ ├── Interfaces/
│ │ │ ├── IAIRecommendationService.cs
│ │ │ ├── IAIReportSummaryService.cs
│ │ │ ├── IAIAssistantService.cs
│ │ │ └── IAIAnomalyDetectionService.cs
│ │ ├── GeminiService.cs
│ │ ├── RecommendationService.cs
│ │ ├── ReportSummaryService.cs
│ │ ├── AssistantService.cs
│ │ └── AnomalyDetectionService.cs
│ ├── Models/
│ │ ├── AIRequest.cs
│ │ ├── AIResponse.cs
│ │ └── RecommendationModel.cs
│ ├── Configurations/
│ │ └── AIConfiguration.cs
│ └── OCSP.AI.csproj
│
├── tests/
│ ├── OCSP.UnitTests/
│ ├── OCSP.IntegrationTests/
│ └── OCSP.API.Tests/
│
├── docker/
│ ├── Dockerfile
│ ├── docker-compose.yml
│ ├── docker-compose.override.yml
│ └── postgres/
│ └── init.sql
│
├── scripts/
│ ├── setup-database.sh
│ ├── run-migrations.sh
│ └── seed-data.sh
│
├── docs/
│ ├── api-documentation.md
│ ├── database-schema.md
│ └── deployment-guide.md
│
├── .gitignore
├── README.md
├── OCSP.Backend.sln
└── global.json
