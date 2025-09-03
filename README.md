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

## 📦 Project Structure

```plaintext
OCSP.Backend/
├── 📂 src/
│   ├── 📂 OCSP.API/              # Web API Layer (.NET)
│   │   ├── 📂 Controllers/       # API endpoints (Auth, Project, Contractor, ...)
│   │   ├── 📂 Middlewares/       # Custom middlewares (ErrorHandling, JWT, Logging)
│   │   ├── 📂 Hubs/              # SignalR real-time hubs (Chat, Notification)
│   │   ├── 📂 Configurations/    # Swagger, CORS, ServiceCollection extensions
│   │   ├── 📄 appsettings.json
│   │   ├── 📄 appsettings.Development.json
│   │   ├── 📄 Program.cs
│   │   └── 📄 OCSP.API.csproj
│   │
│   ├── 📂 OCSP.Application/      # Application Layer (business logic, DTOs, mapping, helpers)
│   │   ├── 📂 Services/          # Service interfaces + implementations
│   │   ├── 📂 DTOs/              # Request/Response DTOs
│   │   ├── 📂 Validators/        # FluentValidation classes
│   │   ├── 📂 Mappings/          # AutoMapper profile
│   │   ├── 📂 Common/            # Helpers, Constants, Exceptions
│   │   └── 📄 OCSP.Application.csproj
│   │
│   ├── 📂 OCSP.Domain/           # Domain Layer (Entities, Enums, Base classes)
│   │   ├── 📂 Entities/          # Core domain entities (User, Project, Contract, ...)
│   │   ├── 📂 Enums/             # Enum types (UserRole, ProjectStatus, ...)
│   │   ├── 📂 Common/            # BaseEntity, AuditableEntity
│   │   └── 📄 OCSP.Domain.csproj
│   │
│   ├── 📂 OCSP.Infrastructure/   # Infrastructure Layer (DB, Identity, Repos, External services)
│   │   ├── 📂 Data/              # ApplicationDbContext, EF Configurations, Migrations, Seeding
│   │   ├── 📂 Repositories/      # Repos + Interfaces
│   │   ├── 📂 ExternalServices/  # Email/SMS/CloudStorage
│   │   ├── 📂 Identity/          # ApplicationUser, ApplicationRole
│   │   ├── 📂 Configurations/    # DB & JWT config
│   │   └── 📄 OCSP.Infrastructure.csproj
│   │
│   └── 📂 OCSP.AI/               # AI Services (C# integration with AI models)
│       ├── 📂 Services/          # AI service interfaces + implementations
│       ├── 📂 Models/            # AI request/response models
│       ├── 📂 Configurations/    # AI configs
│       └── 📄 OCSP.AI.csproj
│
├── 📂 OCSP.AIService/            # 🆕 FastAPI Service (Python AI microservice)
│   ├── 📂 app/
│   │   ├── 📄 main.py            # Entry point (FastAPI app)
│   │   ├── 📂 api/               # REST endpoints (chat, recommendations)
│   │   ├── 📂 core/              # Core configs (settings, DB connection)
│   │   ├── 📂 services/          # Business logic (RAG, embeddings, LLM wrapper)
│   │   ├── 📂 models/            # Pydantic schemas (request/response models)
│   │   └── 📂 utils/             # Helpers (text preprocessing, etc.)
│   ├── 📄 requirements.txt       # Python dependencies
│   ├── 📄 Dockerfile             # Docker build
│   └── 📄 README.md              # AI Service guide
│
├── 📂 docker/                    # Docker Compose setup
│   ├── 📄 docker-compose.yml
│   └── 📄 docker-compose.override.yml
│
├── 📂 tests/                     # .NET test projects
│   ├── 📂 OCSP.UnitTests/
│   ├── 📂 OCSP.IntegrationTests/
│   └── 📂 OCSP.API.Tests/
│
├── 📂 scripts/                   # Automation scripts
│   ├── 📄 setup-database.sh
│   ├── 📄 run-migrations.sh
│   └── 📄 seed-data.sh
│
├── 📂 docs/                      # Documentation
│   ├── 📄 api-documentation.md
│   ├── 📄 database-schema.md
│   └── 📄 deployment-guide.md
│
├── 📄 .gitignore
├── 📄 README.md
├── 📄 OCSP.Backend.sln
└── 📄 global.json
```
