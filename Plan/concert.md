# 📘 TICKETWORLD — ENTERPRISE DATA DICTIONARY & SYSTEM SPECIFICATION v2.0

> **Version:** 2.0.0  
> **Last Updated:** 2026-09-12  
> **Architecture:** Clean Architecture (C# .NET 8) + Angular 18 (TypeScript) + Tailwind CSS + PostgreSQL 16 + Redis + SignalR  
> **Scope:** Multi-Portal Global Ticketing Platform (Customer, Organizer, Platform Admin/Dev)

---

## 📑 TABLE OF CONTENTS

1. [Security, Roles & Access Control Matrix (RBAC/ABAC)](#1-security-roles--access-control-matrix)
2. [Database Data Dictionary (PostgreSQL Schema)](#2-database-data-dictionary)
3. [API Data Dictionary & Endpoint Contract](#3-api-data-dictionary--endpoint-contract)
4. [Frontend Data Models & Route Access Matrix (Angular 18)](#4-frontend-data-models--route-access-matrix)
5. [State Machines & Concurrency Rules](#5-state-machines--concurrency-rules)
6. [Cross-Cutting Architecture & Infrastructure](#6-cross-cutting-architecture--infrastructure)

---

## 1. SECURITY, ROLES & ACCESS CONTROL MATRIX

ระบบใช้ Hybrid **RBAC (Role-Based Access Control)** ผสม **ABAC (Attribute-Based Access Control / Resource Ownership)** ผ่าน ASP.NET Core Claims-based Authorization Policies และ Angular Route Guards

### 1.1 User Roles Definition

| Role ID | Role Name | Code | Level | Target Portal | Description |
| :--- | :--- | :--- | :---: | :--- | :--- |
| `R1` | **Customer** | `ROLE_CUSTOMER` | 1 | Customer Portal (`/`) | ผู้ใช้ทั่วไป ซื้อตั๋ว ติดตามศิลปิน จัดการตั๋วของตนเอง |
| `R2` | **Organizer** | `ROLE_ORGANIZER` | 2 | Organizer Dashboard (`/organizer`) | ผู้จัดงาน, แบรนด์, ตัวแทนศิลปิน จัดการอีเวนต์ ผังที่นั่ง ตรวจสอบยอดขาย สแกนตั๋ว |
| `R3` | **Platform Admin / Dev** | `ROLE_ADMIN` | 3 | Admin Panel (`/admin`) | เจ้าของระบบ ทีมพัฒนา ตรวจสอบ Log, อนุมัติงาน, ควบคุม Feature Flag, ดู System Health |

---

### 1.2 Fine-Grained Permissions & Claims

| Permission Claim | Description | Customer | Organizer | Admin |
| :--- | :--- | :---: | :---: | :---: |
| `events.view:published` | ดูอีเวนต์ที่เปิดขายต่อสาธารณะ | ✅ | ✅ | ✅ |
| `events.view:draft` | ดูอีเวนต์ฉบับร่างของตนเอง (Own Only) | ❌ | ✅ *(Own)* | ✅ *(All)* |
| `events.create` | สร้าง Event / ร่างผังที่นั่ง | ❌ | ✅ | ✅ |
| `events.update:own` | แก้ไขข้อมูล Event ตนเองก่อนเปิดขาย | ❌ | ✅ *(Own)* | ✅ |
| `events.publish` | ส่ง Event เข้าคิวขออนุมัติเปิดขาย | ❌ | ✅ *(Own)* | ✅ |
| `events.moderate` | Approve / Reject / Suspend อีเวนต์ | ❌ | ❌ | ✅ |
| `events.cancel` | ยกเลิกอีเวนต์ (Trigger Auto-Refund) | ❌ | ✅ *(Own, pre-event)* | ✅ |
| `seats.hold` | ล็อกที่นั่งชั่วคราว (Hold 10 นาที) | ✅ | ❌ | ❌ |
| `seats.release` | สละที่นั่งที่ Hold ไว้ | ✅ *(Own)* | ❌ | ✅ *(Force)* |
| `seats.layout:edit` | วาดและจัดผังที่นั่ง (Seat Map Builder) | ❌ | ✅ *(Own)* | ✅ |
| `bookings.create` | ทำรายการจองและจ่ายเงิน | ✅ | ❌ | ❌ |
| `bookings.view:own` | ดูประวัติและตั๋ว QR Code ของตนเอง | ✅ | ❌ | ✅ |
| `bookings.view:event` | ดูรายการจองทั้งหมดใน Event ตนเอง | ❌ | ✅ *(Own)* | ✅ *(All)* |
| `bookings.cancel:own` | ขอยกเลิกตั๋ว / ขอ Refund ตามเงื่อนไข | ✅ | ❌ | ✅ |
| `checkin.scan` | สแกน QR Code ตรวจตั๋วหน้างาน | ❌ | ✅ *(Event)* | ✅ |
| `promo.manage` | สร้าง/แก้ไข/ลบ Promo Code | ❌ | ✅ *(Own)* | ✅ |
| `reviews.write` | เขียนรีวิว Event ที่เข้าร่วมแล้ว | ✅ | ❌ | ❌ |
| `reviews.moderate` | ลบรีวิวที่ไม่เหมาะสม | ❌ | ❌ | ✅ |
| `artists.manage` | สร้าง/แก้ไขโปรไฟล์ศิลปิน | ❌ | ✅ | ✅ |
| `artists.follow` | กดติดตามศิลปิน | ✅ | ❌ | ❌ |
| `waitlist.join` | ลงทะเบียนรอตั๋วเมื่อ Sold Out | ✅ | ❌ | ❌ |
| `waitlist.view` | ดูรายชื่อ Waitlist ของ Event ตนเอง | ❌ | ✅ *(Own)* | ✅ |
| `analytics.organizer` | ดูยอดขาย, Conversion, Heatmap ของงานตนเอง | ❌ | ✅ *(Own)* | ✅ |
| `analytics.platform` | ดู GMV รวมทั่วโลก, Top Events, Platform Revenue | ❌ | ❌ | ✅ |
| `finance.payout:request` | ขอยื่นเบิกเงินยอดขายเข้าบัญชีบริษัท | ❌ | ✅ | ❌ |
| `finance.payout:process` | อนุมัติและโอนเงิน Payout ให้ Organizer | ❌ | ❌ | ✅ |
| `files.upload` | อัปโหลดรูปภาพ / สื่อ | ❌ | ✅ | ✅ |
| `notifications.read` | ดูและจัดการการแจ้งเตือนของตนเอง | ✅ | ✅ | ✅ |
| `audit.logs:view` | ตรวจสอบ Audit Trail (Who/When/What Changed) | ❌ | ❌ | ✅ |
| `system.health:view` | ตรวจสอบ APM, Connection Pool, Cache Hit, Queues | ❌ | ❌ | ✅ |
| `system.config:write` | ปรับแก้ Feature Flags และ System Configs | ❌ | ❌ | ✅ |
| `users.manage` | Ban / Unban / กำหนด Role ผู้ใช้ | ❌ | ❌ | ✅ |

---

### 1.3 Authorization Enforcement Architecture

**Backend (ASP.NET Core):**
- Policy-based: `[Authorize(Policy = "OrganizerOnly")]`, `[Authorize(Roles = "Organizer,Admin")]`
- Resource-based: `EventOwnerAuthorizationHandler` ตรวจสอบ `organizer_id == CurrentUser.OrganizerId`
- Rate Limiting Middleware ป้องกัน Brute-force & Bot scalping:

| Endpoint Group | Rate Limit | Window |
| :--- | :--- | :--- |
| Public Search (`GET /events`) | 120 req | 1 min |
| Seat Hold (`POST /seats/hold`) | 5 req | 1 min per user |
| Payment (`POST /bookings/*/checkout`) | 3 req | 1 min per user |
| Auth Login (`POST /auth/login`) | 10 req | 5 min per IP |
| File Upload (`POST /files/upload`) | 10 req | 1 min per user |

**Frontend (Angular):**
- `canActivate: [AuthGuard, RoleGuard]` ตรวจสอบ JWT Token + Claims ก่อนเข้าหน้า
- Custom Structural Directive `*hasPermission="'events.create'"` ซ่อน/แสดง UI Component อัตโนมัติตามสิทธิ์
- HTTP Interceptor: แนบ `Authorization: Bearer <token>` ทุก Request + Auto refresh token เมื่อ 401

---

## 2. DATABASE DATA DICTIONARY

### 2.1 Table: `users`
เก็บข้อมูลบัญชีผู้ใช้งานส่วนกลางของทุกบทบาทในระบบ

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสประจำตัวผู้ใช้ (UUID v4) | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `email` | `VARCHAR(255)` | UNIQUE, Index | NO | - | อีเมลสำหรับ Login (RFC 5322) | `somchai.dev@gmail.com` |
| `password_hash` | `TEXT` | - | NO | - | รหัสผ่านแฮชด้วย Argon2id / BCrypt | `$argon2id$v=19$m=65536...` |
| `full_name` | `VARCHAR(200)` | - | NO | - | ชื่อ-นามสกุลจริง | `Somchai Prasert` |
| `phone` | `VARCHAR(20)` | - | YES | NULL | เบอร์โทรศัพท์สากล (E.164 format) | `+66812345678` |
| `avatar_url` | `TEXT` | - | YES | NULL | CDN URL รูปประจำตัว | `https://cdn.ticketworld.io/avatars/u1.webp` |
| `preferred_lang` | `VARCHAR(5)` | - | NO | `'en'` | ภาษาที่เลือก (`th`, `en`, `ja`, `ko`) | `'th'` |
| `preferred_currency`| `VARCHAR(3)` | - | NO | `'THB'` | สกุลเงินแสดงผล (ISO 4217) | `'THB'` |
| `timezone` | `VARCHAR(50)` | - | NO | `'Asia/Bangkok'` | IANA Timezone | `'Asia/Bangkok'` |
| `role` | `VARCHAR(20)` | Index | NO | `'Customer'` | บทบาทหลัก (`Customer`, `Organizer`, `Admin`) | `'Customer'` |
| `is_verified` | `BOOLEAN` | - | NO | `FALSE` | ผ่านการยืนยันอีเมลแล้วหรือไม่ | `TRUE` |
| `is_banned` | `BOOLEAN` | Index | NO | `FALSE` | สถานะระงับการใช้งาน | `FALSE` |
| `created_at` | `TIMESTAMPTZ` | Index | NO | `NOW()` | วันเวลาที่สร้างบัญชี (UTC) | `2026-09-12T12:00:00Z` |
| `updated_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันเวลาที่มีการแก้ไขล่าสุด | `2026-09-12T12:00:00Z` |

---

### 2.2 Table: `refresh_tokens`
จัดเก็บ JWT Refresh Token สำหรับ Token Rotation (ป้องกัน Token Replay Attack)

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัส Token | `aa11bb22-cc33-dd44-ee55-ff6677889900` |
| `user_id` | `UUID` | FK (`users.id`), Index | NO | - | เจ้าของ Token | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `token_hash` | `VARCHAR(128)` | UNIQUE, Index | NO | - | SHA-256 Hash ของ Refresh Token (ไม่เก็บ plain text) | `e3b0c44298fc1c149afb...` |
| `device_info` | `VARCHAR(300)` | - | YES | NULL | ชื่อ Device / Browser / OS (User-Agent parsed) | `Chrome 120 on Windows 11` |
| `ip_address` | `INET` | - | YES | NULL | IP ที่สร้าง Token | `'203.144.144.168'` |
| `expires_at` | `TIMESTAMPTZ` | Index | NO | - | วันหมดอายุ (ปกติ 7 วัน) | `2026-09-19T12:00:00Z` |
| `is_revoked` | `BOOLEAN` | Index | NO | `FALSE` | ถูกเพิกถอนแล้วหรือไม่ | `FALSE` |
| `revoked_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาที่ถูก Revoke | NULL |
| `replaced_by_token` | `VARCHAR(128)` | - | YES | NULL | Hash ของ Token ตัวใหม่ที่มาแทน (Token Rotation) | NULL |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่สร้าง | `2026-09-12T12:00:00Z` |

---

### 2.3 Table: `organizer_profiles`
เก็บข้อมูลนิติบุคคลหรือตัวแทนผู้จัดงาน สำหรับการออกใบเสร็จและการจ่ายเงิน Payout

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสโปรไฟล์ผู้จัดงาน | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |
| `user_id` | `UUID` | FK (`users.id`), UNIQUE | NO | - | เชื่อมโยงบัญชี User หลัก | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `company_name` | `VARCHAR(300)` | Index | NO | - | ชื่อบริษัท / ค่ายเพลง / ผู้จัด | `Live Nation Tero Co., Ltd.` |
| `company_logo_url`| `TEXT` | - | YES | NULL | CDN URL โลโก้ผู้จัด | `https://cdn.ticketworld.io/orgs/logo1.png` |
| `description` | `TEXT` | - | YES | NULL | รายละเอียดประวัติผลงาน | `Global entertainment promoter` |
| `website` | `VARCHAR(500)` | - | YES | NULL | เว็บไซต์ทางการ | `https://www.livenation.co.th` |
| `tax_id` | `VARCHAR(50)` | UNIQUE | YES | NULL | เลขประจำตัวผู้เสียภาษี 13 หลัก | `0105561000000` |
| `bank_account_no`| `VARCHAR(50)` | - | NO | - | เลขที่บัญชีสำหรับรับ Payout | `123-4-56789-0` |
| `bank_name` | `VARCHAR(100)` | - | NO | - | ชื่อธนาคาร / Swift Code | `Kasikornbank (KBANK)` |
| `payout_schedule` | `VARCHAR(20)` | - | NO | `'monthly'` | รอบโอนเงิน (`weekly`, `biweekly`, `monthly`) | `'monthly'` |
| `commission_rate` | `DECIMAL(5,4)` | CHECK (0..1) | NO | `0.0500` | ส่วนแบ่งแพลตฟอร์ม (0.0500 = 5%) | `0.0500` |
| `is_approved` | `BOOLEAN` | Index | NO | `FALSE` | อนุมัติโดย Admin ให้เริ่มขายได้ | `TRUE` |
| `approved_at` | `TIMESTAMPTZ` | - | YES | NULL | วันที่ Admin อนุมัติ | `2026-09-13T08:00:00Z` |
| `approved_by` | `UUID` | FK (`users.id`) | YES | NULL | Admin คนไหนเป็นคนอนุมัติ | `ff112233-4455-6677-8899-aabbccddeeff` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่ลงทะเบียนเป็น Organizer | `2026-09-12T12:00:00Z` |
| `updated_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่แก้ไขล่าสุด | `2026-09-12T12:00:00Z` |

---

### 2.4 Table: `artists`
ข้อมูลศิลปิน / วงดนตรี / วิทยากร

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสศิลปิน | `c3d4e5f6-a7b8-9012-cdef-123456789012` |
| `name` | `VARCHAR(300)` | Index | NO | - | ชื่อศิลปินทางการ | `Coldplay` |
| `slug` | `VARCHAR(300)` | UNIQUE, Index | NO | - | SEO URL slug | `'coldplay'` |
| `bio` | `TEXT` | - | YES | NULL | ประวัติย่อ | `British rock band formed in London in 1997` |
| `profile_image_url`| `TEXT` | - | YES | NULL | รูปโปรไฟล์สี่เหลี่ยมจัตุรัส | `https://cdn.ticketworld.io/artists/coldplay.jpg` |
| `cover_image_url` | `TEXT` | - | YES | NULL | รูป Header Cover Banner | `https://cdn.ticketworld.io/artists/coldplay-cover.jpg` |
| `genre` | `VARCHAR(100)` | Index | YES | NULL | แนวเพลงหลัก | `'Alternative Rock'` |
| `country` | `VARCHAR(100)` | - | YES | NULL | ประเทศต้นกำเนิด | `'United Kingdom'` |
| `social_links` | `JSONB` | - | YES | `'{}'` | ลิงก์โซเชียลมีเดีย | `{"spotify":"...","instagram":"..."}` |
| `follower_count` | `INT` | Index | NO | `0` | ยอด Follower รวม (Denormalized) | `145200` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่บันทึกข้อมูล | `2026-09-12T12:00:00Z` |

---

### 2.5 Table: `artist_followers` (Junction)
ตารางความสัมพันธ์ระหว่าง User กับ Artist ที่ติดตาม

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `user_id` | `UUID` | PK (Composite), FK (`users.id`) | NO | - | ผู้ใช้ที่กดติดตาม | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `artist_id` | `UUID` | PK (Composite), FK (`artists.id`), Index | NO | - | ศิลปินที่ถูกติดตาม | `c3d4e5f6-a7b8-9012-cdef-123456789012` |
| `followed_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่เริ่มติดตาม | `2026-09-12T12:00:00Z` |

> **Trigger:** เมื่อ INSERT/DELETE แถวในตาราง → อัปเดต `artists.follower_count` อัตโนมัติ

---

### 2.6 Table: `venues`
สถานที่จัดงาน รองรับแผนที่พิกัดทางภูมิศาสตร์ (GIS)

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสสถานที่จัดงาน | `d4e5f6a7-b8c9-0123-def1-234567890123` |
| `name` | `VARCHAR(300)` | Index | NO | - | ชื่อสถานที่ | `Rajamangala National Stadium` |
| `slug` | `VARCHAR(300)` | UNIQUE, Index | NO | - | SEO slug | `'rajamangala-national-stadium'` |
| `address` | `TEXT` | - | NO | - | ที่อยู่เต็ม | `Hua Mak, Bang Kapi, Bangkok 10240` |
| `city` | `VARCHAR(100)` | Index | NO | - | เมือง / จังหวัด | `'Bangkok'` |
| `country` | `VARCHAR(100)` | Index | NO | - | ประเทศ | `'Thailand'` |
| `country_code` | `VARCHAR(3)` | Index | NO | - | รหัส ISO 3166-1 alpha-2 | `'TH'` |
| `latitude` | `DECIMAL(10,7)`| - | YES | NULL | พิกัดละติจูด | `13.7553170` |
| `longitude` | `DECIMAL(10,7)`| - | YES | NULL | พิกัดลองจิจูด | `100.6222850` |
| `total_capacity` | `INT` | - | NO | `0` | ความจุผู้ชมสูงสุด | `51552` |
| `seat_map_template`| `JSONB` | - | YES | NULL | Layout SVG / Vector Template | `{"zones":[{"name":"A1","nodes":[...]}]}` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่บันทึก | `2026-09-12T12:00:00Z` |

---

### 2.7 Table: `events`
หัวใจของระบบ จัดเก็บข้อมูลคอนเสิร์ตและสถานะวงจรชีวิตของงาน

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสประจำอีเวนต์ | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `organizer_id` | `UUID` | FK (`organizer_profiles.id`), Index | NO | - | รหัสผู้จัดที่เป็นเจ้าของงาน | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |
| `venue_id` | `UUID` | FK (`venues.id`), Index | YES | NULL | สถานที่จัดงาน (NULL = Online Event) | `d4e5f6a7-b8c9-0123-def1-234567890123` |
| `title` | `VARCHAR(500)` | FTS Index | NO | - | ชื่องาน (รองรับ Full-Text Search) | `Coldplay Music of the Spheres World Tour` |
| `slug` | `VARCHAR(500)` | UNIQUE, Index | NO | - | URL Path Slug | `'coldplay-mot-spheres-bangkok-2026'` |
| `description` | `TEXT` | - | YES | NULL | รายละเอียดและกติกาเข้างาน | `Detailed concert description and T&C...` |
| `short_description`| `VARCHAR(300)`| - | YES | NULL | สรุปสั้นสำหรับ Social Share Card | `The spectacular world tour returns to BKK!` |
| `poster_url` | `TEXT` | - | NO | - | โปสเตอร์แนวตั้ง (3:4 ratio) | `https://cdn.ticketworld.io/posters/coldplay2026.webp` |
| `banner_url` | `TEXT` | - | YES | NULL | แบนเนอร์แนวนอน (16:9 ratio) | `https://cdn.ticketworld.io/banners/coldplay-wide.webp` |
| `category` | `VARCHAR(100)` | Index | NO | - | หมวดหมู่ (`Concert`, `Festival`, `FanMeeting`, `Sports`, `Conference`) | `'Concert'` |
| `tags` | `TEXT[]` | Index (GIN) | YES | `'{}'` | แท็กคำค้นหา | `ARRAY['kpop','worldtour','stadium']` |
| `status` | `VARCHAR(30)` | Index | NO | `'Draft'` | ดูหัวข้อ [5.1 Event State Machine](#51-event-state-machine) | `'OnSale'` |
| `event_start` | `TIMESTAMPTZ` | Index | NO | - | วันเวลาเริ่มการแสดง (UTC) | `2026-11-20T12:00:00Z` |
| `event_end` | `TIMESTAMPTZ` | - | YES | NULL | วันเวลาสิ้นสุดการแสดง (UTC) | `2026-11-20T16:00:00Z` |
| `doors_open` | `TIMESTAMPTZ` | - | YES | NULL | ประตูเปิด | `2026-11-20T10:00:00Z` |
| `sale_start` | `TIMESTAMPTZ` | Index | NO | - | วันเวลาที่เปิดให้กดซื้อตั๋ว | `2026-10-01T03:00:00Z` |
| `sale_end` | `TIMESTAMPTZ` | - | NO | - | วันเวลาที่ปิดรับการจอง | `2026-11-20T10:00:00Z` |
| `min_price` | `DECIMAL(12,2)`| Index | NO | `0.00` | ราคาเริ่มต้น (Denormalized) | `2800.00` |
| `max_price` | `DECIMAL(12,2)`| - | NO | `0.00` | ราคาสูงสุด (Denormalized) | `12500.00` |
| `currency` | `VARCHAR(3)` | - | NO | `'THB'` | สกุลเงินฐาน (ISO 4217) | `'THB'` |
| `max_tickets_per_user`| `INT` | - | NO | `4` | จำกัดตั๋วต่อ User (ป้องกันบอท) | `4` |
| `hold_duration_minutes`| `INT` | - | NO | `10` | ระยะเวลา Hold ที่นั่ง (นาที) | `10` |
| `is_featured` | `BOOLEAN` | Index | NO | `FALSE` | แสดง Hero Carousel หน้าแรก | `TRUE` |
| `total_seats` | `INT` | - | NO | `0` | จำนวนที่นั่งทั้งหมด (Denormalized) | `50000` |
| `sold_seats` | `INT` | - | NO | `0` | จำนวนที่ขายสำเร็จแล้ว (Denormalized) | `48200` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่สร้าง | `2026-09-12T12:00:00Z` |
| `updated_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่แก้ไขล่าสุด | `2026-09-12T12:00:00Z` |
| `published_at` | `TIMESTAMPTZ` | - | YES | NULL | วันที่ Publish สำเร็จ | `2026-09-15T09:00:00Z` |

---

### 2.8 Table: `event_artists` (Junction)
ตารางเชื่อมศิลปินกับอีเวนต์ รองรับ Line-up หลายคน

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `event_id` | `UUID` | PK (Composite), FK (`events.id`) | NO | - | อีเวนต์ | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `artist_id` | `UUID` | PK (Composite), FK (`artists.id`), Index | NO | - | ศิลปิน | `c3d4e5f6-a7b8-9012-cdef-123456789012` |
| `is_headliner` | `BOOLEAN` | - | NO | `FALSE` | เป็นศิลปินหลักหรือไม่ (แสดง Badge บน UI) | `TRUE` |
| `display_order` | `INT` | - | NO | `0` | ลำดับแสดงผลใน Line-up | `1` |

---

### 2.9 Table: `zones`
โซนที่นั่งและโครงสร้างราคา

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสโซน | `f6a7b8c9-d0e1-2345-f123-456789012345` |
| `event_id` | `UUID` | FK (`events.id`), Index | NO | - | ผูกกับ Event | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `name` | `VARCHAR(100)` | - | NO | - | ชื่อโซน | `'VIP Experience'` |
| `color` | `VARCHAR(7)` | - | NO | `'#3B82F6'` | รหัสสี Hex สำหรับ Interactive Map | `'#EC4899'` |
| `price` | `DECIMAL(12,2)`| Index | NO | - | ราคาตั๋วต่อที่นั่ง (ไม่รวม Fee) | `12500.00` |
| `currency` | `VARCHAR(3)` | - | NO | `'THB'` | สกุลเงินประจำโซน | `'THB'` |
| `capacity` | `INT` | - | NO | - | ความจุทั้งหมดของโซน | `500` |
| `is_numbered_seat`| `BOOLEAN` | - | NO | `TRUE` | `TRUE` = เลือกที่นั่งได้ / `FALSE` = โซนยืน (GA) | `TRUE` |
| `sort_order` | `INT` | - | NO | `0` | ลำดับแสดงผล | `1` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่สร้าง | `2026-09-12T12:00:00Z` |

---

### 2.10 Table: `seats`
ที่นั่งรายตัว พร้อม Optimistic Concurrency Control

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสที่นั่ง | `01a2b3c4-d5e6-7890-abcd-ef1234567890` |
| `zone_id` | `UUID` | FK (`zones.id`), Index | NO | - | โซนสังกัด | `f6a7b8c9-d0e1-2345-f123-456789012345` |
| `row_label` | `VARCHAR(10)` | - | YES | NULL | ชื่อแถว | `'A'` |
| `seat_number` | `VARCHAR(10)` | - | YES | NULL | หมายเลขเก้าอี้ | `'15'` |
| `display_label` | `VARCHAR(20)` | - | NO | - | ป้ายแสดงผลเต็ม | `'VIP-A-15'` |
| `status` | `VARCHAR(20)` | Index | NO | `'Available'` | ดูหัวข้อ [5.2 Seat State Machine](#52-seat-state-machine) | `'Hold'` |
| `held_by` | `UUID` | FK (`users.id`), Index | YES | NULL | ผู้ใช้ที่กำลัง Hold | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `held_until` | `TIMESTAMPTZ` | Index | YES | NULL | เวลาที่ Hold จะหลุด (UTC) | `2026-09-12T12:35:00Z` |
| `pos_x` | `INT` | - | YES | `0` | พิกัดแกน X บน Canvas | `450` |
| `pos_y` | `INT` | - | YES | `0` | พิกัดแกน Y บน Canvas | `120` |
| `version` | `INT` | - | NO | `1` | **Optimistic Concurrency Version Counter** | `3` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่สร้าง | `2026-09-12T12:00:00Z` |
| `updated_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่แก้ไข | `2026-09-12T12:25:00Z` |

---

### 2.11 Table: `bookings`
คำสั่งจองตั๋ว (Order Record)

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสคำสั่งจอง | `12b3c4d5-e6f7-8901-bcde-f12345678901` |
| `booking_number` | `VARCHAR(25)` | UNIQUE, Index | NO | - | เลขอ้างอิงมนุษย์อ่านได้ | `'TW-20260912-8924A'` |
| `user_id` | `UUID` | FK (`users.id`), Index | NO | - | ผู้ซื้อ | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `event_id` | `UUID` | FK (`events.id`), Index | NO | - | งานที่จอง | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `status` | `VARCHAR(20)` | Index | NO | `'Pending'` | ดู [5.3 Booking State Machine](#53-booking-state-machine) | `'Confirmed'` |
| `total_amount` | `DECIMAL(12,2)`| - | NO | - | ยอดรวมตั๋ว (Gross) | `25000.00` |
| `discount_amount`| `DECIMAL(12,2)`| - | NO | `0.00` | ส่วนลดจาก Promo | `2500.00` |
| `fee_amount` | `DECIMAL(12,2)`| - | NO | `0.00` | ค่าธรรมเนียมระบบ (Convenience Fee) | `200.00` |
| `net_amount` | `DECIMAL(12,2)`| - | NO | - | ยอดสุทธิ (Total - Discount + Fee) | `22700.00` |
| `currency` | `VARCHAR(3)` | - | NO | `'THB'` | สกุลเงิน | `'THB'` |
| `promo_code_id` | `UUID` | FK (`promo_codes.id`) | YES | NULL | โค้ดส่วนลดที่ใช้ | `23c4d5e6-f7a8-9012-cdef-123456789012` |
| `expires_at` | `TIMESTAMPTZ` | Index | YES | NULL | เวลาที่จะ Auto-Cancel ถ้ายังไม่จ่าย | `2026-09-12T12:35:00Z` |
| `confirmed_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาชำระเงินสำเร็จ | `2026-09-12T12:28:15Z` |
| `cancelled_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาที่ยกเลิก | NULL |
| `cancellation_reason`| `TEXT` | - | YES | NULL | สาเหตุการยกเลิก | NULL |
| `created_at` | `TIMESTAMPTZ` | Index | NO | `NOW()` | วันที่สั่งจอง | `2026-09-12T12:25:00Z` |
| `updated_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่อัปเดต | `2026-09-12T12:28:15Z` |

---

### 2.12 Table: `booking_items`
ตั๋วใบย่อยรายที่นั่ง (Individual E-Ticket with QR Code)

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสตั๋วใบนี้ | `34d5e6f7-a8b9-0123-def1-234567890123` |
| `booking_id` | `UUID` | FK (`bookings.id`), Index | NO | - | รหัสออเดอร์แม่ | `12b3c4d5-e6f7-8901-bcde-f12345678901` |
| `seat_id` | `UUID` | FK (`seats.id`), Index | NO | - | รหัสที่นั่งที่ผูก | `01a2b3c4-d5e6-7890-abcd-ef1234567890` |
| `zone_name` | `VARCHAR(100)` | - | NO | - | ชื่อโซน ณ เวลาซื้อ (Snapshot) | `'VIP Experience'` |
| `seat_label` | `VARCHAR(20)` | - | NO | - | ป้ายที่นั่ง ณ เวลาซื้อ (Snapshot) | `'VIP-A-15'` |
| `price` | `DECIMAL(12,2)`| - | NO | - | ราคาต่อใบ ณ เวลาซื้อ | `12500.00` |
| `ticket_code` | `VARCHAR(64)` | UNIQUE, Index | NO | - | Token พิสูจน์ตั๋ว (HMAC-SHA256) | `'TK-COLDPLAY-89A7B3901C'` |
| `qr_code_url` | `TEXT` | - | YES | NULL | URL รูป QR Code บน S3/MinIO | `https://cdn.ticketworld.io/qr/tk89a.png` |
| `is_checked_in` | `BOOLEAN` | Index | NO | `FALSE` | สแกนเข้างานแล้วหรือไม่ | `FALSE` |
| `checked_in_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาสแกนเข้าประตู | NULL |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่สร้าง | `2026-09-12T12:28:15Z` |

---

### 2.13 Table: `payments`
ธุรกรรมทางการเงิน ⇔ Payment Gateway

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสธุรกรรม | `45e6f7a8-b9c0-1234-ef12-345678901234` |
| `booking_id` | `UUID` | FK (`bookings.id`), Index | NO | - | อ้างอิงออเดอร์ | `12b3c4d5-e6f7-8901-bcde-f12345678901` |
| `method` | `VARCHAR(30)` | - | NO | - | วิธีชำระ (`CreditCard`, `PromptPay`, `PayPal`, `BankTransfer`) | `'CreditCard'` |
| `gateway` | `VARCHAR(30)` | - | NO | - | ผู้ให้บริการ (`Stripe`, `Omise`, `Mock`) | `'Stripe'` |
| `gateway_tx_id` | `VARCHAR(200)` | UNIQUE, Index | YES | NULL | Charge ID ของ Gateway | `'ch_3MtwL2LkdIwHu7ix28a3tqZ'` |
| `amount` | `DECIMAL(12,2)`| - | NO | - | ยอดเงินที่ตัดจริง | `22700.00` |
| `currency` | `VARCHAR(3)` | - | NO | `'THB'` | สกุลเงินที่ตัดบัตร | `'THB'` |
| `status` | `VARCHAR(20)` | Index | NO | `'Pending'` | `Pending`, `Completed`, `Failed`, `Refunded` | `'Completed'` |
| `paid_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาที่ Gateway ตอบรับสำเร็จ | `2026-09-12T12:28:14Z` |
| `refunded_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาที่คืนเงินสำเร็จ | NULL |
| `raw_response` | `JSONB` | - | YES | NULL | Raw Webhook payload สำหรับ Audit | `{"id":"ch_xxx","status":"succeeded"}` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | เวลาเริ่มธุรกรรม | `2026-09-12T12:27:00Z` |

---

### 2.14 Table: `promo_codes`
โค้ดส่วนลด / คูปอง ผูกกับ Event เฉพาะหรือใช้ได้ทั้ง Platform

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสโค้ด | `23c4d5e6-f7a8-9012-cdef-123456789012` |
| `event_id` | `UUID` | FK (`events.id`), Index | YES | NULL | ผูกเฉพาะ Event (NULL = ใช้ได้ทุก Event) | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `organizer_id` | `UUID` | FK (`organizer_profiles.id`), Index | YES | NULL | ผู้สร้าง (Organizer หรือ Admin) | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |
| `code` | `VARCHAR(50)` | UNIQUE, Index | NO | - | รหัสโค้ด (ตัวอักษรพิมพ์ใหญ่) | `'EARLYBIRD2026'` |
| `discount_type` | `VARCHAR(10)` | - | NO | - | ประเภทส่วนลด (`percent` หรือ `fixed`) | `'percent'` |
| `discount_value` | `DECIMAL(12,2)`| - | NO | - | ค่าส่วนลด (10 = 10% หรือ 500 = ฿500) | `10.00` |
| `max_uses` | `INT` | - | YES | NULL | จำนวนครั้งที่ใช้ได้สูงสุด (NULL = ไม่จำกัด) | `500` |
| `used_count` | `INT` | - | NO | `0` | จำนวนครั้งที่ถูกใช้ไปแล้ว | `123` |
| `max_discount_amount`| `DECIMAL(12,2)` | - | YES | NULL | ยอดส่วนลดสูงสุดต่อครั้ง (NULL = ไม่จำกัด) | `1000.00` |
| `min_purchase` | `DECIMAL(12,2)`| - | YES | NULL | ยอดสั่งซื้อขั้นต่ำที่ใช้ได้ | `2000.00` |
| `valid_from` | `TIMESTAMPTZ` | Index | YES | NULL | วันเริ่มใช้โค้ดได้ | `2026-10-01T00:00:00Z` |
| `valid_until` | `TIMESTAMPTZ` | Index | YES | NULL | วันหมดอายุ | `2026-11-15T23:59:59Z` |
| `is_active` | `BOOLEAN` | Index | NO | `TRUE` | เปิดใช้งานอยู่หรือไม่ | `TRUE` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่สร้าง | `2026-09-12T12:00:00Z` |

---

### 2.15 Table: `payouts`
ระบบจ่ายเงินให้ Organizer (Platform → Organizer Bank Account)

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสรายการ Payout | `56f7a8b9-c0d1-2345-f123-456789012345` |
| `organizer_id` | `UUID` | FK (`organizer_profiles.id`), Index | NO | - | Organizer ที่ได้รับเงิน | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |
| `event_id` | `UUID` | FK (`events.id`), Index | YES | NULL | Event ที่เป็นแหล่งรายได้ (NULL = สรุปรวมหลาย Event) | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `payout_number` | `VARCHAR(25)` | UNIQUE, Index | NO | - | เลขที่อ้างอิง Payout | `'PO-20261201-0042'` |
| `gross_amount` | `DECIMAL(12,2)`| - | NO | - | ยอดขายรวม (Gross Sales) | `5000000.00` |
| `commission_amount`| `DECIMAL(12,2)` | - | NO | - | ค่าคอมมิชชัน Platform ที่หัก | `250000.00` (5%) |
| `net_amount` | `DECIMAL(12,2)`| - | NO | - | ยอดสุทธิที่โอนให้ Organizer | `4750000.00` |
| `currency` | `VARCHAR(3)` | - | NO | `'THB'` | สกุลเงิน | `'THB'` |
| `status` | `VARCHAR(20)` | Index | NO | `'Pending'` | `Pending`, `Processing`, `Completed`, `Failed` | `'Completed'` |
| `bank_account_snapshot` | `VARCHAR(50)` | - | NO | - | เลขบัญชี ณ เวลาโอน (Snapshot) | `'123-4-56789-0'` |
| `bank_name_snapshot` | `VARCHAR(100)` | - | NO | - | ชื่อธนาคาร ณ เวลาโอน (Snapshot) | `'Kasikornbank'` |
| `bank_ref` | `VARCHAR(100)` | - | YES | NULL | เลข Reference การโอนจากธนาคาร | `'KBANK-20261201-TRF-99281'` |
| `processed_by` | `UUID` | FK (`users.id`) | YES | NULL | Admin ที่อนุมัติ | `ff112233-4455-6677-8899-aabbccddeeff` |
| `processed_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาที่โอนเงินจริง | `2026-12-01T08:00:00Z` |
| `period_start` | `DATE` | - | NO | - | ช่วงเวลาสรุปเริ่มต้น | `2026-11-01` |
| `period_end` | `DATE` | - | NO | - | ช่วงเวลาสรุปสิ้นสุด | `2026-11-30` |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | วันที่สร้างรายการ | `2026-12-01T00:00:00Z` |

---

### 2.16 Table: `notifications`
การแจ้งเตือนแบบ In-App สำหรับทุก Role

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสการแจ้งเตือน | `67a8b9c0-d1e2-3456-a123-456789012345` |
| `user_id` | `UUID` | FK (`users.id`), Index | NO | - | ผู้รับ | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `type` | `VARCHAR(50)` | Index | NO | - | ประเภท (ดู Notification Types ด้านล่าง) | `'BookingConfirmed'` |
| `title` | `VARCHAR(300)` | - | NO | - | หัวข้อการแจ้งเตือน | `'การจองตั๋วสำเร็จ!'` |
| `message` | `TEXT` | - | YES | NULL | รายละเอียดเพิ่มเติม | `'ตั๋ว Coldplay 2 ใบ ยืนยันแล้ว'` |
| `link` | `TEXT` | - | YES | NULL | Deep link ไปหน้าที่เกี่ยวข้อง | `'/my-tickets/12b3c4d5'` |
| `metadata` | `JSONB` | - | YES | `'{}'` | ข้อมูลเพิ่มเติม (eventId, bookingId ฯลฯ) | `{"eventId":"e5f6...","bookingId":"12b3..."}` |
| `is_read` | `BOOLEAN` | Index | NO | `FALSE` | อ่านแล้วหรือยัง | `FALSE` |
| `created_at` | `TIMESTAMPTZ` | Index | NO | `NOW()` | เวลาที่สร้างการแจ้งเตือน | `2026-09-12T12:28:20Z` |

**Notification Types:**

| Type Code | Trigger Event | Target Role |
| :--- | :--- | :--- |
| `BookingConfirmed` | ชำระเงินสำเร็จ | Customer |
| `BookingCancelled` | ออเดอร์ถูกยกเลิก (หมดเวลา / ผู้ใช้กดยกเลิก) | Customer |
| `RefundProcessed` | คืนเงินสำเร็จ | Customer |
| `EventReminder` | 24 ชั่วโมงก่อนงานเริ่ม (Hangfire Cron) | Customer |
| `ArtistNewEvent` | ศิลปินที่ Follow มีงานใหม่ | Customer (Followers) |
| `WaitlistAvailable` | มีที่นั่งว่าง (จาก Waitlist) | Customer |
| `EventApproved` | Admin อนุมัติ Event สำเร็จ | Organizer |
| `EventRejected` | Admin ปฏิเสธ Event พร้อมเหตุผล | Organizer |
| `PayoutCompleted` | โอนเงินเข้าบัญชี Organizer สำเร็จ | Organizer |
| `NewEventSubmitted` | มี Event ใหม่ส่งเข้าคิวรอ Approve | Admin |

---

### 2.17 Table: `reviews`
รีวิว Event โดยผู้ที่เข้าร่วมจริงเท่านั้น (ต้องมี Booking ที่ Confirmed + event_end ผ่านแล้ว)

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสรีวิว | `78b9c0d1-e2f3-4567-b234-567890123456` |
| `user_id` | `UUID` | FK (`users.id`), Index | NO | - | ผู้เขียน | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `event_id` | `UUID` | FK (`events.id`), Index | NO | - | Event ที่รีวิว | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `rating` | `SMALLINT` | CHECK (1-5) | NO | - | คะแนน 1-5 ดาว | `5` |
| `comment` | `TEXT` | - | YES | NULL | ข้อความรีวิว | `'คอนเสิร์ตสุดยอดมาก!'` |
| `is_hidden` | `BOOLEAN` | - | NO | `FALSE` | Admin ซ่อนรีวิวที่ไม่เหมาะสม | `FALSE` |
| `created_at` | `TIMESTAMPTZ` | Index | NO | `NOW()` | วันที่เขียนรีวิว | `2026-11-21T08:00:00Z` |
| UNIQUE Constraint | | (`user_id`, `event_id`) | | | 1 คน รีวิวได้ 1 ครั้งต่อ Event | |

---

### 2.18 Table: `waitlist`
ระบบลงทะเบียนรอตั๋ว เมื่อ Event หรือ Zone ที่ต้องการขายหมดแล้ว

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัส Waitlist Entry | `89c0d1e2-f3a4-5678-c345-678901234567` |
| `user_id` | `UUID` | FK (`users.id`), Index | NO | - | ผู้ใช้ที่ลงทะเบียนรอ | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `event_id` | `UUID` | FK (`events.id`), Index | NO | - | Event ที่ต้องการ | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `zone_id` | `UUID` | FK (`zones.id`) | YES | NULL | โซนที่ต้องการ (NULL = โซนใดก็ได้) | `f6a7b8c9-d0e1-2345-f123-456789012345` |
| `quantity` | `INT` | CHECK (1-10) | NO | `1` | จำนวนที่นั่งที่ต้องการ | `2` |
| `status` | `VARCHAR(20)` | Index | NO | `'Waiting'` | `Waiting`, `Notified`, `Converted`, `Expired` | `'Waiting'` |
| `notified_at` | `TIMESTAMPTZ` | - | YES | NULL | เวลาที่ส่งแจ้งเตือนว่ามีที่นั่งว่าง | NULL |
| `priority` | `INT` | Index | NO | - | ลำดับคิว (มาก่อนได้ก่อน) | `1` |
| `created_at` | `TIMESTAMPTZ` | Index | NO | `NOW()` | วันที่ลงทะเบียน | `2026-10-05T14:30:00Z` |
| UNIQUE Constraint | | (`user_id`, `event_id`) | | | 1 คนลงทะเบียน 1 ครั้งต่อ Event | |

---

### 2.19 Table: `audit_logs` (Immutable)
บันทึกประวัติการเปลี่ยนแปลงข้อมูลสำคัญ (ไม่สามารถ UPDATE หรือ DELETE ได้)

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `BIGSERIAL` | PK | NO | Auto | ลำดับ (Monotonic) | `98401` |
| `user_id` | `UUID` | FK (`users.id`), Index | YES | NULL | ผู้กระทำ (NULL = System Job) | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `action` | `VARCHAR(100)` | Index | NO | - | การกระทำ | `'Event.Approve'` |
| `entity_type` | `VARCHAR(50)` | Index | NO | - | ชนิด Object | `'Event'` |
| `entity_id` | `UUID` | Index | YES | NULL | ID ของ Object | `e5f6a7b8-c9d0-1234-ef12-345678901234` |
| `old_values` | `JSONB` | - | YES | NULL | Snapshot ก่อนเปลี่ยน | `{"status":"PendingApproval"}` |
| `new_values` | `JSONB` | - | YES | NULL | Snapshot หลังเปลี่ยน | `{"status":"Published"}` |
| `ip_address` | `INET` | - | YES | NULL | IP ต้นทาง | `'203.144.144.168'` |
| `user_agent` | `TEXT` | - | YES | NULL | Browser/Device | `'Mozilla/5.0...'` |
| `created_at` | `TIMESTAMPTZ` | Index | NO | `NOW()` | วันเวลาที่เกิด (UTC) | `2026-09-12T12:30:00Z` |

---

### 2.20 Table: `platform_configs`
Feature Flags & Platform Settings สำหรับ Admin ควบคุมพฤติกรรมระบบ

| Column Name | Data Type | Constraint | Nullable | Default | Description / Business Rule | Sample Value |
| :--- | :--- | :--- | :---: | :--- | :--- | :--- |
| `key` | `VARCHAR(100)` | PK | NO | - | ชื่อ Config Key | `'maintenance_mode'` |
| `value` | `JSONB` | - | NO | - | ค่า Config (รองรับทุกประเภท) | `{"enabled":false}` |
| `description` | `TEXT` | - | YES | NULL | คำอธิบาย Config สำหรับ Admin | `'Toggle to enable/disable maintenance mode'` |
| `updated_by` | `UUID` | FK (`users.id`) | YES | NULL | Admin ที่แก้ไขล่าสุด | `ff112233-4455-6677-8899-aabbccddeeff` |
| `updated_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | เวลาที่แก้ไขล่าสุด | `2026-09-12T12:00:00Z` |

**ตัวอย่าง Config Keys:**

| Key | Sample Value | Description |
| :--- | :--- | :--- |
| `maintenance_mode` | `{"enabled": false, "message": "..."}` | เปิด/ปิดโหมดปิดปรับปรุงระบบ |
| `default_commission_rate` | `{"rate": 0.05}` | ค่าคอมมิชชัน Default สำหรับ Organizer ใหม่ |
| `default_hold_minutes` | `{"minutes": 10}` | ระยะเวลา Hold Default |
| `max_tickets_per_user_global` | `{"max": 8}` | จำกัดตั๋วสูงสุดต่อคนต่อ Event (Platform-wide cap) |
| `supported_currencies` | `{"currencies": ["THB","USD","JPY","KRW","EUR"]}` | สกุลเงินที่รองรับ |
| `supported_payment_methods` | `{"methods": ["CreditCard","PromptPay","PayPal"]}` | วิธีชำระเงินที่เปิดใช้ |
| `enable_waitlist` | `{"enabled": true}` | เปิด/ปิดระบบ Waitlist |
| `enable_reviews` | `{"enabled": true}` | เปิด/ปิดระบบรีวิว |

---

## 3. API DATA DICTIONARY & ENDPOINT CONTRACT

### 3.1 Standard Response Envelopes

#### Success (2xx):
```json
{
  "success": true,
  "data": {},
  "meta": { "page": 1, "pageSize": 20, "totalCount": 150, "totalPages": 8 },
  "timestamp": "2026-09-12T12:30:00Z"
}
```

#### Error (4xx/5xx — RFC 7807 ProblemDetails):
```json
{
  "success": false,
  "error": {
    "code": "SEAT_ALREADY_HELD",
    "message": "Selected seat(s) are currently held by another user.",
    "details": [{ "field": "seatIds", "issue": "Seat VIP-A-15 is locked until 12:35:00Z" }],
    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
  },
  "timestamp": "2026-09-12T12:30:00Z"
}
```

---

### 3.2 Complete Endpoint Catalog

#### 🔐 Module: Authentication & Identity

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Public | สมัครสมาชิก (ส่ง Verification Email) |
| `POST` | `/api/v1/auth/login` | Public | เข้าสู่ระบบ (Return AccessToken + RefreshToken) |
| `POST` | `/api/v1/auth/refresh-token` | Public | ขอ Access Token ใหม่ด้วย Refresh Token |
| `POST` | `/api/v1/auth/logout` | Authenticated | Revoke Refresh Token ปัจจุบัน |
| `POST` | `/api/v1/auth/logout-all` | Authenticated | Revoke ทุก Refresh Token ของ User (Logout ทุกเครื่อง) |
| `POST` | `/api/v1/auth/verify-email` | Public | ยืนยันอีเมลจากลิงก์ที่ส่ง (`?token=xxx`) |
| `POST` | `/api/v1/auth/forgot-password` | Public | ขอลิงก์รีเซ็ตรหัสผ่านทางอีเมล |
| `POST` | `/api/v1/auth/reset-password` | Public | ตั้งรหัสผ่านใหม่ด้วย Reset Token |
| `POST` | `/api/v1/auth/change-password` | Authenticated | เปลี่ยนรหัสผ่าน (ต้องกรอกรหัสผ่านเดิม) |
| `GET` | `/api/v1/auth/me` | Authenticated | ดึงโปรไฟล์ Current User |
| `PUT` | `/api/v1/auth/me` | Authenticated | แก้ไขโปรไฟล์ (ชื่อ, โทร, ภาษา, สกุลเงิน, Timezone) |

---

#### 🎟️ Module: Events (Customer)

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/events` | Public | ค้นหาอีเวนต์ + Filter (q, category, city, date, price range) + Pagination |
| `GET` | `/api/v1/events/featured` | Public | อีเวนต์ที่ถูก Feature ในหน้าแรก |
| `GET` | `/api/v1/events/trending` | Public | อีเวนต์ยอดนิยม (จากยอดขาย 7 วันล่าสุด) |
| `GET` | `/api/v1/events/{slug}` | Public | รายละเอียด Event พร้อม Line-up ศิลปิน, ราคาแต่ละโซน |
| `GET` | `/api/v1/events/{slug}/seats` | Public | ผังที่นั่งพร้อมสถานะ Real-time |

---

#### 💺 Module: Seats & Booking (Customer)

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/seats/hold` | Customer | Hold ที่นั่ง 10 นาที (Concurrency Control) |
| `DELETE` | `/api/v1/seats/hold` | Customer (Owner) | ปล่อยที่นั่งที่ Hold ก่อนหมดเวลา (`seatIds` ใน request body) |
| `POST` | `/api/v1/bookings/{id}/checkout` | Customer (Owner) | ชำระเงิน |
| `GET` | `/api/v1/bookings/{id}` | Customer (Owner) | รายละเอียดออเดอร์ |
| `POST` | `/api/v1/bookings/{id}/cancel` | Customer (Owner) | ขอยกเลิก (ตามเงื่อนไข Refund Policy) |
| `GET` | `/api/v1/my-tickets` | Customer | รายการตั๋วทั้งหมด (upcoming/past/cancelled) |
| `GET` | `/api/v1/my-tickets/{id}/qr` | Customer (Owner) | ดาวน์โหลด QR Code ตั๋ว (PDF) |
| `POST` | `/api/v1/promo-codes/validate` | Customer | ตรวจสอบโค้ดส่วนลดก่อน Checkout |

---

#### 🎤 Module: Artists

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/artists` | Public | ค้นหาศิลปิน + Filter (genre, country) |
| `GET` | `/api/v1/artists/{slug}` | Public | โปรไฟล์ศิลปิน + Events ที่จะมา |
| `POST` | `/api/v1/artists/{slug}/follow` | Customer | กดติดตามศิลปิน |
| `DELETE` | `/api/v1/artists/{slug}/follow` | Customer | เลิกติดตาม |
| `GET` | `/api/v1/my-following` | Customer | รายชื่อศิลปินที่ติดตามอยู่ |

---

#### ⭐ Module: Reviews

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/events/{slug}/reviews` | Public | ดูรีวิวทั้งหมดของ Event (+ avg rating) |
| `POST` | `/api/v1/events/{slug}/reviews` | Customer (Attended) | เขียนรีวิว (ต้องมี Confirmed Booking + event ผ่านแล้ว) |
| `PUT` | `/api/v1/reviews/{id}` | Customer (Owner) | แก้ไขรีวิวตนเอง |
| `DELETE` | `/api/v1/reviews/{id}` | Customer (Owner) / Admin | ลบรีวิว |

---

#### ⏳ Module: Waitlist

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/events/{slug}/waitlist` | Customer | ลงทะเบียนรอตั๋ว (เมื่อ SoldOut) |
| `DELETE` | `/api/v1/events/{slug}/waitlist` | Customer | ยกเลิกการลงทะเบียนรอ |
| `GET` | `/api/v1/my-waitlist` | Customer | ดูรายการ Event ที่กำลังรออยู่ |

---

#### 🔔 Module: Notifications

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/notifications` | Authenticated | รายการแจ้งเตือนของตนเอง (Paginated) |
| `GET` | `/api/v1/notifications/unread-count` | Authenticated | จำนวนที่ยังไม่อ่าน (สำหรับ Badge Icon) |
| `PUT` | `/api/v1/notifications/{id}/read` | Authenticated | Mark as Read |
| `PUT` | `/api/v1/notifications/read-all` | Authenticated | Mark All as Read |

---

#### 📁 Module: File Upload

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/files/presigned-url` | Organizer / Admin | ขอ Presigned URL สำหรับ Upload ไฟล์ไปยัง S3/MinIO |
| `POST` | `/api/v1/files/confirm` | Organizer / Admin | ยืนยันว่า Upload เสร็จแล้ว (เพื่อบันทึก URL ลง DB) |

---

#### 📊 Module: Organizer Dashboard

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/organizer/dashboard` | Organizer | Overview stats (ยอดขายวันนี้, รายได้เดือนนี้, Event ที่ On Sale) |
| `GET` | `/api/v1/organizer/events` | Organizer | รายการ Event ทั้งหมดของ Organizer + Filter by status |
| `POST` | `/api/v1/organizer/events` | Organizer | สร้าง Event ใหม่ (Draft) |
| `PUT` | `/api/v1/organizer/events/{id}` | Organizer (Owner) | แก้ไข Event |
| `POST` | `/api/v1/organizer/events/{id}/publish` | Organizer (Owner) | ส่ง Event เข้าคิวอนุมัติ |
| `POST` | `/api/v1/organizer/events/{id}/cancel` | Organizer (Owner) | ยกเลิก Event |
| `POST` | `/api/v1/organizer/events/{id}/seats/generate` | Organizer (Owner) | Bulk สร้างผังที่นั่ง |
| `GET` | `/api/v1/organizer/events/{id}/orders` | Organizer (Owner) | รายการออเดอร์ของ Event + Export CSV |
| `GET` | `/api/v1/organizer/events/{id}/attendees` | Organizer (Owner) | รายชื่อผู้เข้าร่วม |
| `POST` | `/api/v1/organizer/events/{id}/checkin` | Organizer (Owner) | สแกน QR Code ตั๋ว |
| `GET` | `/api/v1/organizer/events/{id}/analytics` | Organizer (Owner) | Sales Chart, Revenue, Seat Heatmap, Conversion |
| `GET` | `/api/v1/organizer/events/{id}/waitlist` | Organizer (Owner) | ดูรายชื่อ Waitlist |
| `POST` | `/api/v1/organizer/promo-codes` | Organizer | สร้าง Promo Code |
| `GET` | `/api/v1/organizer/promo-codes` | Organizer | รายการ Promo Code ทั้งหมด |
| `PUT` | `/api/v1/organizer/promo-codes/{id}` | Organizer (Owner) | แก้ไข Promo Code |
| `DELETE` | `/api/v1/organizer/promo-codes/{id}` | Organizer (Owner) | ลบ Promo Code |
| `GET` | `/api/v1/organizer/finance` | Organizer | สรุปรายได้ + Payout History |
| `POST` | `/api/v1/organizer/finance/payout-request` | Organizer | ยื่นขอเบิกเงิน |
| `PUT` | `/api/v1/organizer/settings` | Organizer | แก้ไข Organizer Profile (บริษัท, บัญชีธนาคาร) |

---

#### 🛠️ Module: Admin Panel

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/admin/dashboard` | Admin | GMV, Active Users, Events, Bookings, Error Rate |
| `GET` | `/api/v1/admin/users` | Admin | ตาราง User ทั้งหมด + Search + Filter by Role |
| `PUT` | `/api/v1/admin/users/{id}/ban` | Admin | Ban / Unban User |
| `PUT` | `/api/v1/admin/users/{id}/role` | Admin | เปลี่ยน Role (Customer → Organizer → Admin) |
| `GET` | `/api/v1/admin/events` | Admin | Event ทั้งหมด + คิว Moderation |
| `POST` | `/api/v1/admin/events/{id}/moderate` | Admin | Approve / Reject / RequestRevision |
| `GET` | `/api/v1/admin/organizers` | Admin | รายการ Organizer ทั้งหมด |
| `POST` | `/api/v1/admin/organizers/{id}/approve` | Admin | อนุมัติ Organizer Profile |
| `GET` | `/api/v1/admin/logs` | Admin | Audit Logs + Filter (entityType, action, date range) |
| `GET` | `/api/v1/admin/analytics` | Admin | Revenue Trend, Top Events, Geographic, Payment Methods |
| `GET` | `/api/v1/admin/system/health` | Admin | DB Pool, Redis, Hangfire, Memory, Error Rate |
| `GET` | `/api/v1/admin/config` | Admin | ดึง Platform Config ทั้งหมด |
| `PUT` | `/api/v1/admin/config/{key}` | Admin | แก้ไข Config / Feature Flag |
| `GET` | `/api/v1/admin/payouts` | Admin | รายการ Payout ที่รอดำเนินการ |
| `POST` | `/api/v1/admin/payouts/{id}/process` | Admin | อนุมัติและ Mark as Paid |

---

#### 🔗 Module: Webhooks (Server-to-Server)

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/webhooks/stripe` | Stripe (Verified by Signature) | รับ Payment Event จาก Stripe (payment_intent.succeeded, charge.refunded ฯลฯ) |

---

#### 📡 Module: SignalR Hub

| Hub URL | Method | Direction | Description |
| :--- | :--- | :--- | :--- |
| `/hubs/seats` | `JoinEvent(eventId)` | Client → Server | Subscribe ดูสถานะที่นั่ง Real-time |
| `/hubs/seats` | `LeaveEvent(eventId)` | Client → Server | Unsubscribe |
| `/hubs/seats` | `SeatStatusChanged(seatId, newStatus, version)` | Server → Client | แจ้งที่นั่งเปลี่ยนสถานะ (Hold/Available/Booked) |

---

## 4. FRONTEND DATA MODELS & ROUTE ACCESS MATRIX

### 4.1 Complete Route Matrix (All 3 Portals)

#### 🎭 Portal 1: Customer Portal (`/`)

| # | Page | Route | Auth Required | Description |
| :--- | :--- | :--- | :---: | :--- |
| 1 | Home / Landing | `/` | ❌ | Featured events, Trending, Categories, Popular artists |
| 2 | Discover / Search | `/events` | ❌ | ค้นหา + Filter + Sort |
| 3 | Event Detail | `/events/:slug` | ❌ | Poster, Line-up, Zones, Reviews, Map |
| 4 | Seat Selection | `/events/:slug/seats` | ✅ | Interactive Seat Map (SVG/Canvas), Real-time status |
| 5 | Checkout | `/checkout` | ✅ | สรุปออเดอร์, Promo Code, Payment, Countdown Timer |
| 6 | Booking Confirmation | `/bookings/:id/confirmation` | ✅ | ยืนยันสำเร็จ + QR Code + Add to Calendar |
| 7 | My Tickets | `/my-tickets` | ✅ | Tabs: Upcoming / Past / Cancelled + QR Code |
| 8 | Booking Detail | `/my-tickets/:id` | ✅ | รายละเอียดออเดอร์ + ปุ่มยกเลิก/Refund |
| 9 | Artist Profile | `/artists/:slug` | ❌ | Bio, Social, Events, Follow Button |
| 10 | My Following | `/my-following` | ✅ | ศิลปินที่ติดตาม |
| 11 | My Waitlist | `/my-waitlist` | ✅ | Event ที่กำลังรอตั๋ว |
| 12 | Notifications | `/notifications` | ✅ | รายการแจ้งเตือน |
| 13 | User Profile | `/profile` | ✅ | แก้ไขข้อมูลส่วนตัว, เปลี่ยนภาษา/สกุลเงิน |
| 14 | Login | `/auth/login` | ❌ | Email + Password, OAuth Google |
| 15 | Register | `/auth/register` | ❌ | สมัครสมาชิก |
| 16 | Forgot Password | `/auth/forgot-password` | ❌ | กรอกอีเมลขอลิงก์รีเซ็ต |
| 17 | Reset Password | `/auth/reset-password` | ❌ | ตั้งรหัสผ่านใหม่ (จากลิงก์ในอีเมล) |
| 18 | Verify Email | `/auth/verify-email` | ❌ | หน้ายืนยันอีเมล |

---

#### 📊 Portal 2: Organizer Dashboard (`/organizer`)

| # | Page | Route | Guard | Description |
| :--- | :--- | :--- | :--- | :--- |
| 1 | Dashboard Home | `/organizer` | Organizer/Admin | Overview: ยอดขายวันนี้, รายได้เดือน, Progress bars |
| 2 | My Events | `/organizer/events` | Organizer/Admin | ตาราง Event + Status filter + สร้างใหม่ |
| 3 | Create Event Wizard | `/organizer/events/new` | Organizer/Admin | Step 1-5: Info → Venue → Zones → Sale Settings → Preview |
| 4 | Edit Event | `/organizer/events/:id/edit` | Organizer (Owner) | แก้ไขข้อมูล Event |
| 5 | Seat Map Builder | `/organizer/events/:id/seats` | Organizer (Owner) | Visual tool: วาง Row, Drag Zone, Disable seats |
| 6 | Event Analytics | `/organizer/events/:id/analytics` | Organizer (Owner) | Sales Chart, Revenue, Seat Heatmap, Funnel |
| 7 | Order Management | `/organizer/events/:id/orders` | Organizer (Owner) | ตาราง Orders + Export CSV/Excel |
| 8 | Attendee List | `/organizer/events/:id/attendees` | Organizer (Owner) | รายชื่อ + Check-in status |
| 9 | Check-in Scanner | `/organizer/events/:id/checkin` | Organizer (Owner) | สแกน QR Code ผ่านกล้อง (ngx-scanner) |
| 10 | Promo Codes | `/organizer/promo-codes` | Organizer/Admin | สร้าง/จัดการ Promo Code |
| 11 | Financial Reports | `/organizer/finance` | Organizer/Admin | รายได้, ค่าคอมมิชชัน, Payout History, Download Invoice |
| 12 | Organizer Settings | `/organizer/settings` | Organizer/Admin | โปรไฟล์บริษัท, บัญชีธนาคาร, Notification Preferences |

---

#### 🔧 Portal 3: Admin Panel (`/admin`)

| # | Page | Route | Guard | Description |
| :--- | :--- | :--- | :--- | :--- |
| 1 | Admin Dashboard | `/admin` | Admin | GMV, Active Users, On Sale Events, Booking/day, Uptime, Error Rate |
| 2 | User Management | `/admin/users` | Admin | Search, Filter by Role, Ban/Unban, Promote Role |
| 3 | Event Moderation | `/admin/events` | Admin | คิว PendingApproval, Approve/Reject, Flag |
| 4 | Organizer Management | `/admin/organizers` | Admin | รายการ Organizer, Approve Profile, Commission Rate |
| 5 | Audit Log Viewer | `/admin/logs` | Admin | Who/Action/Entity + Diff View (Before/After) |
| 6 | Platform Analytics | `/admin/analytics` | Admin | Revenue Trend, Top Events, Geographic Map, Payment Pie |
| 7 | System Health | `/admin/system` | Admin | API Response Time, DB Pool, Redis Hit Rate, Hangfire Queue |
| 8 | Feature Flags | `/admin/config/features` | Admin | Toggle: maintenance_mode, enable_waitlist, enable_reviews |
| 9 | Platform Config | `/admin/config` | Admin | Default Commission, Hold Time, Supported Currencies |
| 10 | Payout Management | `/admin/payouts` | Admin | Pending Payouts, Approve/Process, Mark as Paid |

---

#### 📄 Shared Pages (ทุก Portal)

| # | Page | Route | Description |
| :--- | :--- | :--- | :--- |
| 1 | 404 Not Found | `/404` | หน้าแสดงเมื่อ URL ไม่มีอยู่ |
| 2 | Terms of Service | `/terms` | เงื่อนไขการใช้งาน |
| 3 | Privacy Policy | `/privacy` | นโยบายความเป็นส่วนตัว |

---

## 5. STATE MACHINES & CONCURRENCY RULES

### 5.1 Event State Machine

```mermaid
stateDiagram-v2
    [*] --> Draft : Organizer สร้าง Event
    Draft --> PendingApproval : Organizer กด Submit
    PendingApproval --> Published : Admin กด Approve
    PendingApproval --> Draft : Admin กด Reject + เหตุผล
    Published --> OnSale : ถึงเวลา sale_start (Hangfire Job)
    OnSale --> SoldOut : ตั๋วถูกจองครบ (total == sold)
    SoldOut --> OnSale : มีการ Cancel/Refund → ที่นั่งเปิดคืน
    OnSale --> Completed : event_end ผ่านพ้น
    OnSale --> Cancelled : ยกเลิกฉุกเฉิน (Auto-Refund)
    Completed --> [*]
    Cancelled --> [*]
```

### 5.2 Seat State Machine

```mermaid
stateDiagram-v2
    Available --> Hold : Customer เลือกที่นั่ง (TX + FOR UPDATE NOWAIT)
    Hold --> Available : หมดเวลา 10 นาที (Hangfire Worker)
    Hold --> Available : Customer กดยกเลิก
    Hold --> Booked : ชำระเงินสำเร็จ (Stripe Webhook)
    Booked --> Available : Refund (Admin/Organizer)
    Available --> Disabled : Organizer ปิดที่นั่ง (Seat Builder)
    Disabled --> Available : Organizer เปิดใช้งานที่นั่ง
```

**Race Condition Prevention:**
```sql
BEGIN TRANSACTION;
SELECT id, status, version FROM seats
  WHERE id = ANY(@seatIds) AND status = 'Available'
  FOR UPDATE NOWAIT; -- Error 54001 ถ้าถูก Lock อยู่
-- ตรวจจำนวน → ROLLBACK ถ้าไม่ครบ
UPDATE seats SET status='Hold', held_by=@userId,
  held_until=NOW()+INTERVAL '10 minutes', version=version+1
  WHERE id = ANY(@seatIds);
COMMIT;
-- → SignalR Broadcast SeatStatusChanged
```

### 5.3 Booking State Machine

```mermaid
stateDiagram-v2
    [*] --> Pending : Hold ที่นั่งสำเร็จ
    Pending --> Confirmed : Payment Succeeded (Webhook)
    Pending --> Cancelled : หมดเวลา Hold / ผู้ใช้กดยกเลิก
    Confirmed --> Refunded : อนุมัติคืนเงิน (Event Cancelled / Dispute)
    Cancelled --> [*]
    Refunded --> [*]
    Confirmed --> [*] : เข้างานสำเร็จ
```

---

## 6. CROSS-CUTTING ARCHITECTURE & INFRASTRUCTURE

### 6.1 Multi-Currency Strategy
- สกุลเงินฐานของ Event กำหนดโดย Organizer (เช่น THB, USD, JPY)
- ราคาแสดงผลตาม `user.preferred_currency` ด้วย Exchange Rate Cache (อัปเดตทุก 1 ชม. จาก API เช่น Open Exchange Rates)
- **เก็บ Transaction ในสกุลเงินฐานของ Event เสมอ** (ไม่แปลงตอนบันทึก)

### 6.2 Internationalization (i18n)
- Backend: Response messages เป็นภาษาอังกฤษ (Error codes เป็น Enum)
- Frontend: Angular `@angular/localize` + Transloco สำหรับ `th`, `en`
- Database: เก็บ Content ภาษาเดียว (ตามที่ Organizer กรอก) — Phase 2 ค่อยเพิ่ม Multi-lang content

### 6.3 Email Notification Triggers

| Trigger | Template | Recipient |
| :--- | :--- | :--- |
| User Register | Welcome + Verify Email Link | Customer |
| Forgot Password | Reset Password Link (หมดอายุ 1 ชม.) | User |
| Booking Confirmed | ตั๋ว QR Code + รายละเอียดงาน + Add to Calendar (.ics) | Customer |
| Booking Cancelled | สรุปการยกเลิก + สถานะ Refund | Customer |
| Event Reminder | เตือน 24 ชม. ก่อนงาน + ที่อยู่สถานที่ + Google Maps | Customer |
| Event Approved | แจ้ง Event ผ่านการอนุมัติ + วันเริ่มขายตั๋ว | Organizer |
| Payout Completed | แจ้งโอนเงินเข้าบัญชีสำเร็จ + ยอดเงิน | Organizer |

### 6.4 Docker Compose (Development)

| Service | Image | Port | Purpose |
| :--- | :--- | :--- | :--- |
| `postgres` | `postgres:16-alpine` | 5432 | Primary Database |
| `redis` | `redis:7-alpine` | 6379 | Cache + Session |
| `minio` | `minio/minio` | 9000 / 9001 | Object Storage (S3-compatible) |
| `seq` | `datalust/seq` | 5341 / 8081 | Structured Log Viewer (Serilog sink) |
| `maildev` | `maildev/maildev` | 1080 / 1025 | Mock SMTP (Dev email testing) |

### 6.5 Logging Strategy (Serilog)
- **Structured Logging** ด้วย Serilog → Seq (Development) / Elasticsearch (Production)
- Log Levels: `Information` สำหรับ Business Events, `Warning` สำหรับ Concurrency Conflicts, `Error` สำหรับ Unhandled Exceptions
- **Correlation ID** (`X-Correlation-Id` Header) ผูก Request → Response → Background Job สำหรับ Distributed Tracing

### 6.6 CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/ci.yml
on: [push, pull_request]
jobs:
  backend:
    - dotnet restore
    - dotnet build --no-restore
    - dotnet test --no-build      # Unit + Integration + Arch Tests
    - dotnet publish               # Build artifacts
  frontend:
    - npm ci
    - npm run lint                 # ESLint + Prettier
    - npm run test -- --watch=false  # Karma/Jest
    - npm run build -- --configuration production
```

---

*— End of Document —*
*TicketWorld Enterprise Data Dictionary & System Specification v2.0*
