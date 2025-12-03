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

cloudflared tunnel --url http://localhost:80
sudo nano /etc/nginx/sites-available/ocsp-backend

# Cấu hình email trong docker-compose.yml

# Thay your-email@gmail.com và your-app-password

TempPassword123!

# Start toàn bộ hệ thống

cd docker
docker-compose up -d

# Xem logs

docker-compose logs -f api --50
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

# Tạo một migration sau đó ra thư mục chính chạy các lệnh sau

# dotnet ef migrations add TenMigration --project "src/OCSP.Infrastructure" --startup-project "src/OCSP.API"

# dotnet ef database update --project "src/OCSP.Infrastructure" --startup-project "src/OCSP.API" --connection "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=root"

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
│ │ │ ├── AdminController.cs
│ │ │ ├── AuthController.cs
│ │ │ ├── ChatController.cs
│ │ │ ├── ContractorController.cs
│ │ │ ├── ContractsController.cs
│ │ │ ├── NotificationController.cs
│ │ │ ├── ProfileController.cs
│ │ │ ├── ProjectController.cs
│ │ │ ├── ProposalsController.cs
│ │ │ ├── QuotesController.cs
│ │ │ └── SupervisorController.cs
│ │ ├── Hubs/ # SignalR Hubs
│ │ │ ├── ChatHub.cs
│ │ │ └── NotificationHub.cs
│ │ ├── Middlewares/
│ │ │ ├── ErrorHandlingMiddleware.cs
│ │ │ ├── JwtMiddleware.cs
│ │ │ └── LoggingMiddleware.cs
│ │ ├── Configurations/
│ │ │ ├── CorsConfiguration.cs
│ │ │ ├── ServiceCollectionExtensions.cs
│ │ │ └── SwaggerConfiguration.cs
│ │ ├── appsettings.json
│ │ ├── appsettings.Development.json
│ │ ├── Program.cs
│ │ └── OCSP.API.csproj
│ │
│ ├── OCSP.Application/ # Application Layer
│ │ ├── Common/
│ │ │ ├── Constants/
│ │ │ │ ├── AppConstants.cs
│ │ │ │ └── ErrorMessages.cs
│ │ │ ├── Exceptions/ (3 files)
│ │ │ └── Helpers/ (3 files)
│ │ ├── DTOs/
│ │ │ ├── Auth/ (7 files)
│ │ │ ├── Common/ (1 file)
│ │ │ ├── Contractor/ (11 files)
│ │ │ ├── Contracts/ (7 files)
│ │ │ ├── Profile/ (1 file)
│ │ │ ├── Project/ (4 files)
│ │ │ ├── Proposals/ (5 files)
│ │ │ ├── Quotes/ (4 files)
│ │ │ └── Supervisor/ (3 files)
│ │ ├── Mappings/
│ │ │ ├── AutoMapperProfile.cs
│ │ │ └── ContractorMappingProfile.cs
│ │ ├── Services/
│ │ │ ├── Interfaces/ (12 files)
│ │ │ ├── AIRecommendationService.cs
│ │ │ ├── AuthService.cs
│ │ │ ├── ChatService.cs
│ │ │ ├── ContractorService.cs
│ │ │ ├── ContractService.cs
│ │ │ ├── FileService.cs
│ │ │ ├── NotificationService.cs
│ │ │ ├── ProfileService.cs
│ │ │ ├── ProjectService.cs
│ │ │ ├── ProposalService.cs
│ │ │ ├── QuoteService.cs
│ │ │ └── SupervisorService.cs
│ │ └── Validators/
│ │ ├── AuthValidators/...
│ │ ├── ContractorValidators/...
│ │ └── ProjectValidators/...
│ │
│ ├── OCSP.Domain/ # Domain Layer
│ │ ├── Common/
│ │ │ ├── AuditableEntity.cs
│ │ │ └── BaseEntity.cs
│ │ ├── Entities/ (21 files)
│ │ └── Enums/ (10 files)
│ │
│ ├── OCSP.Infrastructure/ # Infrastructure Layer
│ │ ├── Configurations/
│ │ │ ├── DatabaseConfiguration.cs
│ │ │ └── JwtConfiguration.cs
│ │ ├── Data/ (9 files)
│ │ ├── ExternalServices/ (6 files)
│ │ ├── Identity/
│ │ │ ├── ApplicationRole.cs
│ │ │ └── ApplicationUser.cs
│ │ ├── Migrations/ (3 files)
│ │ └── Repositories/ (10 files)
│ │
├── tests/
│ ├── OCSP.API.Tests/
│ ├── OCSP.IntegrationTests/
│ └── OCSP.UnitTests/
│
├── docker/
│ ├── Dockerfile
│ ├── docker-compose.yml
│ ├── docker-compose.override.yml
│ └── postgres/
│ └── init.sql
│
├── scripts/
│ ├── run-migrations.sh
│ ├── seed-data.sh
│ └── setup-database.sh
│
├── docs/
│ ├── api-documentation.md
│ ├── database-schema.md
│ └── deployment-guide.md
│
├── OCSP.Backend.sln
├── global.json
└── README.md
