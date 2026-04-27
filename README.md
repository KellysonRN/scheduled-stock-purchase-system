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

### Solution Structure

```js
src/
    │
    ├── Scheduled.Stock.Purchase.Api
    │   └── Program.cs # API setup, middleware, route registration
    │
    ├── Scheduled.Stock.Purchase.Domain/ # Business logic & rules 
        │ 
        ├── Entities/ # Client, Account, Holding, Portfolio, Trade 
        │ 
        ├── ValueObjects/ # Money, Ticker, Quantity, CPF 
        │ 
        └── Services/ # Domain Services (calculations, logic) 
    │ 
    ├── Scheduled.Stock.Purchase.Infrastructure/ # Technical implementation 
        │ 
        ├── Database/ # EF Core DbContext, Migrations 
        │ 
        ├── Repositories/ # Data access layer 
        │ 
        ├── Kafka/ # Event publishing 
        │ 
        ├── Scheduler/ # Background jobs (days 5, 15, 25) 
        │ 
        └── Parsers/ # CotahistParser for market data 
    │ 
    ├── Scheduled.Stock.Purchase.Features/ # Vertical Slices (Business use cases) 
        │ 
        ├── Clients/  
            │ 
            └── SubscribeClient/ # Register client, set contribution, create account 
        │ 
        ├── Contributions/  
            │ 
            └── ProcessContribution/ # Load clients, calculate contribution, generate plan 
        │ 
        ├── Purchases/ 
            │ 
            └── ExecutePurchase/ # Calculate quantities, create orders, update master 
        │ 
        ├── Distribution/  
            │ 
            └── DistributeAssets/ # Distribute proportionally, update holdings 
        │ 
        ├── Portfolio/ 
            │  
            └── RebalancePortfolio/ # Compare allocation, generate trades 
        │ 
        └── Taxes/ 
            │ 
            └── CalculateTaxes/ # Calculate IR and capital gain taxes 
  i 
```

### Vertical Slice Pattern

Each feature follows this structure:

```js
FeatureName/ 
├── Command.cs # Input DTO (Command pattern) 
├── Handler.cs # Use case handler (orchestration) 
├── Validator.cs # Validation rules 
├── Endpoint.cs # Minimal API endpoint 
└── Tests.cs # Unit & integration tests
```

### Domain Events (Event-Driven Decoupling)

- ContributionProcessed → Triggered after contribution processed
- PurchaseExecuted → After stocks purchased (master account)
- AssetsDistributed → After distribution to clients
- TaxCalculated → After tax calculation

### Scheduler Processing Flow

Day 5, 15, 25 (3x per month):

1. ProcessContribution → Load active clients, calculate contributions
2. ExecutePurchase → Aggregate funds, buy stocks (master account)
3. DistributeAssets → Distribute stocks proportionally to clients
4. CalculateTaxes → Calculate IR and capital gain taxes
5. Publish Events → Send to Kafka for external systems

---

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

## Implementation Plan

### **PHASE 0 - Foundation & Setup**

**Checkpoints:**

- ✅ 0.1: Infrastructure (Solution, folder structure, dependencies)
- ✅ 0.2: Database (MySQL + Docker, EF Core setup, migrations)
- ✅ 0.3: Messaging (Kafka, Producer/Consumer, serialization, Dead Letter Topic)
- ✅ 0.4: CI/CD (GitHub Actions, Code Coverage, SonarQube, Docker build)

**Deliverables:** Project skeleton, Docker Compose, first passing build

---

### **PHASE 1 - Domain Model Core** (🔴 CRITICAL)

**Checkpoints:**

- ✅ 1.1: Value Objects (Money, Ticker, Quantity, CPF, ContributionAmount)
- ✅ 1.2: Domain Entities (Client, Account, Portfolio, Trade, Holding, Contribution)
- ✅ 1.3: Financial Calculations (Avg Price, Proportional Distribution, Tax Formulas)
- ✅ 1.4: Domain Events (ContributionProcessed, PurchaseExecuted, AssetsDistributed, TaxCalculated)

**Key Requirements:**

- 100% test coverage for all calculations
- Business rule validation in Value Objects
- Event publishing interface
- Proper aggregates and root entities

**Deliverables:** Domain layer complete, all formulas validated

---

### **PHASE 2 - Client Management**

