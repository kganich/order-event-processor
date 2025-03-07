# order-event-processor

## Overview

Order Event Processor is a .NET 8 console application designed to handle order-related events using RabbitMQ and store order data in a PostgreSQL database. The application ensures correct event processing, even if a PaymentEvent arrives before its corresponding OrderEvent.

### Features

- Event-driven architecture using RabbitMQ.

- Ensures proper event sequencing when processing orders and payments.

- PostgreSQL database integration for order and payment storage.

- Docker support for containerized deployment.

- Event Consumers to handle asynchronous message processing.

- Supports X-MsgType header detection to differentiate between OrderEvent and PaymentEvent messages.


### How It Works

1. The application connects to a RabbitMQ message broker.

2. It sends test OrderEvent and PaymentEvent messages.

3. Consumers listen for incoming events:

    - OrderEventConsumer: Handles order events and stores them in PostgreSQL.

    - PaymentEventConsumer: Handles payment events, stores them in PostgreSQL, and updates the corresponding order status.

4. The application ensures that orders are correctly stored in PostgreSQL.

5. If a PaymentEvent arrives before its corresponding OrderEvent, it is stored temporarily and processed once the OrderEvent arrives.

6. The application logs events and errors to the console.

### Models

```
OrderEvent
{
  "id": "string",          // Order identification, e.g., O-123
  "product": "string",     // Product identification, e.g., PR-ABC
  "total": "decimal",      // Total order price, pattern ####.##, e.g., 12.34
  "currency": "string"     // Currency identification, ISO-4217, e.g., USD
}
```

```
PaymentEvent

{
  "orderId": "string",     // Order identification, e.g., O-123
  "amount": "decimal"       // Amount of price paid, pattern ####.##, e.g., 11.00
}
```

### Prerequisites

Before running the application, ensure you have the following installed:

- .NET 8 SDK

- RabbitMQ 

- PostgreSQL 

- Docker (optional, for containerized deployment)


### Installation and Setup

1. Clone the repository:

```
git clone
```

2. Modify connection strings 


3. Run the application in a Docker container, build the image and run the container:

```
docker compose build
docker compose up -d
```









