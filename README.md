# OpenTelemetry Zero-Code DB Instrumentation Demo for .NET

This sample application demonstrates how to test and validate **OpenTelemetry zero-code instrumentation** for database calls to **PostgreSQL, MySQL, and MongoDB**.  
It uses **Docker Compose Profiles** to independently configure and run each scenario.

---

### Prerequisites
- **Docker Compose V2** (required, since profiles are only supported in V2)
- Docker installed
- `.env` file present in the project root (see configuration below)

---

### Environment Configuration

Set environment variables in your `.env` file depending on the database you are testing and then specify certain profile in "docker compose" command.

#### Scenario 1: PostgreSQL

.env file

```bash
DB_TYPE=postgres
DB_CONNECTION_STRING=Host=postgresql;Username=otelu;Password=otelp;Database=otel

#DB_TYPE=mysql
#DB_CONNECTION_STRING=Server=mysql;Uid=otelu;Pwd=otelp;Database=otel

#DB_TYPE=mongo
#DB_CONNECTION_STRING=mongodb://otelu:otelp@mongo:27017/otel?authSource=admin
```

Start/Stop

```bash
docker compose --profile postgres up --force-recreate -d
docker compose --profile postgres down
```

#### Scenario 2: MySQL

.env file

```bash
# DB_TYPE=mysql
# DB_CONNECTION_STRING=Host=postgresql;Username=otelu;Password=otelp;Database=otel

DB_TYPE=mysql
DB_CONNECTION_STRING=Server=mysql;Uid=otelu;Pwd=otelp;Database=otel

#DB_TYPE=mongo
#DB_CONNECTION_STRING=mongodb://otelu:otelp@mongo:27017/otel?authSource=admin
```

Start/Stop

```bash
docker compose --profile mysql up --force-recreate -d
docker compose --profile mysql down
```

#### Scenario 3: Mongo

.env file

```bash
# DB_TYPE=mysql
# DB_CONNECTION_STRING=Host=postgresql;Username=otelu;Password=otelp;Database=otel

# DB_TYPE=mysql
# DB_CONNECTION_STRING=Server=mysql;Uid=otelu;Pwd=otelp;Database=otel

DB_TYPE=mongo
DB_CONNECTION_STRING=mongodb://otelu:otelp@mongo:27017/otel?authSource=admin
```

Start/Stop

```bash
docker compose --profile mongo up --force-recreate -d
docker compose --profile mongo down
```
