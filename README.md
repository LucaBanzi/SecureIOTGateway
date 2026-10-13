# 🛡️ Secure IoT Gateway & Management Platform

A high-performance, Zero-Trust IoT Management Platform built with **.NET 8 Web API**, **PostgreSQL**, **Mosquitto MQTT Broker**, and **mTLS (Mutual TLS)** certificate-based authentication.

---

## 🎯 Overview

The **Secure IoT Gateway** provides an enterprise-grade backend infrastructure designed to securely ingest real-time telemetry from IoT edge devices (e.g., ESP32, Raspberry Pi, Industrial Gateways) while offering a centralized REST API for device lifecycle management.

### Key Features
* **Zero-Trust Security with mTLS / X.509:** Device authentication enforced at the network protocol layer (TLS 1.2/1.3) using client certificates issued by a local Certificate Authority (CA).
* **Application-Level Certificate Validation:** Device access control validated against dynamic database state (`isActive` flag and SHA-256 Thumbprints).
* **Real-time MQTT Ingestion:** Asynchronous `.NET BackgroundService` consuming telemetry streams from Mosquitto via wildcard topic filtering (`devices/+/telemetry`).
* **Relational Persistence:** Efficient telemetry logging and device relational tracking powered by **PostgreSQL 16** and **Entity Framework Core 8**.
* **Interactive Management Interface:** OpenAPI / Swagger UI integration for device registration and telemetry history querying.

---

## 🏗️ System Architecture