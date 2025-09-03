from pydantic import BaseModel
from typing import List, Optional, Dict, Any
from datetime import datetime

class ChatMessage(BaseModel):
    id: str
    text: str
    is_user: bool
    timestamp: datetime

class UserContext(BaseModel):
    user_id: str
    user_role: str  # contractor, homeowner, supervisor
    current_project_id: Optional[str] = None
    location: Optional[str] = "Da Nang"  # Default to Da Nang
    preferences: Dict[str, Any] = {}

class ChatRequest(BaseModel):
    message: str
    user_id: str
    user_context: UserContext
    chat_history: List[ChatMessage] = []
    max_tokens: int = 512
    temperature: float = 0.7

class RelevantDocument(BaseModel):
    content: str
    source: str
    score: float
    metadata: Dict[str, Any] = {}

class ChatResponse(BaseModel):
    message: str
    user_id: str
    confidence_score: float
    relevant_docs: List[RelevantDocument] = []
    timestamp: datetime = datetime.now()
    processing_time_ms: int = 0