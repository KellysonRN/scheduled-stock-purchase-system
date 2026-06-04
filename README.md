# Technical Challenge - Scheduled Stock Purchase System

## [Source the challenge](https://github.com/gcamarao/teste_itau_v2/blob/main/desafio-tecnico-compra-programada.md)

## Overview

This project implements a **scheduled stock purchase system** where clients make monthly contributions and the system periodically executes consolidated stock purchases.

All client contributions are aggregated, assets are purchased through a **master account**, and then distributed proportionally among investors.

The implementation follows these principles:

- **Vertical Slice Architecture** - Features organized by business use cases
- **Modular Monolith** - High cohesion, low coupling, ready to evolve to microservices
- **Domain Driven Design (DDD-lite)** - Strong domain modeling with business rules
- **Event-Driven Integration** - Asynchronous communication via Apache Kafka
- **Clean Architecture** - Clear separation between Domain, Infrastructure, and Features

The goal is to maintain **high feature cohesion**, low coupling, and enable future evolution toward a distributed architecture.

---

## Business Requirements

### Scheduled Purchase

- Each client defines a **monthly investment amount**
- System executes purchases **3 times per month**: Days 5, 15, 25
- Each execution invests: `monthlyContribution / 3`

### Recommended Portfolio (Top Five)

| Asset | Allocation |
|-------|-----------|
| PETR4 | 30% |
| VALE3 | 25% |
| ITUB4 | 20% |
| BBDC4 | 15% |
| WEGE3 | 10% |

### Consolidated Purchase

1. Aggregate all client contributions
2. Calculate shares for each asset
3. Execute purchase using **master account**
4. Prioritize: Standard lot (100 shares) → Fractional market

### Asset Distribution

1. Distribute assets proportionally (based on client contribution)
2. Update each client's holdings
3. Remaining shares stay in master account

### Custody Model

- **Master Account** - Executes all stock purchases
- **Client Account** - Stores assets owned by each client

### Financial Calculations

**Average Price:**

```js
    AveragePrice = (PreviousQty * PreviousPrice + NewQty * PurchasePrice) / TotalQty
```

**Taxes:**

- Withholding Tax (IR): 0.005% over sale value
- Capital Gain Tax: 20% over profit (if monthly sales > R$20,000)

---

## Architecture

The project was recently refactored to a Vertical Slice organization. See the full architecture specification in [docs/architecture-spec.md](docs/architecture-spec.md).


## Technology Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 10 |
| **API** | Minimal API (Built-in) |
| **Database** | MySQL + EF Core |
| **Messaging** | Apache Kafka |
| **Testing** | xUnit |
| **Architecture** | Vertical Slice Architecture |
| **Design Pattern** | Domain Driven Design (DDD-lite) |
| **Containerization** | Docker & Docker Compose |

---

## Implementation Plan - Vertical Slice Approach

> **Strategy:** Each phase delivers a **complete feature vertical slice** (API endpoint → handler → domain logic → persistence → events). Features are independent and deliver value incrementally.

---

### **PHASE 0 - Foundation & Infrastructure** ✅

**Setup Checkpoints:**

- ✅ 0.1: Project structure, solution setup, NuGet dependencies
- ✅ 0.2: Docker + Docker Compose (MySQL, Kafka, tooling)
- ✅ 0.3: EF Core, migrations, database schema
- ✅ 0.4: CI/CD pipeline, code coverage, SonarQube integration
- ✅ 0.5: Shared libraries (Result, Error, Domain Events interfaces)

**Deliverables:** Project runs, tests pass, infrastructure ready

---

### **PHASE 1 - Client Registration (Vertical Slice)** ✅

**Feature:** Client self-registration with account creation and portfolio assignment.

**Vertical Slice Components:**

