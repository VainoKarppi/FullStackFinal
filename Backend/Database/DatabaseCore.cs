
using System.Data;
using MySqlConnector;

namespace Backend;

public static partial class Database {
    private static MySqlConnection? Connection;
    public class ConnectionManager {
        public required MySqlConnectionStringBuilder Builder { get; set; }
        public bool Autocreate { get; set; } = false;
    }
    public static ConnectionManager GetConnectionBuilder() {
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
            Database = database,
            Password = password,
        };

        return new ConnectionManager() {Builder = builder, Autocreate = autocreate};
    }
    public static async void ConnectToDatabase() {
        
        ConnectionManager manager = GetConnectionBuilder();

        // Remove Database from connection query, so that we can open the database if its not found yet
        string database = manager.Builder.Database;
        manager.Builder.Database = null;

        // Connect to database and open it. For now lets keep the socket always open
        Connection = new MySqlConnection(manager.Builder.ConnectionString);
        await Connection.OpenAsync();

        // Create actual database if not found
        if (manager.Autocreate) {
            using MySqlCommand command = new ($"CREATE DATABASE IF NOT EXISTS {database}", Connection);
            if (await command.ExecuteNonQueryAsync() != 0)
                Console.WriteLine($"Database not found, and new was created! ({database})");
        }

        // Set active database. Will throw error if database not found!
        await Connection.ChangeDatabaseAsync(database);

        // Create ncessary tables automatically if not created
        if (manager.Autocreate) CreateTables();
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
        int tablesCreated = 0;
        // Users
        string tableName = "users";
        using MySqlCommand users = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            user_id INT AUTO_INCREMENT PRIMARY KEY,
            username VARCHAR(64) NOT NULL,
            password_hash VARCHAR(64) NOT NULL,
            salt INT NOT NULL,
            last_login_time_utc TIMESTAMP NULL DEFAULT NULL,
            registration_date_utc TIMESTAMP DEFAULT UTC_TIMESTAMP(),
            active TINYINT(1) NOT NULL DEFAULT 1
        )", Connection);
        tablesCreated += users.ExecuteNonQuery();


        // ActivityTypes
        tableName = "activity_types";
        using MySqlCommand activityTypes = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            activity_type_id INT AUTO_INCREMENT PRIMARY KEY,
            name CHAR NOT NULL
        )", Connection);
        tablesCreated += activityTypes.ExecuteNonQuery();

        // Activities
        tableName = "activities";
        using MySqlCommand activities = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            activity_id INT AUTO_INCREMENT PRIMARY KEY,
            owner_id INT NOT NULL,
            name VARCHAR(255) NOT NULL,
            description TEXT,
            start_date_utc TIMESTAMP,
            end_date_utc TIMESTAMP NULL DEFAULT NULL,
            due_date_utc TIMESTAMP NULL DEFAULT NULL,
            status TINYINT UNSIGNED NOT NULL,
            tags VARCHAR(255),
            times_completed INT NOT NULL DEFAULT 0,
            activity_type_id INT NULL DEFAULT NULL,
            FOREIGN KEY (activity_type_id) REFERENCES activity_types(activity_type_id),
            FOREIGN KEY (owner_id) REFERENCES users(user_id)
        )", Connection);
        tablesCreated += activities.ExecuteNonQuery();
        
        // Tasks
        tableName = "tasks";
        using MySqlCommand tasks = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            task_id INT AUTO_INCREMENT PRIMARY KEY,
            owner_id INT NOT NULL,
            activity_id INT NULL DEFAULT NULL,
            name VARCHAR(255) NOT NULL,
            description TEXT NULL DEFAULT NULL,
            start_date_utc TIMESTAMP NOT NULL,
            end_date_utc TIMESTAMP NULL DEFAULT NULL,
            tags VARCHAR(255) NULL DEFAULT NULL,
            status TINYINT UNSIGNED NOT NULL,
            times_completed INT NOT NULL DEFAULT 0,
            FOREIGN KEY (activity_id) REFERENCES activities(activity_id),
            FOREIGN KEY (owner_id) REFERENCES users(user_id)
        )", Connection);
        tablesCreated += tasks.ExecuteNonQuery();

        // Logs
        tableName = "logs";
        using MySqlCommand logs = new ($@"CREATE TABLE IF NOT EXISTS {tableName} (
            log_id INT AUTO_INCREMENT PRIMARY KEY,
            user_id INT NULL DEFAULT NULL,
            timestamp TIMESTAMP,
            code TINYINT UNSIGNED NULL DEFAULT NULL,
            message TEXT NULL DEFAULT NULL,
            FOREIGN KEY (user_id) REFERENCES users(user_id)
        )", Connection);
        tablesCreated += logs.ExecuteNonQuery();

        if (tablesCreated > 0) Console.WriteLine($"Automatically Created {tablesCreated} new tables!");
    }
    
}

