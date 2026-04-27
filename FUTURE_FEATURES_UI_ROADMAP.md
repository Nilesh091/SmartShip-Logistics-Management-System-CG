# SmartShip Future Features and UI Roadmap

This document defines future-facing product features and the corresponding UI workstreams based on the current SmartShip codebase.

## 1. Current baseline summary

Implemented backend capabilities already available for UI:

- Auth with signup, login, OTP verification, token refresh/revoke
- Customer shipment creation and lifecycle actions
- Tracking timeline retrieval and status updates
- Document upload and delivery proof APIs
- Admin dashboards, shipment exception handling, user role management, reports
- Gateway-based route aggregation

Known technical constraints impacting UI planning:

- Tracking service startup can fail when RabbitMQ is unavailable in certain local setups
- Shipment service removed internal RabbitMQ coupling and now uses direct DB update paths
- OPERATOR role exists in domain context but has no dedicated UI surface yet

## 2. Roadmap principles

- Build role-first workflows before visual embellishments
- Prioritize operational clarity and low-friction task completion
- Keep all new features API-contract driven and measurable
- Introduce reusable UI primitives before adding specialized pages

## 3. Feature roadmap by horizon

## Horizon A: Near-term (0 to 2 sprints)

### A1. Customer shipment workspace hardening

Objective:

- Reduce failure rates in create/book/cancel flows and improve confidence.

UI pages/components to add:

- Customer shipment wizard with step progress and autosave draft state
- Shipment status timeline widget reused across details and list rows
- Conflict resolution banner when status transition fails

Backend dependency:

- Existing shipment and tracking APIs

Definition of done:

- Create shipment completion funnel visible in analytics
- Validation errors mapped field-by-field
- Clear status transition feedback

### A2. Admin exception operations center

Objective:

- Make shipment exception handling faster and auditable.

UI pages/components to add:

- Dedicated exception queue page with priority sorting
- Exception details side panel with action history
- Resolve confirmation modal with resolution notes

Backend dependency:

- Existing admin exception and resolve endpoints
- Optional API enhancement for resolution notes persistence

Definition of done:

- Resolve actions require reason capture in UI
- Mean time to resolve shown in dashboard cards

### A3. Delivery proof and documents UX completion

Objective:

- Operationalize POD/document endpoints into complete user flows.

UI pages/components to add:

- Unified documents and POD tabbed workspace in shipment details
- Upload queue and progress indicator
- File preview cards (image/document metadata)

Backend dependency:

- Existing tracking document and delivery proof endpoints
- Optional signed URL endpoint for secure previews/downloads

Definition of done:

- Users can upload and later retrieve proof artifacts from one page
- File type and size errors are explicit

## Horizon B: Mid-term (3 to 5 sprints)

### B1. Operator console (new role-based surface)

Objective:

- Introduce OPERATOR workflow for pickup, transit updates, and delivery confirmation.

UI pages/components to add:

- Operator task inbox page
- Shipment scan/update status panel
- Route stop checklist card

Backend dependency:

- Existing status update endpoints
- New operator assignment and task APIs required

Definition of done:

- OPERATOR role has dedicated navigation and guarded routes
- Status updates are traceable to operator identity

### B2. Real-time operational awareness

Objective:

- Improve visibility of shipment movement and critical delays.

UI pages/components to add:

- Live timeline refresh controls
- Alerts center (late shipment, failed delivery, missing POD)
- Dashboard alert severity chips and drill-down views

Backend dependency:

- Polling with current APIs as baseline
- Future push channel support (SignalR/WebSocket) recommended

Definition of done:

- Critical alerts surfaced within target SLA window
- Admin can filter alerts by severity and region

### B3. Role and access governance UX

Objective:

- Strengthen user administration and prevent permission mistakes.

UI pages/components to add:

- User role history panel
- Permission preview before role change
- Bulk role update workflow with safeguards

Backend dependency:

- Existing role update endpoint
- New audit/history endpoint for role changes recommended

Definition of done:

- Every role change action is reviewable in UI
- Risky role changes require additional confirmation step

## Horizon C: Long-term (6+ sprints)

### C1. Analytics and forecasting suite

Objective:

- Move from descriptive dashboards to predictive planning.

UI pages/components to add:

- Delivery trend forecasting dashboards
- Region-level performance heatmaps
- SLA breach prediction cards

Backend dependency:

- Existing reports endpoint as seed
- New analytics aggregation and prediction APIs required

Definition of done:

- Forecast views available for weekly and monthly planning
- Action recommendations shown with confidence labels

### C2. Customer communication center

Objective:

- Provide transparent shipment communication history.

UI pages/components to add:

- Notification timeline per shipment
- Preferred channel settings page
- Triggered message templates preview

Backend dependency:

- Notification service integration endpoints required

Definition of done:

- Customers can see what was sent, when, and why
- Admin can manage template variants and defaults

### C3. Enterprise controls and audit

Objective:

- Support compliance-heavy deployments.

UI pages/components to add:

- Full audit trail explorer
- Data retention policy settings UI
- Export and compliance report center

Backend dependency:

- New audit and compliance APIs required

Definition of done:

- Traceability for critical actions across users and roles
- Configurable retention and export workflows

## 4. Page and component segregation for future delivery

## 4.1 Shared platform components

Build once and reuse:

- App shell and role-aware navigation
- Status badge library for lifecycle states
- Data table with server-side pagination hooks
- Form validation and error mapping framework
- File upload primitives
- Timeline component
- Alert center and toast system

## 4.2 Customer module future pages

- Shipment wizard v2
- Unified shipment workspace (details, tracking, docs, POD)
- Customer notification center

## 4.3 Admin module future pages

- Exception operations center
- Reports and forecasting dashboards
- Governance and access control center

## 4.4 Operator module future pages

- Task inbox
- Field update console
- Delivery confirmation workflows

## 5. Suggested implementation sequencing

Sprint grouping recommendation:

1. Stabilize current customer + admin flows with better state handling
2. Introduce reusable shared components and role-aware shell hardening
3. Launch exception operations center and complete POD/doc experiences
4. Add operator console and real-time alerting foundations
5. Expand analytics and governance features

## 6. Success metrics to track

- Shipment creation completion rate
- Exception resolution time
- Failed status transition rate
- POD submission success rate
- Role update error rate
- Admin task completion time

## 7. Immediate next actions

- Align API payload contracts with frontend schema definitions
- Finalize role-based route map for CUSTOMER, ADMIN, OPERATOR
- Build UI component library baseline before feature-specific screens
- Start with Horizon A items in parallel tracks: customer and admin
