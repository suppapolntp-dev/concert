# 🔐 Security & Compliance Plan

> **Scope:** Secrets Management, PDPA/PCI-DSS Compliance, Incident Response, Fraud Detection  
> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Section 7)

---

## 1. Secrets & Key Management

### 1.1 Secret Storage Strategy

| Secret | Storage | Rotation Cycle | Zero-Downtime Mechanism |
| :--- | :--- | :--- | :--- |
| Stripe / Omise API Keys | Vault / AWS Secrets Manager | 90 วัน | Stripe รองรับ 2 active keys — rotate ทีละตัว |
| JWT Signing Keys (RS256) | KMS + JWKS endpoint (`/auth/jwks.json`) | 6 เดือน | Key ID ใน JWT header — key เก่า valid อีก 24 ชม. |
| HMAC Key (QR Ticket) | KMS | รายปี | Versioned prefix (`v1:`, `v2:`) — ตรวจตั๋วเก่าด้วย key version ที่ออก |
| AES Key (Bank Account) | KMS (Envelope Encryption) | รายปี + ทันทีเมื่อสงสัยรั่ว | Re-encrypt ข้อมูลเดิมด้วย key ใหม่ batch job |
| DB / Redis / MinIO Credentials | Vault + Sidecar Injection | 90 วัน | Sidecar refresh ไม่ต้อง restart pod |

### 1.2 Secret Prevention Rules
- ❌ ห้าม secret ใน Git → บังคับ pre-commit hook (`gitleaks`)
- ❌ ห้าม log ค่า secret → Serilog Destructuring Policy mask `*Password*`, `*Secret*`, `*Token*`
- ❌ ห้ามเก็บ secret ใน `appsettings.json` หรือ `.env` บน production
- ✅ Dev/Local ใช้ `.env` + Docker Secrets ได้
- ✅ Production ใช้ Vault / Cloud KMS เท่านั้น

---

## 2. PDPA (Thailand) Compliance

### 2.1 Lawful Basis & Data Inventory

| ข้อมูลที่เก็บ | ฐานกฎหมาย | Retention |
| :--- | :--- | :--- |
| ชื่อ, อีเมล, โทรศัพท์ | สัญญา (ขายตั๋ว) | จนกว่า user ขอลบ |
| ประวัติการจอง, ใบเสร็จ | กฎหมายบัญชี | 7 ปี |
| Audit Logs | Legitimate Interest (Fraud) | 2 ปี |
| Email Logs | Legitimate Interest | 1 ปี |
| Marketing Preferences | Consent (Opt-in) | จนกว่า user ถอน consent |

### 2.2 Data Subject Rights Implementation

| สิทธิ์ | Endpoint | กลไก |
| :--- | :--- | :--- |
| **Right to Access** | `GET /api/v1/auth/me` | คืน profile + booking history |
| **Right to Portability** | `POST /api/v1/auth/me/export` | สร้าง JSON export → ส่งอีเมลลิงก์ดาวน์โหลด (24 ชม.) |
| **Right to Erasure** | `DELETE /api/v1/auth/me` | Soft-delete + Anonymize (ดู 2.3) |
| **Right to Rectification** | `PUT /api/v1/auth/me` | แก้ไขข้อมูลส่วนตัว |
| **Withdraw Consent** | `PUT /api/v1/auth/me/consent` | ถอน marketing consent → หยุดส่งอีเมลโปรโมชัน |

### 2.3 Anonymization Procedure (Erasure Flow)
```
User ขอลบบัญชี (DELETE /auth/me)
         │
         ▼
    ยังมี Booking status=Pending?
    ┌──YES──┐──NO──┐
    ▼              ▼
 Cancel ก่อน    ดำเนินการ Anonymize
                   │
    ┌──────────────┼──────────────────────────┐
    ▼              ▼                          ▼
 users:         bookings:                  อื่นๆ:
 - email →      - ชื่อ/อีเมลใน             - Revoke ทุก token
   anon+{uuid}    snapshot →               - Cancel waitlist
   @deleted       pseudonymize             - Unfollow ทั้งหมด
   .invalid     - FK user_id คงไว้          - Reviews แสดงเป็น
 - full_name →    (integrity บัญชี)           "ผู้ใช้ที่ลบบัญชี"
   [anonymized]                             - บันทึก audit_logs
 - phone → NULL                              Users.Anonymize
 - avatar → NULL
 - google_sub → NULL
 - deleted_at = NOW()
```

---

## 3. PCI-DSS Scope Minimization

| ข้อกำหนด | การปฏิบัติ |
| :--- | :--- |
| **SAQ A เท่านั้น** | ไม่มี Card Data แตะ Server — ใช้ Stripe Elements / Hosted Fields บน Frontend |
| **3DS / SCA** | เปิดทุก Transaction — บันทึกผลใน `payments.three_ds_status` |
| **Payload Sanitization** | `payments.raw_response` ผ่าน whitelist filter — ตัด `card.*`, last4, network ก่อนบันทึก |
| **Fraud Monitoring** | Stripe Radar + Internal velocity rules |
| **Annual Pentest** | ทดสอบ penetration ประจำปี + ระบุ scope เครื่องที่แตะ token |

---

## 4. Incident Response & Severity Matrix

### 4.1 Severity Levels

| Sev | ตัวอย่าง | Response Time | Resolution |
| :--- | :--- | :--- | :--- |
| **Sev1 (Critical)** | DB รั่ว, Payment double-charge หลายราย, ระบบล่มช่วง on-sale | Page on-call ทันที | War room ภายใน 15 นาที |
| **Sev2 (High)** | Token reuse detection, Webhook ค้าง, Rate limit bypass | ตอบภายใน 1 ชม. | แก้ภายใน 4 ชม. |
| **Sev3 (Medium)** | Dependency CVE ปานกลาง, ช่องโหว่ไม่มี exploit | บันทึกรายสัปดาห์ | แก้ใน sprint ถัดไป |

### 4.2 Fraud Detection Signals (Auto-flag + Audit)
- Hold-success rate สูง + checkout rate ต่ำ → bot ยึดที่นั่ง
- Email domain ใหม่ register จำนวนมากพร้อม artist follower spike
- 1 device fingerprint สั่งซื้อหลายบัญชี
- Velocity > X orders/นาที ระหว่าง hot on-sale
- Webhook จาก source ที่ไม่ผ่าน signature verification

### 4.3 Runbook Scenarios
- **Payment Gateway ล่มช่วง on-sale:** เปิด maintenance mode เฉพาะ checkout route + ขยาย hold อัตโนมัติ
- **Leaked Key:** Revoke ที่ KMS ทันที + ตรวจ audit_logs 30 วัน
- **Data Breach:** แจ้ง PDPC ภายใน 72 ชม. + แจ้ง subject ที่กระทบ

---

## 5. Backup & Disaster Recovery

| เป้าหมาย | ค่า |
| :--- | :--- |
| **RPO** (เสียข้อมูลได้สูงสุด) | 5 นาที (WAL archiving + PITR) |
| **RTO** (กลับมาให้บริการภายใน) | 1 ชั่วโมง |
| **Full Backup** | รายวัน (เข้ารหัส AES-256) เก็บ cross-region |
| **Restore Drill** | ทดสอบ restore ทุก 3 เดือน (บันทึกผลใน audit) |
| **Object Storage** | Versioning + Replicated bucket |