- **Endpoint:** `POST /clients` (Register client)
- **Handler:** SubscribeClientHandler
- **Domain:** Client aggregate, Account entity, Portfolio value object
- **Persistence:** Client repository, migrations
- **Events:** `ClientSubscribed` event (published to Kafka)

**Checkpoints:**

- ✅ 1.1: Client value objects (CPF, Email, Name validation)
- ✅ 1.2: Account creation (Master account for system, Client account per investor)
- ✅ 1.3: Portfolio assignment (Top 5 portfolio linked to client)
- ✅ 1.4: Event publishing (ClientSubscribed → Kafka → DLT on failure)
- ✅ 1.5: Integration tests (Happy path, validation failures, duplicates)

**Key Requirements:**

- CPF unique constraint
- Email validation
- Atomic transaction (Client + Accounts created together)
- Idempotent by CPF
- Comprehensive error handling

**Deliverables:** Clients can register, accounts created, event stream ready

---

### **PHASE 2 - Contribution Management (Vertical Slice)** ✅

**Feature:** Client defines monthly contribution amount and scheduler triggers purchases 3x/month.

**Vertical Slice Components:**

- **Endpoint:** `POST /clients/{clientId}/contributions` (Set monthly contribution)
- **Handler:** RegisterContributionHandler
- **Domain:** Contribution aggregate, ContributionAmount value object
- **Scheduler:** Cron jobs (days 5, 15, 25 at 10:00 AM)
- **Events:** `ContributionScheduled`, `PurchaseTriggered` events

**Checkpoints:**

- ✅ 2.1: Contribution entity (Amount, frequency, validation)
- ✅ 2.2: Contribution repository (Persistence layer)
- ✅ 2.3: Scheduler implementation (Days 5, 15, 25 trigger)
- ✅ 2.4: Idempotency (Prevent duplicate purchases same day)
- ✅ 2.5: Integration tests (Schedule accuracy, edge cases)

**Key Requirements:**

- Minimum contribution: R$ 500/month
- Scheduler reliable and testable
- Idempotent by (clientId, executionDate)
- Event-driven trigger to next phase

**Deliverables:** Contributions tracked, scheduler functional, purchase pipeline triggered

---

### **PHASE 3 - Purchase Order Generation (Vertical Slice)** ⚠️ CRITICAL

**Feature:** Generate purchase plan from aggregated contributions with asset allocation.

**Vertical Slice Components:**

- **Handler:** GeneratePurchasePlanHandler (triggered by `PurchaseTriggered` event)
- **Domain:** PurchaseOrder aggregate, Asset quantities calculation
- **Service:** Asset price lookup, portfolio allocation math
- **Persistence:** PurchaseOrder repository, execution history
- **Events:** `PurchasePlanGenerated`, `PurchaseExecuted` events

**Checkpoints:**

- ✅ 3.1: Aggregate contributions (Sum all client contributions for the day)
- ✅ 3.2: Calculate quantities (Apply 30/25/20/15/10% allocation to assets)
- ✅ 3.3: Lot prioritization (Standard lot 100 shares → fractional)
- ✅ 3.4: PurchaseOrder creation (Status transitions: Pending → Executed → Distributed)
- ✅ 3.5: Event publishing (PurchasePlanGenerated → Kafka)

**Key Requirements:**

- ✅ Financial accuracy (Zero loss, all R$ accounted for)
- ✅ Asset math validated (100% of contribution allocated)
- ✅ Idempotent by execution date
- ✅ Comprehensive unit + integration tests

**Deliverables:** Purchase orders generated accurately, ready for execution

---

### **PHASE 4 - Master Account Execution (Vertical Slice)** 🔴 HIGH RISK

**Feature:** Execute purchase on master account, update holdings, recalculate average price.

**Vertical Slice Components:**

- **Handler:** ExecutePurchaseHandler (triggered by `PurchasePlanGenerated` event)
- **Domain:** Holding aggregate, average price calculation formula
- **Service:** Master account debit/credit logic
- **Persistence:** Holding repository, transaction history
- **Events:** `PurchaseExecuted` event (with state snapshot)

