using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using MySqlConnector;
using MongoDB.Driver;

public class DbDemoWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string? dbType = Environment.GetEnvironmentVariable("DB_TYPE");
        string? connStr = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        dbType = dbType?.ToLower() switch
        {
            "postgres" => "postgres",
            "mysql" => "mysql",
            "mongo" => "mongo",
            _ => "postgres"
        };

        if (string.IsNullOrWhiteSpace(connStr))
        {
            Console.WriteLine("DB_CONNECTION_STRING not set. Exiting.");
            return;
        }

        Console.WriteLine($"DB_TYPE: {dbType}");
        Console.WriteLine($"DB_CONNECTION_STRING: {connStr}");

        // Wait for the DB to be ready before starting
        try
        {
            await WaitForDatabaseReady(dbType, connStr, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database not available after waiting: {ex.Message}");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                switch (dbType)
                {
                    case "postgres":
                        await PostgresInsert(connStr);
                        break;
                    case "mysql":
                        await MySqlInsert(connStr);
                        break;
                    case "mongo":
                        await MongoInsert(connStr);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error after initial DB ready] {ex.Message}");
            }
            await Task.Delay(10000, stoppingToken);
        }
    }

    // ... Paste WaitForDatabaseReady, PostgresInsert, MySqlInsert, MongoInsert, DemoLog from your original Program.cs here ...
    static async Task WaitForDatabaseReady(string dbType, string connStr, CancellationToken token, int maxAttempts = 30, int delaySeconds = 2)
    {
        for (int i = 1; i <= maxAttempts; i++)
        {
            try
            {
                switch (dbType)
                {
                    case "postgres":
                        using (var conn = new NpgsqlConnection(connStr))
                        {
                            await conn.OpenAsync(token);
                            Console.WriteLine("[Postgres] Connection successful.");
                            return;
                        }
                    case "mysql":
                        using (var conn = new MySqlConnection(connStr))
                        {
                            await conn.OpenAsync(token);
                            Console.WriteLine("[MySQL] Connection successful.");
                            return;
                        }
                    case "mongo":
                        var client = new MongoClient(connStr);
                        await client.ListDatabaseNamesAsync(cancellationToken: token);
                        Console.WriteLine("[MongoDB] Connection successful.");
                        return;
                }
            }
            catch
            {
                Console.WriteLine($"[{dbType}] Waiting for database... ({i}/{maxAttempts})");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            }
        }
        throw new Exception($"Database {dbType} not available after {maxAttempts * delaySeconds} seconds.");
    }

    static async Task PostgresInsert(string connStr)
    {
        using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();
        var cmdCreate = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS demo_log (id SERIAL PRIMARY KEY, log_time TIMESTAMPTZ)", conn);
        await cmdCreate.ExecuteNonQueryAsync();
        var cmdInsert = new NpgsqlCommand("INSERT INTO demo_log (log_time) VALUES (@now)", conn);
        cmdInsert.Parameters.AddWithValue("now", DateTime.UtcNow);
        await cmdInsert.ExecuteNonQueryAsync();
        Console.WriteLine($"[Postgres] Inserted {DateTime.UtcNow}");
    }

    static async Task MySqlInsert(string connStr)
    {
        using var conn = new MySqlConnection(connStr);
        await conn.OpenAsync();
        var cmdCreate = new MySqlCommand("CREATE TABLE IF NOT EXISTS demo_log (id INT AUTO_INCREMENT PRIMARY KEY, log_time DATETIME(6))", conn);
        await cmdCreate.ExecuteNonQueryAsync();
        var cmdInsert = new MySqlCommand("INSERT INTO demo_log (log_time) VALUES (@now)", conn);
        cmdInsert.Parameters.AddWithValue("@now", DateTime.UtcNow);
        await cmdInsert.ExecuteNonQueryAsync();
        Console.WriteLine($"[MySQL] Inserted {DateTime.UtcNow}");
    }

    static async Task MongoInsert(string connStr)
    {
        var client = new MongoClient(connStr);
        var db = client.GetDatabase("otel");
        var collection = db.GetCollection<DemoLog>("demo_log");
        var log = new DemoLog { log_time = DateTime.UtcNow };
        await collection.InsertOneAsync(log);
        Console.WriteLine($"[MongoDB] Inserted {DateTime.UtcNow}");
    }

    class DemoLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime log_time { get; set; }
    }
}