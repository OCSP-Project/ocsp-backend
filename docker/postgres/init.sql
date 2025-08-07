-- PostgreSQL initialization script
-- This runs when the container starts for the first time

-- Ensure database exists (should already exist from POSTGRES_DB)
SELECT 'CREATE DATABASE ocsp' 
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ocsp');

-- Connect to the ocsp database
\c ocsp;

-- Create UUID extension for GUID support
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create any additional extensions if needed
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Set timezone
SET timezone = 'UTC';

-- You can add initial data here if needed
-- INSERT INTO ... (will run after EF migrations)

COMMIT;