**Checkpoints:**

- ✅ 4.1: Master account debit (Deduct cash from master account)
- ✅ 4.2: Holding update (Add shares to master account holdings)
- ✅ 4.3: Average price recalc (Formula: `(PrevQty * PrevPrice + NewQty * NewPrice) / TotalQty`)
- ✅ 4.4: Transaction atomicity (All-or-nothing with rollback)
- ✅ 4.5: Idempotency (Re-execution safe, detect duplicates by OrderId)
- ✅ 4.6: Event publishing + DLT (Failure recovery)

**Key Requirements:**

- 🔴 Transaction integrity (financial consistency non-negotiable)
- 🔴 State snapshots (before/after for audit)
- 🔴 Kafka retry + DLT on failure
- 100% unit test coverage for financial calculations

**Deliverables:** Master account updated safely, stocks purchased, audit trail recorded

---

### **PHASE 5 - Asset Distribution (Vertical Slice)** 🔴 HIGH RISK

**Feature:** Distribute purchased assets proportionally to clients based on contribution.

**Vertical Slice Components:**

- **Handler:** DistributeAssetsHandler (triggered by `PurchaseExecuted` event)
- **Domain:** Distribution logic, proportional allocation calculation
- **Service:** Per-client holding allocation, remainder handling
- **Persistence:** Client holdings repository, distribution records
- **Events:** `AssetsDistributed` event

**Checkpoints:**

- ✅ 5.1: Proportional calculation (Each client gets: `(ClientContribution / TotalContribution) * PurchasedShares`)
- ✅ 5.2: Remainder handling (Fractional shares stay in master account, documented)
- ✅ 5.3: Client holding updates (Atomic per-client holding creation/update)
- ✅ 5.4: Average price inheritance (Client holdings get same avg price as master)
- ✅ 5.5: Reconciliation (Verify: sum of all client holdings = master holdings - remainder)

**Key Requirements:**

- 🔴 100% accuracy (No loss, no creation of assets)
- 🔴 Remainder strategy documented and auditable
- Atomic updates per client (no partial distributions)
- Comprehensive reconciliation tests

**Deliverables:** Assets distributed correctly, client holdings updated, audit trail complete

---

### **PHASE 6 - Tax Calculation (Vertical Slice)**

**Feature:** Calculate and track taxes (IR + Capital Gains).

**Vertical Slice Components:**

- **Handler:** CalculateTaxHandler (triggered by `AssetsDistributed` event, or on-demand)
- **Domain:** Tax calculation formulas, TaxLiability aggregate
- **Service:** Period aggregation, sales threshold detection (> R$20k)
- **Persistence:** Tax records, liability tracking
- **Events:** `TaxCalculated` event

**Checkpoints:**

- ✅ 6.1: Withholding Tax (IR) calculation (0.005% over sale value)
- ✅ 6.2: Capital Gain detection (20% if monthly sales > R$20k)
- ✅ 6.3: Period aggregation (Monthly tax liability report)
- ✅ 6.4: Debtor tracking (Link taxes to clients)
- ✅ 6.5: Event publishing (Tax calculated → audit log)

**Key Requirements:**

- Tax formula accuracy (verified against regulatory standards)
- Monthly period aggregation
- Client liability tracking
- Audit trail for tax authority compliance

**Deliverables:** Taxes calculated and tracked, reports ready

---

### **PHASE 7 - Portfolio Rebalancing (Vertical Slice)**

**Feature:** Detect allocation drift and trigger rebalancing trades.

**Vertical Slice Components:**

- **Endpoint:** `POST /rebalance` (Manual trigger or scheduled)
- **Handler:** RebalancePortfolioHandler
- **Domain:** Rebalancing logic, trade generation
- **Service:** Deviation detection, trade execution
- **Persistence:** Rebalancing history, trade records
- **Events:** `RebalancingTriggered`, `TradesExecuted` events

