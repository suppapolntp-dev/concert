# Git Execution Log & Command History

บันทึกชุดคำสั่งทั้งหมดที่รันจริงในกระบวนการสร้าง branch, แก้ไขปัญหา Auth Headless, refactor component, และ push/pull บน GitHub repository `suppapolntp-dev/concert`

---

## 1. การสร้าง Branch และ Commit ครั้งแรก (Initial Implementation)

```bash
# 1.1 สลับและสร้าง branch ใหม่ชื่อ 'card'
git checkout -b card

# 1.2 Stage ไฟล์ที่แก้ไข
git add frontend/src/app/pages/organizer/organizer.component.ts

# 1.3 Commit การ์ด 4 เลเยอร์ตาม Freud v2 Specification
git commit -m "feat(organizer): implement Freud v2 mindful metrics cards on dashboard

- Apply 4-layer card anatomy (Header, Hero Stat & Trend, Data Viz, AI Insights Footer)
- Integrate SVG curved line sparkline with gradient fill and rounded pill bars
- Add time range filter pills (Realtime, Today, Tour Total)
- Use calming tonal palette (emerald/sage, indigo, amber) with 24px squircle borders"
```

---

## 2. การแก้ปัญหา Git Credential Manager (GCM) บน Headless Terminal & Push

### ปัญหาที่พบ (Root Cause):
เมื่อรัน `git push -u origin card` ตรงๆ ใน session ของ AI/Headless process คำสั่งจะค้าง (Timeout 180s) เนื่องจาก `Git Credential Manager` บน Windows พยายามเปิดหน้าต่าง GUI Browser Login เพื่อขอยืนยันตัวตน แต่ไม่มี Desktop TTY แสดงผล

### วิธีแก้ปัญหาผ่าน GitHub OAuth Device Flow (No-TTY Standard):
```bash
# 2.1 ร้องขอ Device Code จาก GitHub OAuth API (ใช้ Client ID ทางการของ GitHub CLI)
curl -s -X POST -H "Accept: application/json" \
  -d "client_id=178c6fc778ccc68e1d6a&scope=repo" \
  https://github.com/login/device/code

# ผลลัพธ์ที่ได้:
# user_code: "DC98-6414"
# verification_uri: "https://github.com/login/device"
```

```bash
# 2.2 รัน Background Polling Script ดักจับสัญญาณ handshake เมื่อ User กด Authorize ที่เบราว์เซอร์
# Script: poll_github_push.py
# ทันทีที่ได้ access_token:
git remote set-url origin https://x-access-token:<TOKEN>@github.com/suppapolntp-dev/concert.git

# 2.3 ยิง push ขึ้น origin ทันที
git push -u origin card
```

---

## 3. การ Refactor แยก Component (Clean Architecture)

เมื่อแยกโค้ดที่ซ้ำซ้อนใน `organizer.component.ts` ออกมาเป็น `MetricCardComponent`:

```bash
# 3.1 ตรวจสอบสถานะไฟล์
git status

# 3.2 ทดสอบ Build เพื่อการันตีว่า TypeScript และ Angular ผ่านฉลุย 100%
npm run build --prefix frontend

# 3.3 Stage ทั้งตัว component กลางและหน้าที่เรียกใช้
git add frontend/src/app/pages/organizer/organizer.component.ts \
        frontend/src/app/shared/components/metric-card/metric-card.component.ts

# 3.4 Commit การ refactor
git commit -m "refactor(organizer): extract reusable MetricCardComponent and eliminate duplication

- Create standalone MetricCardComponent under shared/components/metric-card
- Define MetricCardData interface with dynamic theming and SVG chart types
- Refactor organizer dashboard page to iterate over metrics via @for loop
- Clean up architecture and reduce boilerplate in organizer dashboard"

# 3.5 Push commit ใหม่ขึ้น branch card บน GitHub
git push origin card
```

---

## 4. การ Sync โค้ดกลับเข้า Main หลัง Merge PR

```bash
# 4.1 สลับกลับมาที่ branch main
git checkout main

# 4.2 Pull การเปลี่ยนแปลงล่าสุดที่ถูก merge บน GitHub กลับเข้าเครื่อง
git pull origin main

# 4.3 ตรวจสอบ log และความสะอาดของ working tree
git log -3 --oneline
git status
```
