#!/bin/bash

# Script to apply 3D Model Tracking migration to PostgreSQL

echo "🚀 Applying 3D Model Tracking migration..."

# Check if running in Docker
if [ -f /.dockerenv ]; then
    # Running inside Docker container
    psql -U $POSTGRES_USER -d $POSTGRES_DB -f /migrations/migration-3d-model-tracking.sql
else
    # Running on host machine
    psql -h localhost -p 5432 -U $POSTGRES_USER -d $POSTGRES_DB -f migration-3d-model-tracking.sql
fi

echo "✅ Migration completed!"