**Checkpoints:**

- ✅ 7.1: Deviation detection (Current allocation vs target, threshold > 5%)
- ✅ 7.2: Trade generation (Sell/buy orders to rebalance)
- ✅ 7.3: Trade execution (Update master + client holdings)
- ✅ 7.4: History tracking (Rebalancing history with KPIs)
- ✅ 7.5: Monitoring dashboard (Allocation history charts)

**Key Requirements:**

- Automatic detection on threshold breach
- Manual trigger available
- Atomic trade execution
- Performance KPIs tracked

**Deliverables:** Rebalancing automated, portfolio allocation maintained

---

### **PHASE 8 - Integration & E2E Testing**

**Feature:** Complete end-to-end scenarios and performance validation.

**Checkpoints:**

- ✅ 8.1: Happy path (Register → Contribute → Purchase → Distribute → Rebalance)
- ✅ 8.2: Error scenarios (Insufficient funds, validation errors, Kafka failures)
- ✅ 8.3: Concurrency tests (Simultaneous contributions, race conditions)
- ✅ 8.4: Load testing (100 clients, 3 purchases/month, Kafka lag < 5s)
- ✅ 8.5: Disaster recovery (Database failure, Kafka failure, scheduler crash recovery)

**Key Requirements:**

- 5+ complete e2e flows
- Load test results documented
- Recovery procedures validated
- Performance SLAs met

**Deliverables:** System production-ready, tested, documented

---

## 📊 Implementation Status & Progress

> **Current Progress: ~30% Complete** | Last Update: June 4, 2026

### ✅ What's Complete

#### Phase 0: Foundation (100%)
- Domain model: Client & Trade entities
- Value Objects: Cpf, Money, Quantity, Ticker, TradeId, ContributionAmount (all with business rules)
- Result pattern with error handling
- Vertical Slice Architecture setup
- Entity base class
- Abstractions: IHandler, IApiEndpoint

#### Phase 1: API & Middleware (75%)
- ✅ **Single Feature:** POST `/trades` (CreateTrade vertical slice - complete)
- ✅ Rate Limiting Middleware (2 req/10s per user per endpoint)
- ✅ Service collection auto-registration (handlers + endpoints via reflection)
- ✅ OpenAPI + Scalar documentation
- ✅ Exception handling middleware
- ❌ Database persistence layer

#### Phase 2: Trade Management (25%)
- ✅ CreateTrade endpoint
- ❌ GetTrade, ListTrades, UpdateTrade, DeleteTrade
- ❌ Trade repository

### ⚠️ Testing Status

| Project | Tests | Coverage |
|---------|-------|----------|
| **Scheduled.Stock.Purchase.Domain.Tests** | ✅ 7/7 test classes | All entities & value objects |
| **Scheduled.Stock.Purchase.Api.Tests** | ✅ 2/3 suites | Rate limiting complete; CreateTrade tests missing |
| **Scheduled.Stock.Purchase.Shared.Tests** | ✅ 1/1 | Result pattern validated |

### ❌ What's Missing (Next Priorities)

| Phase | Feature | Status | Needed |
|-------|---------|--------|--------|
| **2-4** | **Database & Persistence** | ❌ | Infrastructure project, EF Core, DbContext, Migrations, Repositories |
| **2** | **Trade CRUD** | ⚠️ | GetTrade, ListTrades, UpdateTrade, DeleteTrade endpoints |
| **1** | **Client Management** | ❌ | Create, Get, List, Update, Delete client features |
| **2** | **Contributions** | ❌ | RecordContribution, GetContribution endpoints |
| **3** | **Scheduler** | ❌ | Background job for days 5, 15, 25 |
| **3-4** | **Purchase Logic** | ❌ | GeneratePurchasePlan, ExecutePurchase handlers |
| **5** | **Asset Distribution** | ❌ | DistributeAssets handler with proportional allocation |
| **6** | **Tax Calculation** | ❌ | CalculateTax handler (IR + Capital Gains) |
| **7** | **Rebalancing** | ❌ | Portfolio rebalancing logic |
| **5** | **Kafka Integration** | ❌ | Events, Producers, Consumers, Topics |

