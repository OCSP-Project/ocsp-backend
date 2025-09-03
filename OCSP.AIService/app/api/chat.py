from fastapi import APIRouter, HTTPException, Depends
from typing import List
import asyncio

from app.models.chat import ChatRequest, ChatResponse, ChatMessage
from app.services.rag_service import RAGService
from app.services.llm_service import LLMService

router = APIRouter()

# Dependency injection
async def get_rag_service() -> RAGService:
    return RAGService()

async def get_llm_service() -> LLMService:
    return LLMService()

@router.post("/message", response_model=ChatResponse)
async def send_message(
    request: ChatRequest,
    rag_service: RAGService = Depends(get_rag_service),
    llm_service: LLMService = Depends(get_llm_service)
):
    try:
        # 1. Get relevant context using RAG
        relevant_docs = await rag_service.get_relevant_documents(
            query=request.message,
            user_context=request.user_context
        )
        
        # 2. Generate response using LLM
        response = await llm_service.generate_response(
            query=request.message,
            context=relevant_docs,
            chat_history=request.chat_history
        )
        
        return ChatResponse(
            message=response,
            user_id=request.user_id,
            confidence_score=0.95,  # Calculate actual confidence
            relevant_docs=relevant_docs[:3]  # Return top 3 relevant docs
        )
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"AI service error: {str(e)}")

@router.get("/history/{user_id}")
async def get_chat_history(user_id: str):
    # Get chat history from your database
    # This will integrate with ASP.NET API
    pass

@router.post("/feedback")
async def submit_feedback(feedback_data: dict):
    # Handle user feedback for model improvement
    pass