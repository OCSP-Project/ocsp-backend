from pydantic import BaseSettings
from typing import List

class Settings(BaseSettings):
    # API Settings
    API_HOST: str = "0.0.0.0"
    API_PORT: int = 8000
    
    # CORS
    ALLOWED_HOSTS: List[str] = ["http://localhost:3000", "http://localhost:5000"]
    
    # Vector Database
    VECTOR_DB_PATH: str = "./data/vector_db"
    EMBEDDING_MODEL: str = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
    
    # LLM Settings
    LLM_MODEL_PATH: str = "path/to/gpt-oss-20b"  # Your model path
    MAX_TOKENS: int = 512
    TEMPERATURE: float = 0.7
    
    # Redis (for caching)
    REDIS_URL: str = "redis://localhost:6379"
    
    # ASP.NET Integration
    ASPNET_API_URL: str = "http://localhost:5000"
    ASPNET_API_KEY: str = "your-api-key"
    
    class Config:
        env_file = ".env"

settings = Settings()