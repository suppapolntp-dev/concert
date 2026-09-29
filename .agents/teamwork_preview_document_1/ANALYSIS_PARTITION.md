# ANALYSIS_PARTITION

input_format: latex
text_map_path: c:\Users\wwwwww\Documents\project-test\concert\.agents\teamwork_preview_document_1\DOCUMENT_TEXT_MAP.md
total_sections: 15

## Segments

### Segment 1: data_model_and_api_contract
- **name**: data_model_and_api_contract
- **category**: Modeling
- **section_ranges**: ["Plan/00_ROADMAP_READING_ORDER.md", "Plan/01_concert_requirement_spec.md#Data-Dictionary", "Plan/02_master_project_plan.md", "Plan/05_oop_design_plan.md", "Plan/06_entity_db_plan.md", "Plan/07_api_endpoint_plan.md"]
- **context_instruction**: Execute a rigorous cross-plan structural and data model consistency audit addressing Requirement R1 and acceptance criteria. Compare 05_oop_design_plan.md vs 06_entity_db_plan.md vs 07_api_endpoint_plan.md vs 01_concert_requirement_spec.md. Verify all 24 entities, enums (17 enums), value objects (8 VOs), field types (e.g., decimal vs double, UUID vs string, enum vs integer), naming conventions (PascalCase vs camelCase vs snake_case), nullability, foreign key constraints, and indices. Audit all 78 endpoint contracts in 07 against requirements in 01. Check for missing endpoints, missing request/response DTO fields, and zero unaddressed conflicts between DB entities and API DTO contracts.

### Segment 2: concurrency_realtime_and_financial
- **name**: concurrency_realtime_and_financial
- **category**: Theory / Math
- **section_ranges**: ["Plan/01_concert_requirement_spec.md#Booking-Rules", "Plan/03_docker_infra_plan.md#Redis", "Plan/06_entity_db_plan.md#Concurrency-xmin", "Plan/08_realtime_concurrency_plan.md", "Plan/09_payment_reconciliation_plan.md"]
- **context_instruction**: Execute an exhaustive mathematical and technical audit of high-concurrency workflows, real-time sync, and financial integrity addressing Requirement R2 and acceptance criteria. Scrutinize the Virtual Waiting Room queue flow (Redis Sorted Sets ZADD/ZRANK, token issuance, heartbeat expiration, admission throttle). Stress-audit the microsecond seat locking with RedLock (TTL 3s, clock drift, auto-extension, release script) vs PostgreSQL transactions (SELECT FOR UPDATE NOWAIT / EF Core xmin shadow property). Rigorously analyze the 3 payment webhook race scenarios (clean win, race lost auto-refund, 90s grace window), Hangfire soft-extension job, outbox pattern, payment idempotency keys, and webhook replay protection.

### Segment 3: security_compliance_and_infrastructure
- **name**: security_compliance_and_infrastructure
- **category**: Methodology
- **section_ranges**: ["Plan/03_docker_infra_plan.md#Secrets-Environment", "Plan/04_backend_guidelines.md", "Plan/07_api_endpoint_plan.md#Auth-Error-Envelopes", "Plan/12_security_compliance_plan.md", "Plan/13_testing_qa_plan.md#Security-Tests"]
- **context_instruction**: Benchmark the specifications against industry standards and statutory security compliance addressing Requirement R3. Audit API Error envelopes against RFC 7807 (Problem Details - type, title, status, detail, instance, extensions). Audit authentication, JWT/refresh tokens, and authorization against OWASP API Security Top 10 (BOLA, broken function level auth, mass assignment) and NIST SP 800-63B (AAL2, password complexity, throttling, credential stuffing). Verify financial and privacy compliance against PCI-DSS SAQ A (hosted tokenization, Omise/Stripe iframe isolation, zero PAN in backend/logs) and Thailand PDPA (explicit consent, data subject rights, crypto-shredding / anonymization, retention). Audit Docker compose secret management in 03.

### Segment 4: frontend_uxui_qa_and_roadmap
- **name**: frontend_uxui_qa_and_roadmap
- **category**: Experiments
- **section_ranges**: ["Plan/01_concert_requirement_spec.md#UX-Flows", "Plan/02_master_project_plan.md#Milestones", "Plan/10_uxui_design_system_plan.md", "Plan/11_frontend_architecture_plan.md", "Plan/13_testing_qa_plan.md", "Plan/14_sprint_roadmap_plan.md"]
- **context_instruction**: Audit frontend architecture, UX/UI design system, QA testing strategy, and sprint delivery roadmap for holistic coherence. Audit Angular 18 architecture (Signals, standalone components, 3 portals) against SignalR real-time seat map updates (SVG rendering performance under 10k seats, WebRTC QR scanning). Verify 13_testing_qa_plan.md testing pyramid, k6 load test specifications (500 VUs, SLA < 200ms at 95th percentile, booking spike) against concurrency SLAs in 08/09. Audit sprint roadmap in 14 (6 phases, 12 sprints) for missing dependencies, sequencing flaws, and unallocated technical debt.
