-- -------------------------------------------------------------
-- Master Data Seed Script (PostgreSQL 16)
-- System Roles, Default Administrator, System Configs
-- -------------------------------------------------------------

-- Example idempotent statement:
-- INSERT INTO users (id, email, full_name, role, is_verified, created_at, updated_at)
-- VALUES ('a0000000-0000-0000-0000-000000000001', 'admin@ticketworld.io', 'Super Admin', 'Admin', TRUE, NOW(), NOW())
-- ON CONFLICT (email) DO NOTHING;
