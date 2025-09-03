import chromadb
from sentence_transformers import SentenceTransformer
from typing import List, Dict, Any
import asyncio
from concurrent.futures import ThreadPoolExecutor

from app.core.config import settings
from app.models.chat import RelevantDocument, UserContext

class RAGService:
    def __init__(self):
        self.client = chromadb.PersistentClient(path=settings.VECTOR_DB_PATH)
        self.collection = self.client.get_or_create_collection("construction_knowledge")
        self.embedding_model = SentenceTransformer(settings.EMBEDDING_MODEL)
        self.executor = ThreadPoolExecutor(max_workers=4)
    
    async def get_relevant_documents(
        self, 
        query: str, 
        user_context: UserContext,
        top_k: int = 5
    ) -> List[RelevantDocument]:
        
        # Enhance query with user context
        enhanced_query = self._enhance_query_with_context(query, user_context)
        
        # Get embeddings
        query_embedding = await asyncio.get_event_loop().run_in_executor(
            self.executor, self._get_embedding, enhanced_query
        )
        
        # Search vector database
        results = self.collection.query(
            query_embeddings=[query_embedding],
            n_results=top_k,
            where=self._build_filter(user_context)
        )
        
        # Convert to RelevantDocument objects
        relevant_docs = []
        for i, (doc, metadata, distance) in enumerate(zip(
            results['documents'][0],
            results['metadatas'][0],
            results['distances'][0]
        )):
            relevant_docs.append(RelevantDocument(
                content=doc,
                source=metadata.get('source', 'unknown'),
                score=1 - distance,  # Convert distance to similarity score
                metadata=metadata
            ))
        
        return relevant_docs
    
    def _enhance_query_with_context(self, query: str, context: UserContext) -> str:
        """Enhance query with user context for better retrieval"""
        enhancements = [query]
        
        if context.location:
            enhancements.append(f"khu vực {context.location}")
        
        if context.user_role:
            role_map = {
                "contractor": "nhà thầu",
                "homeowner": "chủ nhà",
                "supervisor": "giám sát"
            }
            enhancements.append(f"cho {role_map.get(context.user_role, context.user_role)}")
        
        return " ".join(enhancements)
    
    def _build_filter(self, context: UserContext) -> Dict[str, Any]:
        """Build metadata filter based on user context"""
        filters = {}
        
        if context.location:
            filters["location"] = {"$eq": context.location}
        
        if context.user_role:
            filters["target_audience"] = {"$in": [context.user_role, "all"]}
        
        return filters
    
    def _get_embedding(self, text: str) -> List[float]:
        """Get embedding for text"""
        return self.embedding_model.encode(text).tolist()
    
    async def add_document(self, content: str, metadata: Dict[str, Any]) -> str:
        """Add new document to vector database"""
        doc_id = f"doc_{len(self.collection.get()['ids'])}"
        
        embedding = await asyncio.get_event_loop().run_in_executor(
            self.executor, self._get_embedding, content
        )
        
        self.collection.add(
            documents=[content],
            embeddings=[embedding],
            metadatas=[metadata],
            ids=[doc_id]
        )
        
        return doc_id