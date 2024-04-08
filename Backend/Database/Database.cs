// TODO Get user
// TODO Remove User

// TODO Get Tasks
// TODO Add Task
// TODO Remove Task


using System.Data;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using MySqlConnector;

namespace Backend;
public static partial class Database {
    private static string GetSalt(string username) {
        // Get user salt from database
        string sql = "SELECT DISTINCT salt FROM users WHERE username=@username";
        using var command = new MySqlCommand(sql, Connection);
        command.Parameters.AddWithValue("@username", username);

        // Read salt
        string? salt = command.ExecuteScalar()!.ToString();

        if (string.IsNullOrEmpty(salt)) throw new Exception($"Unable to get hash for user: {username}");

        // Return salt
        return salt;
    }

    public static async Task<User?> GetUserAsync(string username, string password) {
        try {
            // Get User salt and apply this to final password
            string? salt = GetSalt(username) ?? throw new Exception($"Salt not found for user: {username}");
            string hashedPassword = HashCalculator.ComputeSHA256Hash(password + salt);


            // Get data for User From database using username and password hash
            string query = "SELECT id,username,last_login_time_utc FROM users WHERE username=@username AND password_hash=@password_hash";
            using MySqlCommand cmd = new MySqlCommand(query, Connection);

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password_hash", hashedPassword);

            User? user = null;

            // Read USER data from Database
            using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                user = new User() {
                    Id = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    LastLoginUTC = reader.GetDateTime("last_login_time_utc")
                };
            }

            // Make sure the data was found!
            if (user is null || user.Id is null) return null;

            // Return actual user
            return user;
        } catch (Exception ex) {
            Console.WriteLine(ex);
            return null;
        }
    }
    public static async Task<int> AddUserAsync(User user) {

        // Check if username is already in use by other user
        if(user.Username is not null && await UsernameInUseAsync(user.Username)) throw new UsernameInUseException();
        
        string query = @"INSERT INTO users (username, password_hash, salt, last_login_time_utc)
            VALUES (@username, @password_hash, @salt, @last_login_time_utc);
            SELECT LAST_INSERT_ID();
        ";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        // Calculate random key as salt and hash it
        int salt = new Random().Next(0, 100000);
        string hashedPassword = HashCalculator.ComputeSHA256Hash(user.Password + salt.ToString());

        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password_hash", hashedPassword);
        cmd.Parameters.AddWithValue("@salt", salt);
        cmd.Parameters.AddWithValue("@last_login_time_utc", user.LastLoginUTC);

        // Return created ID from the database
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        if (id == 0) throw new Exception($"Unable to add user! ({user.Username})");

        // Return ID
        return id;
    }
    public static async Task RemoveUserAsync(int userId) {
        string query = "DELETE FROM users WHERE user_id=@user_id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@user_id", userId);
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected != 1) throw new Exception($"Unable to remove user! {userId}");
    }

    private static async Task<bool> UsernameInUseAsync(string username) {
        string query = @"SELECT COUNT(*) FROM users WHERE username=@username";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@username", username);
        int? count = (int?)await cmd.ExecuteScalarAsync();

        return count is null || count > 0;
    }
    public static async Task UpdateUserAsync(User user, bool fireAndForget = false) {

        // Check if there is nothing to update
        if (user.Username is null && user.Password is null && user.LastLoginUTC is null) return;

        // Check if new username is already in use
        if(user.Username is not null && await UsernameInUseAsync(user.Username)) throw new UsernameInUseException();

        // Update values only if not null
        string query = "UPDATE users SET" ;
        if (user.Username is not null) query += " username=@username,";
        if (user.Password is not null) query += " password_hash=@password_hash,";
        if (user.LastLoginUTC is not null) query += " last_login_time_utc=@last_login_time_utc";
        query += " WHERE user_id=@user_id";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        // Update values only if not null
        if (user.Password is not null) cmd.Parameters.AddWithValue("@username", user.Username);
        if (user.Password is not null) cmd.Parameters.AddWithValue("@password_hash", user.Password);
        if (user.Password is not null) cmd.Parameters.AddWithValue("@last_login_time_utc", user.LastLoginUTC);
        
        if (fireAndForget) {
            _ = Task.Run(cmd.ExecuteNonQueryAsync);
        } else {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected != 1) throw new Exception($"Unable to update user! {user.Id}");
        }
    }

    public static async Task<List<TodoTask>> GetTasksAsync(int userId, string? filter, int offset = 0) {
        string query = "SELECT * FROM tasks WHERE owner_id=@owner_id ";

        // Add Filter if in use
        if (!string.IsNullOrEmpty(filter)) query += $" AND LOWER(name) LIKE '%{filter.ToLower()}%' ";

        query += "ORDER BY task_id LIMIT 10 OFFSET " + offset;

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", userId);

        // Read data from Database
        List<TodoTask> tasks = [];
        using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            TodoTask task = new() {
                Id = reader.GetInt32("task_id"),
                OwnerId = reader.GetInt32("owner_id"),
                Name = reader.GetString("name"),
                Description = await reader.IsDBNullAsync("description") ? null : reader.GetString("description"),
                Status = (TodoTask.TaskStatus?)reader.GetInt32("status"),
                StartDateUTC = reader.GetDateTime("start_date_utc"),
                EndDateUTC =  await reader.IsDBNullAsync("end_date_utc") ? null : reader.GetDateTime("end_date_utc")
            };

            task.IsCompleted = task.Status is TodoTask.TaskStatus.InProgress;

            tasks.Add(task);
        }

        return tasks;
    }

    public static async Task<TodoTask?> GetTaskAsync(int userId, int taskId) {
        string query = "SELECT * FROM tasks WHERE owner_id=@owner_id AND task_id=@task_id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", userId);
        cmd.Parameters.AddWithValue("@task_id",taskId);

        // Read data from Database
        List<TodoTask> tasks = [];
        using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            TodoTask task = new() {
                Id = reader.GetInt32("task_id"),
                OwnerId = reader.GetInt32("owner_id"),
                Name = reader.GetString("name"),
                Description = await reader.IsDBNullAsync("description") ? null : reader.GetString("description"),
                Status = (TodoTask.TaskStatus?)reader.GetInt32("status"),
                StartDateUTC = reader.GetDateTime("start_date_utc"),
                EndDateUTC =  await reader.IsDBNullAsync("end_date_utc") ? null : reader.GetDateTime("end_date_utc")
            };

            task.IsCompleted = task.Status is TodoTask.TaskStatus.InProgress;

            return task;
        }

        return null;
    }


    // TODO
    public static async Task<int> CreateTaskAsync(TodoTask task) {

        string query = @"INSERT INTO tasks (owner_id, name, description, start_date_utc, status)
            VALUES (@owner_id, @name, @description, @start_date_utc, @status);
            SELECT LAST_INSERT_ID();
        ";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", task.OwnerId);
        cmd.Parameters.AddWithValue("@name", task.Name);
        cmd.Parameters.AddWithValue("@description", task.Description);
        cmd.Parameters.AddWithValue("@start_date_utc", task.StartDateUTC);
        cmd.Parameters.AddWithValue("@status", task.Status);

        // Return created ID from the database
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        if (id == 0) throw new Exception($"Unable to add task for user! ({task.OwnerId}) - {task.Name}");

        Console.WriteLine($"Added task for user: {task.OwnerId} - ({task.Name})");
        return id;
    }
}
