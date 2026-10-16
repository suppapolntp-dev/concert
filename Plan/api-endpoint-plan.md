# 📡 API Endpoint Contract Plan

> **Scope:** Complete REST API Catalog — All Modules, Request/Response Contracts, Headers & Rate Limits  
> **Base URL:** `https://api.ticketworld.io/api/v1`  
> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Section 3)

---

## 1. Global API Standards

### 1.1 Required Headers (ทุก Request)

| Header | Required | Description |
| :--- | :---: | :--- |
| `Authorization` | Conditional | `Bearer {accessToken}` — ต้องมีสำหรับ Authenticated endpoints |
| `Content-Type` | Yes | `application/json` |
| `Accept-Language` | Optional | `th`, `en` — กำหนดภาษา error message |
| `X-Correlation-Id` | Auto | Frontend สร้าง UUID ต่อ request → Backend propagate ลง log ทั้ง chain |
| `X-Idempotency-Key` | Conditional | UUID — บังคับสำหรับ financial mutation endpoints |
| `X-WaitingRoom-Token` | Conditional | JWT bypass token — บังคับเมื่อ event เปิด Waiting Room |
| `X-CSRF-Token` | Conditional | Double-submit cookie — บังคับสำหรับ mutating requests |

### 1.2 Rate Limits

| Endpoint Group | Limit | Window | Scope |
| :--- | :--- | :--- | :--- |
| Public Search (`GET /events`) | 120 req | 1 min | Per IP |
| Seat Hold (`POST /seats/hold`) | 5 req | 1 min | Per User |
| Payment (`POST /bookings/*/checkout`) | 3 req | 1 min | Per User |
| Auth Login (`POST /auth/login`) | 10 req | 5 min | Per IP |
| File Upload (`POST /files/*`) | 10 req | 1 min | Per User |
| Waiting Room Heartbeat (`GET /queue/position`) | 30 req | 1 min | Per User |
| Check-in Scan (`POST /*/checkin`) | 60 req | 1 min | Per Staff |
| OTP Verify (`POST /auth/2fa/verify`) | 5 req | 5 min | Per User |

---

## 2. Module: Authentication & Identity (12 Endpoints)

| # | Method | Endpoint | Auth | Idempotent | Description |
| :---: | :--- | :--- | :--- | :---: | :--- |
| 1 | `POST` | `/auth/register` | Public | ✅ (email) | สมัครสมาชิก + ส่ง Verification Email |
| 2 | `POST` | `/auth/login` | Public | — | Login → Access Token + Refresh Token (cookie) |
| 3 | `POST` | `/auth/refresh-token` | Public (cookie) | — | ขอ Access Token ใหม่ (Token Rotation) |
| 4 | `POST` | `/auth/logout` | Authenticated | — | Revoke Refresh Token ปัจจุบัน |
| 5 | `POST` | `/auth/logout-all` | Authenticated | — | Revoke ทุก Token ทุกอุปกรณ์ |
| 6 | `POST` | `/auth/verify-email` | Public | ✅ | ยืนยันอีเมล (`?token=xxx`) |
| 7 | `POST` | `/auth/forgot-password` | Public | ✅ | ขอลิงก์รีเซ็ตรหัสผ่าน (หมดอายุ 1 ชม.) |
| 8 | `POST` | `/auth/reset-password` | Public | ✅ | ตั้งรหัสผ่านใหม่ด้วย Reset Token |
| 9 | `POST` | `/auth/change-password` | Authenticated | — | เปลี่ยนรหัสผ่าน (ต้องกรอกรหัสเดิม) |
| 10 | `GET` | `/auth/me` | Authenticated | — | ดึง Profile ปัจจุบัน |
| 11 | `PUT` | `/auth/me` | Authenticated | — | แก้ไข Profile (ชื่อ, โทร, ภาษา, สกุลเงิน) |
| 12 | `DELETE` | `/auth/me` | Authenticated | — | ขอลบบัญชี (PDPA Anonymize) |

### 2FA Sub-module (3 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 13 | `POST` | `/auth/2fa/setup` | Authenticated | เริ่ม TOTP → คืน QR + Secret |
| 14 | `POST` | `/auth/2fa/verify` | Authenticated | ยืนยัน OTP code เปิดใช้ / Step-up auth |
| 15 | `POST` | `/auth/2fa/disable` | Authenticated + Password | ปิด 2FA |

### OAuth Sub-module (2 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 16 | `GET` | `/auth/oauth/google/login` | Public | เริ่ม OAuth flow (PKCE) |
| 17 | `GET` | `/auth/oauth/google/callback` | Public | รับ code → link/สร้างบัญชี |

