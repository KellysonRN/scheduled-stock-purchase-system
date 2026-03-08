# Technical Challenge - Scheduled Stock Purchase System

## Source the challenge [here](https://github.com/gcamarao/teste_itau_v2/blob/main/desafio-tecnico-compra-programada.md).

## Overview

This project implements a **scheduled stock purchase system** where
clients make monthly contributions and the system periodically executes
consolidated stock purchases.

All client contributions are aggregated, assets are purchased through a
**master account**, and then distributed proportionally among investors.

The implementation follows these principles:

-   Vertical Slice Architecture
-   Modular Monolith
-   Domain Driven Design (DDD-lite)
-   Event-Driven Integration
-   Apache Kafka for event publishing

The goal is to maintain **high feature cohesion**, low coupling, and
enable future evolution toward a distributed architecture.

------------------------------------------------------------------------

# Functional Requirements

## Scheduled Purchase

Each client defines a **monthly investment amount**.

The system executes purchases **three times per month**:

-   Day 5
-   Day 15
-   Day 25

Each execution invests:

    monthlyContribution / 3

------------------------------------------------------------------------

## Recommended Portfolio

Purchases follow a **Top Five portfolio**, composed of five assets with
predefined allocation percentages.

Example:

  Asset   Allocation
  ------- ------------
  PETR4   30%
  VALE3   25%
  ITUB4   20%
  BBDC4   15%
  WEGE3   10%

------------------------------------------------------------------------

## Consolidated Purchase

The system must:

1.  Aggregate all client contributions
2.  Calculate how many shares to purchase for each asset
3.  Execute the purchase using the **master account**

Purchases should prioritize:

-   standard lot (100 shares)
-   fractional market when necessary

------------------------------------------------------------------------

## Asset Distribution

After the consolidated purchase:

1.  Assets are distributed proportionally to each client
2.  Allocation is based on each client's contribution
3.  Remaining shares that cannot be distributed remain in the master
    account

------------------------------------------------------------------------

## Custody Model

There are two custody levels.

### Master Account

Responsible for executing all stock purchases.

### Client Account

Stores the assets owned by each client.

------------------------------------------------------------------------

## Average Price Calculation

After each purchase the system recalculates the **average acquisition
price**.

Formula:

    AveragePrice = (PreviousQty * PreviousPrice + NewQty * PurchasePrice) / TotalQty

------------------------------------------------------------------------

## Taxes

### Withholding Tax (IR Dedo-Duro)

    0.005% over sale value

### Monthly Capital Gain Tax

If monthly sales exceed **R\$20,000**:

    20% over the profit

Tax events must be **published to Kafka**.

------------------------------------------------------------------------

# Non‑Functional Requirements

## Architecture

The system must use:

-   Vertical Slice Architecture
-   Modular Monolith
-   Clear separation between Domain and Infrastructure

------------------------------------------------------------------------

## Messaging

Important events must be published using **Apache Kafka**.

Examples:

-   purchase-executed
-   assets-distributed
-   tax-calculated

------------------------------------------------------------------------

## Asynchronous Processing

Domain events allow integration with:

-   tax reporting systems
-   auditing platforms
-   monitoring systems
-   analytics pipelines

------------------------------------------------------------------------

# Architecture

## Architectural Strategy

The application is implemented as a **Modular Monolith using Vertical
Slices**.

Each feature represents a **business use case**.

Benefits:

-   high cohesion
-   low coupling
-   independent evolution of features
-   easier migration to microservices in the future

------------------------------------------------------------------------

# Project Structure

src

Domain - Entities - ValueObjects - DomainServices

Infrastructure - Database - Repositories - Kafka - Scheduler -
CotahistParser

Features - Clients - SubscribeClient - Contributions -
ProcessContribution - Purchases - ExecutePurchase - Distribution -
DistributeAssets - Portfolio - RebalancePortfolio - Taxes -
CalculateTaxes

API

------------------------------------------------------------------------

# Vertical Slice Structure

Each feature follows this structure:

FeatureName - Command / Query - Handler - Validator - Endpoint - Tests

Example:

SubscribeClient - Command.cs - Handler.cs - Validator.cs - Endpoint.cs -
Tests.cs

------------------------------------------------------------------------

# Domain Model

Main entities:

-   Client
-   Account
-   Holding
-   Portfolio
-   Trade
-   Contribution
-   Asset

------------------------------------------------------------------------

# Domain Events

Events used to decouple modules:

-   ContributionProcessed
-   PurchaseExecuted
-   AssetsDistributed
-   TaxesCalculated

------------------------------------------------------------------------

# Scheduler

A background job runs on:

-   day 5
-   day 15
-   day 25

Processing flow:

ProcessContribution → ExecutePurchase → DistributeAssets →
CalculateTaxes

------------------------------------------------------------------------

# Implementation Plan

## Phase 1 -- Project Setup

-   create solution
-   configure dependencies
-   configure database
-   configure Kafka
-   create slice structure

## Phase 2 -- Domain Modeling

Implement:

-   Client
-   Account
-   Holding
-   Portfolio
-   Trade

Add business rules.

## Phase 3 -- Client Registration

Slice: SubscribeClient

Responsibilities:

-   register client
-   define monthly contribution
-   create custody account

## Phase 4 -- Top Five Portfolio

Structure:

-   ticker
-   allocationPercent

## Phase 5 -- Market Data Parser

Service:

CotahistParser

Responsible for reading market price files.

## Phase 6 -- Contribution Processing

Slice: ProcessContribution

Responsibilities:

-   load active clients
-   calculate contribution amount
-   generate purchase plan

## Phase 7 -- Purchase Engine

Slice: ExecutePurchase

Responsibilities:

-   calculate asset quantities
-   create buy orders
-   update master account

Publishes event:

PurchaseExecuted

## Phase 8 -- Asset Distribution

Slice: DistributeAssets

Responsibilities:

-   distribute assets proportionally
-   update client holdings

Publishes event:

AssetsDistributed

## Phase 9 -- Tax Calculation

Slice: CalculateTaxes

Publishes event:

tax-calculated

## Phase 10 -- Portfolio Rebalancing

Slice: RebalancePortfolio

Responsibilities:

-   compare current vs target allocation
-   generate rebalancing trades

------------------------------------------------------------------------

# Testing Strategy

### Domain Tests

-   average price calculation
-   proportional distribution
-   rebalancing logic

### Integration Tests

-   feature flows
-   persistence

------------------------------------------------------------------------

# Technology Stack

-   .NET 8
-   Minimal API
-   Vertical Slice Architecture
-   Apache Kafka
-   EF Core
-   xUnit

------------------------------------------------------------------------

# Conclusion

This solution prioritizes:

-   simple and scalable architecture
-   strong domain modeling
-   feature‑level cohesion
-   asynchronous integration through Kafka

Vertical Slice Architecture enables incremental development while
keeping the system maintainable and extensible.
