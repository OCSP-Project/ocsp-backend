-- chạy trong DB $POSTGRES_DB (đặt = ocsp trong .env)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- tuỳ chọn: timezone mặc định của phiên, không bắt buộc
SET TIME ZONE 'UTC';
