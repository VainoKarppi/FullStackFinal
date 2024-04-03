
using System.Data;
using MySqlConnector;



public static partial class Database {
    private static MySqlConnection? Connection;

    public static async void ConnectToDatabase() {
        IConfigurationRoot configuration = Program.Configuration;

        // Retrieve configuration values for MySql connection
        string? server = configuration.GetValue<string>("Database:Server");
        uint port = configuration.GetValue<uint>("Database:Port");
        string? database = configuration.GetValue<string>("Database:Database");
        string? userId = configuration.GetValue<string>("Database:Username");
        string? password = configuration.GetValue<string>("Database:Password");
        bool autocreate = configuration.GetValue<bool>("Database:AutoCreateTablesAndDatabase");

        // Settings to connect to the database
        MySqlConnectionStringBuilder builder = new() {
            Server = server,
            Port = port,
            UserID = userId,
            Password = password
        };

        // Connect to database and open it. For now lets keep the socket always open
        Connection = new MySqlConnection(builder.ConnectionString);
        await Connection.OpenAsync();

        // Create actual database if not found
        if (autocreate) {
            using MySqlCommand command = new ($"CREATE DATABASE IF NOT EXISTS {database}", Connection);
            if (await command.ExecuteNonQueryAsync() != 0)
                Console.WriteLine($"Database not found, and new was created! ({database})");
        }

        // Set active database. Will throw error if database not found!
        await Connection.ChangeDatabaseAsync(database!);

        // Create ncessary tables automatically if not created
        if (autocreate) CreateTables();
    }


    // Close database and disposes socket
    public static async void CloseDatabase() {
        if (Connection is null) return;
        
        await Connection.CloseAsync();
        await Connection.DisposeAsync();
        
        Connection = null;
    }

    // Check if database is open and simple qyery can be made
    public static bool IsConnectedToDatabase() {
        if (Connection is null) return false;
        if (Connection.State != ConnectionState.Open) return false;

        // Make sure database can be accessed using a simple query
        try {
            using MySqlCommand command = new("SELECT 1", Connection);
            command.ExecuteScalar();
            return true; 
        } catch (Exception) {
            return false;
        }
    }

    // TODO tasks and
    private static void CreateTables() {
        string tableName = "tasks";
        using MySqlCommand weatherdata = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            Id INT AUTO_INCREMENT PRIMARY KEY,
            device_name TEXT,
            timestamp TIMESTAMP,
            humidity FLOAT NULL DEFAULT NULL,
            temperature FLOAT NULL DEFAULT NULL,
            wind FLOAT NULL DEFAULT NULL,
            pressure FLOAT NULL DEFAULT NULL
        )", Connection);
        weatherdata.ExecuteNonQuery();

        tableName = "users";
        using MySqlCommand users = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            Id INT AUTO_INCREMENT PRIMARY KEY,
            username VARCHAR(64) NOT NULL,
            password_hash TEXT NOT NULL,
            salt INT NOT NULL,
            lastLoginTime TIMESTAMP NULL DEFAULT NULL
        )", Connection);
        users.ExecuteNonQuery();

        
        tableName = "logs";
        using MySqlCommand logs = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            Id INT AUTO_INCREMENT PRIMARY KEY,
            user_id INT NULL DEFAULT NULL,
            timestamp TIMESTAMP,
            code INT NULL DEFAULT NULL,
            message TEXT NULL DEFAULT NULL,
            FOREIGN KEY (user_id) REFERENCES users(Id)
        )", Connection);
        logs.ExecuteNonQuery();
    }
    
}



public class TodoTask {
    public enum TaskStatus {
        New,
        InProgress,
        Done,
        Cancelled
    }

    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string? Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string[]? Tags { get; set; }
    public TaskStatus? Status { get; set; }
}

public class User {
    public int? Id { get; set; }
    public string? Username { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? PasswordHash { get; set; }
    public Guid? SessionToken { get; set; }
    public string? Salt { get; set; }

    public User() {} 
    public User(string username, string passwordHash) {
        Username = username;
        PasswordHash = passwordHash;
        LastLogin = DateTime.Now;

        var sessionguid = Guid.NewGuid();
        SessionManager.AddSession(sessionguid);
        SessionToken = sessionguid;
    }
}