### 🚀 Next Steps - Aligned with Vertical Slice Plan

> Follow this exact sequence to maintain feature independence and incremental delivery

---

#### **PHASE 1.5: Trade Persistence Foundation** (Week 1) 🔴 BLOCKING

Before implementing any new features, complete Trade CRUD with database:

**Vertical Slice: Complete Trade Management**

1. **Create Infrastructure Project**
   ```
   src/Scheduled.Stock.Purchase.Infrastructure/
   ├─ Data/ScheduledStockPurchaseDbContext.cs
   ├─ Data/Migrations/
   └─ Repositories/
   ```

2. **Setup EF Core DbContext**
   - Map Trade entity (TradeId, Ticker, Quantity, Money as owned types)
   - Map Client entity (Cpf as owned type)
   - DbSet<Trade>, DbSet<Client>

3. **Create Initial Migration**
   - `dotnet ef migrations add Initial`
   - Include Trades and Clients tables

4. **Implement Repositories**
   - `ITradeRepository`, `TradeRepository` (CRUD operations)
   - `IClientRepository`, `ClientRepository` (placeholder for Phase 2)
   - Register in DI: `services.AddInfrastructure()`

5. **Complete Trade CRUD Endpoints**
   - ✅ POST `/trades` (already exists - update to use repository)
   - ➕ GET `/trades/{id}` - GetTradeEndpoint/Handler
   - ➕ GET `/trades` (paged) - ListTradesEndpoint/Handler
   - ➕ PUT `/trades/{id}` - UpdateTradeEndpoint/Handler
   - ➕ DELETE `/trades/{id}` - DeleteTradeEndpoint/Handler

6. **Integration Tests**
   - `CreateTradeEndpointTests` (happy path + validation)
   - `GetTradeEndpointTests`
   - `ListTradesEndpointTests` (pagination)
   - `UpdateTradeEndpointTests`
   - `DeleteTradeEndpointTests`

**Definition of Done:**
- All Trade operations persist to MySQL ✅
- Rate limiting still works ✅
- All tests pass (domain + integration) ✅
- Docker Compose: `docker-compose up` → ready to use ✅

---

#### **PHASE 2: Client Registration Vertical Slice** (Week 2-3) 🔴 CRITICAL

**Feature:** Register client with account creation and portfolio assignment

**Implementation Steps:**

1. **Add Client Domain Model**
   - Client entity: FullName, Email, Cpf, CreatedAt
   - Account entity: AccountType (Master/Client), Balance, Holdings
   - Portfolio entity: Link Top 5 assets (PETR4, VALE3, ITUB4, BBDC4, WEGE3)
   - Domain events: `ClientSubscribed` (not publishing yet, just define interface)

2. **Update DbContext**
   - DbSet<Client>, DbSet<Account>, DbSet<Portfolio>
   - Add constraints: CPF unique, Email unique

3. **Create Migration**
   - `dotnet ef migrations add AddClientManagement`

4. **Implement Repositories**
   - `IClientRepository`, `ClientRepository`
   - Query: `GetByIdAsync(clientId)`, `GetByCpfAsync(cpf)`, `ListAsync()`
   - Commands: `AddAsync(client)`, `UpdateAsync(client)`, `DeleteAsync(id)`

5. **Create Vertical Slice: `Features/Clients/CreateClient/`**
   - `CreateClientEndpoint.cs` - POST `/clients`
   - `CreateClientHandler.cs` - Business logic
   - `CreateClientRequest.cs`, `CreateClientResponse.cs`
   - `CreateClientValidator.cs` - Fluent validation

