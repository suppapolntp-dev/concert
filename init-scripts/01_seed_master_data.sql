-- ==============================================================================
-- TICKETWORLD MASTER DATA SEED SCRIPT (PostgreSQL 16)
-- Automatically executed once on container initialization if postgres_data is empty
-- ==============================================================================

-- 1. Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 2. Seed default platform admin if table exists and empty (Idempotent)
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'users') THEN
        INSERT INTO users (
            id, email, password_hash, full_name, role, is_verified, is_banned, two_factor_enabled, created_at, updated_at
        ) VALUES (
            'a0000000-0000-0000-0000-000000000001',
            'admin@ticketworld.io',
            -- Hash for 'Admin@123456'
            '$argon2id$v=19$m=65536,t=3,p=4$c29tZXNhbHR2YWx1ZQ$Gq4hLw8k4k2V7b+9v9e1tZ5w2X0u8y6A3c4d5e6f7g8',
            'Platform Administrator',
            'Admin',
            TRUE,
            FALSE,
            FALSE,
            NOW(),
            NOW()
        )
        ON CONFLICT (email) DO NOTHING;
    END IF;
END $$;
