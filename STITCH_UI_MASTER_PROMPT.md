# Stitch UI Master Prompt for SmartShip

Copy everything below and paste into Stitch.

---

You are a senior product designer and frontend architect. Design a complete production-grade web UI for a logistics management platform called SmartShip.

## 1. Product context and boundaries

- This is an API-first microservices product with an API Gateway.
- Main user roles: CUSTOMER and ADMIN.
- OPERATOR exists in domain role enums and should be considered for future-ready navigation placeholders.
- Build a responsive web app for desktop first, then mobile/tablet adaptation.

## 2. Visual identity you must use

Use this visual style direction:

- Overall style: operational, documentation-friendly, clean enterprise UI.
- Backgrounds: very light neutral surfaces.
- Primary text: deep slate/charcoal.
- Top navigation: dark header bar.
- Semantic action colors:
  - GET/info: blue family
  - POST/success: green family
  - PUT/update: amber family
  - DELETE/error: red family
- Typography:
  - Primary: sans-serif for UI text
  - Secondary: monospace for code-like values, IDs, status logs, timeline details
- Corner radii: compact to medium (4px to 8px)
- Shadows: subtle, low elevation
- Motion: restrained 150ms to 300ms transitions

Design personality:

- Trustworthy, operational, no visual noise
- Strong information hierarchy
- Fast scanning for status-heavy content

## 3. Information architecture and route map

Generate the app with this route structure.

### Public

- /auth/login
- /auth/signup
- /auth/verify-otp

### Customer area

- /app/customer/dashboard
- /app/customer/shipments/new
- /app/customer/shipments
- /app/customer/shipments/:id
- /app/customer/shipments/:id/tracking
- /app/customer/shipments/:id/documents
- /app/customer/shipments/:id/delivery-proof
- /app/customer/profile

### Admin area

- /app/admin/dashboard
- /app/admin/shipments
- /app/admin/shipments/exceptions
- /app/admin/shipments/:id
- /app/admin/users
- /app/admin/reports
- /app/admin/settings

### Shared/system

- /unauthorized
- /not-found
- /error

## 4. API contracts and behavior to model in UI

Use these gateway route groups:

- Auth: /auth/\*
- Admin: /admin/\*
- Shipments: /shipments/\*
- Tracking: /tracking/\*

### Auth flows

1. Signup

- POST /auth/signup
- Fields: name, email, password

2. Login (2-step)

- POST /auth/login with email and password
- If valid, show OTP verification screen

3. Verify OTP

- POST /auth/verify-otp with email and otp
- Success returns tokens and role context

4. Session utilities

- POST /auth/refresh?token=...
- POST /auth/revoke?token=...

### Shipment flows

- Create shipment: POST /shipments
  - Sender: name, street, city, state, zipCode
  - Receiver: name, street, city, state, zipCode
  - Package: weight, description
- My shipments list: GET /shipments/my
- Shipment details: GET /shipments/{id}
- Book shipment: PUT /shipments/{id}/book
- Cancel shipment: DELETE /shipments/{id}

### Tracking flows

- Tracking timeline: GET /tracking/{shipmentId}
- Update status: PUT /tracking/{shipmentId}/status
  - status: DRAFT, BOOKED, PICKED_UP, IN_TRANSIT, OUT_FOR_DELIVERY, DELIVERED
  - location optional
- Upload document: POST /tracking/{shipmentId}/documents/upload (multipart)
  - documentType, file
- Get documents: GET /tracking/{shipmentId}/documents
- Submit delivery proof: POST /tracking/{shipmentId}/delivery-proof (multipart)
  - recipientName, deliveryLocation optional, signature optional, proofImage optional
- Get delivery proof: GET /tracking/{shipmentId}/delivery-proof

### Admin flows

- Dashboard: GET /admin/dashboard
- Exception shipments: GET /admin/shipments/exceptions
- All shipments: GET /admin/shipments
- Resolve shipment: PUT /admin/shipments/{id}/resolve
- Users: GET /admin/users
- Update user role: PUT /admin/users/{id}
  - role value
- Reports: GET /admin/reports
  - totalShipments, deliveredShipments, failedShipments, inTransitShipments, deliveredPercentage, trends[]

## 5. Page-by-page design requirements

For each page, provide:

- Purpose
- Primary user
- Data dependencies (API endpoints)
- Main layout regions
- Components used
- States (loading, empty, error, success)
- Validation and guardrails
- Primary actions and secondary actions