6. **Additional Client Endpoints**
   - GET `/clients/{id}` - GetClientEndpoint
   - GET `/clients` (paged) - ListClientsEndpoint
   - PUT `/clients/{id}` - UpdateClientEndpoint
   - DELETE `/clients/{id}` - DeleteClientEndpoint

7. **Atomic Account Creation**
   - When Client created → Automatically create Master Account (shared) + Client Account
   - Link Portfolio (Top 5) to Client

8. **Integration Tests**
   - `CreateClientEndpointTests` - Happy path, validation, duplicate CPF
   - `GetClientEndpointTests`
   - `ListClientsEndpointTests` (pagination)
   - `UpdateClientEndpointTests`
   - `DeleteClientEndpointTests`

**Definition of Done:**
- Clients register with valid CPF/Email ✅
- Accounts created atomically (Master + Client) ✅
- Portfolio linked automatically ✅
- CPF unique constraint enforced ✅
- All tests pass ✅

---

#### **PHASE 3: Contribution Management Vertical Slice** (Week 4) 🔴 CRITICAL

**Feature:** Record monthly contributions and trigger purchase scheduler

**Implementation Steps:**

1. **Add Domain Model**
   - Contribution entity: ClientId, Amount, ContributionDate, Status
   - Domain event: `ContributionRecorded` (interface defined)

2. **Update DbContext**
   - DbSet<Contribution>
   - Index: (ClientId, ContributionDate) for fast aggregation

3. **Create Migration**
   - `dotnet ef migrations add AddContributions`

4. **Implement Repository**
   - `IContributionRepository`, `ContributionRepository`
   - Key method: `GetAggregatedForExecutionDateAsync(executionDate)` → List<(ClientId, TotalAmount)>

5. **Create Vertical Slice: `Features/Contributions/RecordContribution/`**
   - `RecordContributionEndpoint.cs` - POST `/clients/{clientId}/contributions`
   - `RecordContributionHandler.cs`
   - `RecordContributionRequest.cs`, `RecordContributionResponse.cs`
   - Minimum: R$ 500/month validation

6. **Setup Scheduler (Background Job)**
   - Create: `Features/Scheduler/ScheduledPurchaseJob.cs`
   - Trigger dates: 5th, 15th, 25th of month at 10:00 AM
   - Calls: `GetAggregatedForExecutionDateAsync()` → aggregates all contributions for the day
   - Publishes: `PurchaseTriggered` event (Kafka integration in Phase 4)

7. **Idempotency Strategy**
   - Track executed dates in ExecutionLog table
   - Prevent double-execution same day

8. **Integration Tests**
   - `RecordContributionEndpointTests` - Happy path, validation, minimum amount
   - `ScheduledPurchaseJobTests` - Correct aggregation, idempotency
   - Verify scheduler triggers on correct dates

**Definition of Done:**
- Clients can record monthly contributions ✅
- Scheduler triggers 3x/month automatically ✅
- Aggregation works correctly ✅
- Idempotent execution ✅
- All tests pass ✅

---

#### **PHASE 4: Purchase Order Generation** (Week 5) ⚠️ CRITICAL

**Feature:** Generate purchase plan from aggregated contributions

**Implementation Steps:**

1. **Add Domain Model**
   - PurchaseOrder aggregate: OrderId, OrderDate, TotalAmount, Status (Pending→Executed→Distributed)
   - PurchaseOrderLine: Asset, TargetQuantity, Price, Status
   - Domain event: `PurchasePlanGenerated`

2. **Update DbContext**
   - DbSet<PurchaseOrder>, DbSet<PurchaseOrderLine>

3. **Create Migration**
   - `dotnet ef migrations add AddPurchaseOrders`

4. **Implement Repository**
   - `IPurchaseOrderRepository`, `PurchaseOrderRepository`

