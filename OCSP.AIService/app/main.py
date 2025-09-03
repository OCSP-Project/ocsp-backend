from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
import uvicorn

from app.api import chat, recommendations
from app.core.config import settings
from app.core.database import init_vector_db

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    print("🚀 Starting OCSP AI Service...")
    await init_vector_db()
    print("✅ Vector database initialized")
    yield
    # Shutdown
    print("🔄 Shutting down OCSP AI Service...")

app = FastAPI(
    title="OCSP AI Service",
    description="RAG-powered construction advisory service for OCSP platform",
    version="1.0.0",
    lifespan=lifespan
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.ALLOWED_HOSTS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Include routers
app.include_router(chat.router, prefix="/api/v1/chat", tags=["chat"])
app.include_router(recommendations.router, prefix="/api/v1/recommendations", tags=["recommendations"])

@app.get("/")
async def root():
    return {"message": "OCSP AI Service is running! 🤖"}

@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "OCSP AI Service"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)