---

## 3. Module: Events — Customer (5 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/events` | Public | ค้นหา + Filter (q, category, city, date, price) + Pagination |
| 2 | `GET` | `/events/featured` | Public | Event ที่ถูก Feature หน้าแรก |
| 3 | `GET` | `/events/trending` | Public | Event ยอดนิยม (ยอดขาย 7 วัน) |
| 4 | `GET` | `/events/{slug}` | Public | รายละเอียด Event + Line-up + ราคาแต่ละโซน |
| 5 | `GET` | `/events/{slug}/seats` | Public | ผังที่นั่ง Real-time status |

---

## 4. Module: Seats & Booking — Customer (10 Endpoints)

| # | Method | Endpoint | Auth | Idempotent | Description |
| :---: | :--- | :--- | :--- | :---: | :--- |
| 1 | `POST` | `/seats/hold` | Customer | ✅ Key | Hold ที่นั่ง 10 นาที (ต้องมี WaitingRoom Pass ถ้าเปิดคิว) |
| 2 | `DELETE` | `/seats/hold` | Customer (Owner) | — | ปล่อยที่นั่ง Hold ก่อนหมดเวลา (`seatIds` ใน request body) |
| 3 | `POST` | `/bookings/{id}/checkout` | Customer (Owner) | ✅ Key | ชำระเงิน (บังคับ `X-Idempotency-Key`) |
| 4 | `GET` | `/bookings/{id}` | Customer (Owner) | — | รายละเอียดออเดอร์ |
| 5 | `POST` | `/bookings/{id}/cancel` | Customer (Owner) | — | ขอยกเลิก (ตาม Refund Policy) |
| 6 | `GET` | `/my-tickets` | Customer | — | ตั๋วทั้งหมด (upcoming/past/cancelled) |
| 7 | `GET` | `/my-tickets/{id}/qr` | Customer (Owner) | — | ดาวน์โหลด QR Code (PDF) |
| 8 | `POST` | `/my-tickets/{id}/transfer` | Customer (Owner) | ✅ Key | โอนตั๋ว → ผู้รับยืนยันภายใน 24 ชม. |
| 9 | `POST` | `/my-tickets/{id}/transfer/accept` | Customer (Recipient) | ✅ | ยืนยันรับตั๋วที่โอน |
| 10 | `POST` | `/promo-codes/validate` | Customer | — | ตรวจสอบโค้ดส่วนลดก่อน Checkout |

---

## 5. Module: Virtual Waiting Room (3 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `POST` | `/queue/enter` | Public | เข้าคิว → คืน `queue_token` (signed + position) |
| 2 | `GET` | `/queue/position` | Public (token) | ดูลำดับคิว + estimated wait (heartbeat 3s) |
| 3 | `POST` | `/queue/claim-pass` | Public (token) | ถึงคิว → แลก `seatmap_pass` JWT (5 นาที) |

---

## 6. Module: Artists (5 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/artists` | Public | ค้นหาศิลปิน (genre, country) |
| 2 | `GET` | `/artists/{slug}` | Public | Profile + Upcoming Events |
| 3 | `POST` | `/artists/{slug}/follow` | Customer | กดติดตาม (Idempotent ON CONFLICT DO NOTHING) |
| 4 | `DELETE` | `/artists/{slug}/follow` | Customer | เลิกติดตาม |
| 5 | `GET` | `/my-following` | Customer | รายชื่อศิลปินที่ติดตาม |

---

## 7. Module: Reviews (4 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/events/{slug}/reviews` | Public | รีวิวทั้งหมด + avg rating |
| 2 | `POST` | `/events/{slug}/reviews` | Customer (Attended) | เขียนรีวิว (ต้องมี Confirmed Booking + event จบแล้ว) |
| 3 | `PUT` | `/reviews/{id}` | Customer (Owner) | แก้ไขรีวิวตนเอง |
| 4 | `DELETE` | `/reviews/{id}` | Owner / Admin | ลบรีวิว |

---

## 8. Module: Waitlist (3 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `POST` | `/events/{slug}/waitlist` | Customer | ลงทะเบียนรอตั๋ว (เมื่อ SoldOut) |
| 2 | `DELETE` | `/events/{slug}/waitlist` | Customer | ยกเลิกรอ |
| 3 | `GET` | `/my-waitlist` | Customer | รายการ Event ที่กำลังรอ |

---