5. **Create Event Handler: `Features/Purchases/GeneratePurchasePlan/`**
   - Triggered by `PurchaseTriggered` event (from Phase 3 scheduler)
   - Handler: `GeneratePurchasePlanHandler`
   - Service: `PurchaseOrderCalculationService`
     - Aggregate contributions per execution date
     - Calculate asset quantities: `(TotalAmount * AllocationPercentage)`
     - Asset allocation: PETR4 30%, VALE3 25%, ITUB4 20%, BBDC4 15%, WEGE3 10%
     - Lot prioritization: Standard (100 shares) → Fractional
   - Creates PurchaseOrder with status: Pending

6. **Asset Price Service** (Placeholder for now)
   - Interface: `IAssetPriceService.GetCurrentPriceAsync(ticker)`
   - Will be implemented in Phase 5+ with real market data
   - For now: Return fixed prices for testing

7. **Unit Tests**
   - `PurchaseOrderCalculationServiceTests` - Verify asset quantities, allocations, lot math
   - Test: Total allocation = 100%, No loss/creation of assets
   - Test: Fractional remainder handled correctly

**Definition of Done:**
- Contributions aggregated correctly ✅
- Purchase order created with accurate quantities ✅
- Asset allocation validated (100% = TotalAmount) ✅
- All calculations tested ✅

---

#### **PHASE 5: Master Account Execution** (Week 6) 🔴 HIGH RISK

**Feature:** Execute purchase on master account, update holdings

**Implementation Steps:**

1. **Add Domain Model**
   - Holding aggregate: AccountId, Ticker, Quantity, AveragePrice, LastUpdatedAt
   - Transaction record for audit trail
   - Domain event: `PurchaseExecuted` (with before/after snapshot)

2. **Update DbContext**
   - DbSet<Holding>
   - DbSet<TransactionHistory> for audit

3. **Create Migration**
   - `dotnet ef migrations add AddHoldings`

4. **Implement Repository**
   - `IHoldingRepository`, `HoldingRepository`
   - Methods: `GetOrCreateAsync(accountId, ticker)`, `UpdateAsync(holding)`

5. **Create Event Handler: `Features/Purchases/ExecutePurchase/`**
   - Triggered by `PurchasePlanGenerated` event
   - Handler: `ExecutePurchaseHandler`
   - Service: `PurchaseExecutionService`
     - **CRITICAL:** Use DbTransaction for atomicity
     - Debit master account cash
     - For each asset in PurchaseOrder:
       - Get or create Holding
       - Update quantity and average price
       - Formula: `AvgPrice = (PrevQty * PrevPrice + NewQty * NewPrice) / TotalQty`
     - Create TransactionHistory record (before/after snapshot)
     - Idempotency: Detect by OrderId (prevent re-execution)

6. **Error Handling & Rollback**
   - Transaction rollback on any failure
   - Log error with full context (for DLT in Phase 6)

7. **Comprehensive Tests**
   - `PurchaseExecutionServiceTests` - Average price formula, atomicity
   - Test idempotency: Execute twice → same result
   - Test edge cases: First purchase (no previous holdings), fractional quantities

**Definition of Done:**
- Master account updated safely ✅
- Stocks purchased with correct average price ✅
- Transactions atomic (all-or-nothing) ✅
- Audit trail recorded ✅
- Idempotent execution ✅

---

#### **PHASE 6: Asset Distribution** (Week 7) 🔴 HIGH RISK

**Feature:** Distribute purchased assets proportionally to clients

**Implementation Steps:**

1. **Create Event Handler: `Features/Distribution/DistributeAssets/`**
   - Triggered by `PurchaseExecuted` event
   - Handler: `DistributeAssetsHandler`
   - Service: `AssetDistributionService`
     - Get PurchaseOrder and Client contributions
     - For each client:
       - Calculate share: `(ClientContribution / TotalContribution) * PurchasedShares`
       - Create or update Holding for client
       - Inherit average price from master account
     - Remainder: Fractional shares stay in master account (documented)