### 5.1 Login page

Components:

- Brand header
- Email and password fields
- Remember session checkbox
- Sign in button
- Link to signup

States:

- Invalid credentials inline message
- Server unavailable global alert

### 5.2 OTP verification page

Components:

- 6-digit OTP input
- Countdown timer (5 minutes)
- Resend OTP action
- Verify button

States:

- Expired OTP
- Invalid OTP
- Success redirect by role

### 5.3 Signup page

Components:

- Name, email, password fields
- Password strength hint
- Create account button

### 5.4 Customer dashboard

Components:

- KPI cards (shipment count by status)
- Recent shipment activity
- Quick actions (new shipment, track shipment)

### 5.5 Create shipment page

Components:

- Sender address form section
- Receiver address form section
- Package form section
- Live shipment summary card
- Save draft/create button

Validation:

- Required fields
- Weight positive numeric only
- Zip format checks

### 5.6 My shipments page

Components:

- Search, status filter, date filter
- Shipment table/cards
- Status badges
- Row actions: view, track, book, cancel

### 5.7 Shipment details page

Components:

- Shipment meta panel
- Sender/receiver cards
- Package card
- Status lifecycle visual
- Action toolbar based on role and status

### 5.8 Tracking timeline page

Components:

- Vertical timeline
- Status node chip
- Timestamp and location details
- Optional map placeholder region for future

### 5.9 Documents page

Components:

- Upload form (documentType + file)
- Uploaded documents list
- File metadata chips
- Download/view action placeholders

### 5.10 Delivery proof page

Components:

- Recipient name input
- Delivery location input
- Signature uploader
- Proof image uploader
- Delivery proof details display

### 5.11 Admin dashboard

Components:

- KPI row from reports
- Exception queue preview
- Shipment distribution widgets
- Trend list/chart placeholders

### 5.12 Admin shipments page

Components:

- Global shipment table
- Filters by status, date, location
- Bulk action affordances (UI only placeholder if API absent)
- Resolve action for exception rows

### 5.13 Admin users page

Components:

- User table
- Role pill
- Role change modal
- Confirm action pattern for role updates

### 5.14 Admin reports page

Components:

- Metric cards
- Delivered percentage visual
- Trends panel
- Export placeholder

## 6. Global component system

Create reusable components and map where each is used:

- App shell with role-aware navigation
- Topbar with user menu and token/session state
- Side navigation with grouped sections
- Page header with breadcrumbs and action slot
- Data table (sorting, filtering, pagination-ready)
- Status badge set for shipment lifecycle
- Modal, drawer, toast, confirm dialog
- Form primitives with error/help text
- Empty state, error state, skeleton loaders
- File upload with size validation messaging

## 7. Role-based experience rules

- CUSTOMER sees only personal shipment records and customer routes.
- ADMIN sees platform-wide shipments, exceptions, users, and reports.
- Unauthorized route attempts redirect to /unauthorized.
- Hide non-permitted actions in UI and also show disabled rationale where useful.

## 8. UX quality requirements

- Accessibility: keyboard focus order, visible focus states, semantic labels, contrast-compliant text.
- Responsiveness:
  - Desktop: data-dense tables and split panels
  - Tablet: compressed table with expandable rows
  - Mobile: card stacks and bottom action bars
- Performance-minded behavior:
  - Lazy-load heavy admin pages
  - Optimistic updates only where rollback strategy exists

## 9. Output format you must return

Return all of the following in order:

1. High-level sitemap and navigation model
2. Page inventory matrix (page, purpose, role, APIs, key components)
3. Component inventory matrix (component, variants, props, states)
4. Detailed page specs for every route in section 3
5. Interaction flows for:
   - Signup to login to OTP verification
   - Create shipment to booking
   - Admin exception resolution
   - Delivery proof submission
6. Design tokens summary (color, type, spacing, radius, shadows, motion)
7. Implementation checklist with phases:
   - Phase 1 MVP
   - Phase 2 hardening
   - Phase 3 advanced features

## 10. Constraints and guardrails

- Do not produce generic placeholders like Dashboard Widget 1.
- Use concrete logistics-domain naming for cards, actions, and sections.
- Every page must include clear empty, loading, and error behavior.
- Keep components reusable and avoid one-off UI patterns unless justified.
- Prefer clarity and operational efficiency over decorative complexity.

Now generate the complete UI specification.

---

End of prompt.