## 9. Module: Notifications (4 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/notifications` | Authenticated | รายการแจ้งเตือน (Paginated) |
| 2 | `GET` | `/notifications/unread-count` | Authenticated | จำนวนที่ยังไม่อ่าน (Badge) |
| 3 | `PUT` | `/notifications/{id}/read` | Authenticated | Mark as Read |
| 4 | `PUT` | `/notifications/read-all` | Authenticated | Mark All as Read |

---

## 10. Module: File Upload (2 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `POST` | `/files/presigned-url` | Organizer / Admin | ขอ Presigned URL สำหรับ Upload ไป MinIO/S3 |
| 2 | `POST` | `/files/confirm` | Organizer / Admin | ยืนยัน Upload เสร็จ → บันทึก URL ลง DB |

---

## 11. Module: Organizer Dashboard (12 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/organizer/dashboard` | Organizer | Overview stats |
| 2 | `GET` | `/organizer/events` | Organizer | รายการ Event ของตน + Filter by status |
| 3 | `POST` | `/organizer/events` | Organizer | สร้าง Event ใหม่ (Draft) |
| 4 | `PUT` | `/organizer/events/{id}` | Organizer (Owner) | แก้ไข Event |
| 5 | `POST` | `/organizer/events/{id}/submit` | Organizer (Owner) | ส่ง Event เข้าคิวอนุมัติ |
| 6 | `PUT` | `/organizer/events/{id}/seats` | Organizer (Owner) | บันทึกผังที่นั่ง (Seat Map Builder) |
| 7 | `GET` | `/organizer/events/{id}/analytics` | Organizer (Owner) | Sales Chart, Revenue, Heatmap |
| 8 | `GET` | `/organizer/events/{id}/orders` | Organizer (Owner) | รายการ Orders + Export CSV |
| 9 | `GET` | `/organizer/events/{id}/attendees` | Organizer (Owner) | รายชื่อ + Check-in status |
| 10 | `POST` | `/organizer/events/{id}/checkin` | Organizer/Staff | สแกน QR Code ตรวจตั๋ว |
| 11 | `GET` | `/organizer/finance` | Organizer | รายได้, Commission, Payout History |
| 12 | `POST` | `/organizer/finance/payout-request` | Organizer | ขอยื่นเบิกเงิน |

### Staff Management Sub-module (3 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 13 | `GET` | `/organizer/staff` | Organizer | รายชื่อ Staff |
| 14 | `POST` | `/organizer/staff/invite` | Organizer | เชิญพนักงาน + กำหนดสิทธิ์ |
| 15 | `DELETE` | `/organizer/staff/{id}` | Organizer | เพิกถอนการเข้าถึง |

---

## 12. Module: Platform Admin (12 Endpoints)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/admin/dashboard` | Admin | GMV, Active Users, Uptime, Error Rate |
| 2 | `GET` | `/admin/users` | Admin | Search, Filter by Role |
| 3 | `PUT` | `/admin/users/{id}/ban` | Admin (Step-up) | Ban / Unban User |
| 4 | `PUT` | `/admin/users/{id}/role` | Admin (Step-up) | Promote / Demote Role |
| 5 | `GET` | `/admin/events` | Admin | คิว PendingApproval |
| 6 | `POST` | `/admin/events/{id}/approve` | Admin | Approve Event |
| 7 | `POST` | `/admin/events/{id}/reject` | Admin | Reject Event + เหตุผล |
| 8 | `GET` | `/admin/audit-logs` | Admin | Audit Trail (Who/When/What + Diff) |
| 9 | `GET` | `/admin/payouts` | Admin | Pending Payouts |
| 10 | `POST` | `/admin/payouts/{id}/process` | Admin (Step-up) | อนุมัติ + โอนเงิน |
| 11 | `GET` | `/admin/system/health` | Admin | APM, DB Pool, Redis Hit, Queues |
| 12 | `PUT` | `/admin/config/{key}` | Admin (Step-up) | แก้ไข Feature Flags / System Config |

---

## 13. Webhook Receiver (1 Endpoint)

| # | Method | Endpoint | Auth | Description |
| :---: | :--- | :--- | :--- | :--- |
| 1 | `POST` | `/webhooks/stripe` | Signature Verify | รับ Payment Webhook (Idempotent dedupe by `event_id`) |

---

## Summary: Total Endpoints = **78**

| Module | Count |
| :--- | :---: |
| Auth & Identity | 17 |
| Events (Customer) | 5 |
| Seats & Booking | 10 |
| Waiting Room | 3 |
| Artists | 5 |
| Reviews | 4 |
| Waitlist | 3 |
| Notifications | 4 |
| File Upload | 2 |
| Organizer Dashboard + Staff | 15 |
| Platform Admin | 12 |
| Webhook Receiver | 1 |
| **Total** | **78** |