2. **Reconciliation Logic**
   - After distribution, verify: Sum(all client holdings) + master remainder = master holdings
   - Test: No assets lost/created

3. **Distribution Records**
   - Create DistributionRecord for audit trail
   - Track: Who got what, when, how much

4. **Comprehensive Tests**
   - `AssetDistributionServiceTests` - Proportional calculation accuracy
   - Test: 100 shares ÷ 3 clients = 33, 33, 34 (remainder handled)
   - Test: Reconciliation passes
   - Test: Average price inherited correctly

**Definition of Done:**
- Assets distributed proportionally ✅
- Remainder strategy documented and working ✅
- Reconciliation tests pass ✅
- All clients get correct holdings ✅

---

#### **PHASE 7: Kafka Integration** (Week 8) 🔴 CRITICAL

**Feature:** Event-driven communication between features

**Implementation Steps:**

1. **Setup Kafka Producers**
   - `IEventPublisher` interface
   - Publish events: ClientSubscribed, ContributionRecorded, PurchaseTriggered, PurchasePlanGenerated, PurchaseExecuted, AssetsDistributed
   - Topic naming: `scheduled-stock-purchase.{EventName}`

2. **Setup Kafka Consumers**
   - Consumer for each event
   - Process events asynchronously

3. **Dead Letter Topic (DLT)**
   - Failed events → DLT for retry/investigation
   - Monitoring: Alert on DLT messages

4. **Update Event Handlers**
   - Each handler now publishes to Kafka
   - Retry policy: Exponential backoff
   - Idempotency by event ID

5. **Docker Compose**
   - Add Kafka broker + Zookeeper
   - Ensure `docker-compose up` starts everything

6. **Integration Tests**
   - Mock Kafka for unit tests
   - Integration tests with real Kafka container

**Definition of Done:**
- Events flow through Kafka ✅
- Failed events sent to DLT ✅
- Consumers process events reliably ✅
- No message loss ✅

---

#### **PHASE 8: Integration & E2E Testing** (Week 9)

**Features:** Complete scenarios, load testing, disaster recovery

1. **E2E Test Scenarios**
   - Happy path: Register → Contribute → Purchase → Distribute
   - Error scenarios: Validation failures, insufficient funds
   - Concurrency: Simultaneous contributions

2. **Load Testing**
   - 100 clients, 3 purchases/month
   - Kafka lag < 5s
   - Database performance validated

3. **Disaster Recovery**
   - Database failure handling
   - Kafka failure handling
   - Scheduler crash recovery

**Definition of Done:**
- 5+ complete E2E scenarios ✅
- Load tests documented ✅
- Recovery procedures validated ✅
- System production-ready ✅

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker & Docker Compose
- Git

### Setup

```bash
# Clone repository
git clone https://github.com/KellysonRN/scheduled-stock-purchase-system.git
cd scheduled-stock-purchase-system

# Create .env file
cp .env.example .env

# Update .env with your credentials
# MYSQL_ROOT_PASSWORD=your_password_here
# MYSQL_DATABASE=your_database_here
# MYSQL_USER=your_username_here
# MYSQL_PASSWORD=your_password_here

# Start infrastructure (MySQL, Kafka)
docker-compose up -d

# Restore NuGet packages
dotnet restore

# Run migrations
dotnet ef database update

# Run application
dotnet run --project src/Scheduled.Stock.Purchase.Api

```

### Testing

```bash

# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverageMetrics=true

# Run specific test class
dotnet test --filter "ClassName=MyClass"

```

### Deployment

```bash
# Build Docker image
docker build -t scheduled-stock-purchase:latest .

# Push to registry
docker push scheduled-stock-purchase:latest

# Deploy with Docker Compose
docker-compose -f docker-compose.prod.yml up -d
```