**Checkpoints:**

- ✅ 2.1: SubscribeClient Slice (Command, Handler, Validator, Endpoint)
- ✅ 2.2: Client Persistence (Repository, Migrations, Queries)
- ✅ 2.3: Account Creation (Master + Client accounts, relationships)

**Key Requirements:**

- CPF unique validation
- Email validation
- Account creation atomic

**Deliverables:** Clients can register, accounts created

---

### **PHASE 3 - Portfolio & Market Data**

**Checkpoints:**

- ✅ 3.1: Portfolio Entity (5 assets, allocation percentages, validation)
- ✅ 3.2: CotahistParser (Parse files, extract prices, store history)
- ✅ 3.3: Asset Price Service (Interface, implementation, caching)

**Key Requirements:**

- Portfolio validation: sum of % = 100%
- Historical price tracking
- Current price cache (daily expiration)

**Deliverables:** Portfolio configured, prices available

---

### **PHASE 4 - Contribution Pipeline**

**Checkpoints:**

- ✅ 4.1: Contribution Entity (Amount, validation, business rules)
- ✅ 4.2: Purchase Plan Generation (Calculate asset quantities, prioritize lots)
- ✅ 4.3: Scheduler Integration (Cron jobs on days 5, 15, 25, idempotency)

**Key Requirements:**

- Idempotent processing (avoid duplicates)
- Purchase plan accuracy
- Scheduler reliability

**Deliverables:** Contributions processed, purchase plans generated

---

### **PHASE 5 - Purchase Execution** (🔴 CRITICAL)

**Checkpoints:**

- ✅ 5.1: Purchase Order Creation (Order entity, status transitions)
- ✅ 5.2: Master Account Updates (Debit cash, credit holdings, avg price recalculation)
- ✅ 5.3: Event Publishing (PurchaseExecuted event, retry policy, DLT)
- ✅ 5.4: Rollback & Idempotency (Detect re-execution, transaction integrity)

**Key Requirements:**

- Financial transaction integrity
- State snapshots (before/after)
- Idempotent by purchase order ID
- Kafka retry + DLT on failure

**Deliverables:** Stocks purchased safely, events published

---

### **PHASE 6 - Asset Distribution** (🔴 HIGH RISK)

**Checkpoints:**

- ✅ 6.1: Proportional Distribution Logic (Calculate per-client allocation, handle remainders)
- ✅ 6.2: Client Holdings Update (Atomic updates, average price recalculation)
- ✅ 6.3: Event Publishing (AssetsDistributed event, confirmation)

**Key Requirements:**

- Distribution 100% accurate (no loss)
- Remainder handling documented
- Each client gets correct amount

**Deliverables:** Assets distributed correctly, holdings updated

---

### **PHASE 7 - Tax Calculation**

**Checkpoints:**

- ✅ 7.1: Tax Engine (Calculate IR 0.005%, Capital Gain 20% if sales > R$20k)
- ✅ 7.2: Tax Liability Tracking (Aggregate by period, debtor tracking)
- ✅ 7.3: Event Publishing (tax-calculated event)

**Key Requirements:**

- Tax calculations accurate
- Period aggregation
- Integration ready

**Deliverables:** Taxes calculated, events published

---

### **PHASE 8 - Portfolio Rebalancing**

**Checkpoints:**

- ✅ 8.1: Rebalancing Logic (Compare current vs target, generate trades)
- ✅ 8.2: Rebalancing Execution (Execute sell/buy, update holdings)
- ✅ 8.3: Monitoring (Track history, KPIs)

**Key Requirements:**

- Trigger: Allocation deviation > 5%
- Trade execution atomic
- Reporting ready

**Deliverables:** Rebalancing operational

---

### **PHASE 9 - Integration & E2E Testing**

**Checkpoints:**

- ✅ 9.1: Feature Flow Tests (End-to-end: Subscribe → Contribute → Purchase → Distribute)
- ✅ 9.2: Load Testing (100 clients, 3x/month scheduler, Kafka lag < 5s)
- ✅ 9.3: Disaster Recovery (BD failure, Kafka failure, scheduler failure scenarios)

**Key Requirements:**

- 5+ complete e2e scenarios
- Load testing results documented
- Recovery procedures documented

**Deliverables:** System tested, performance validated

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