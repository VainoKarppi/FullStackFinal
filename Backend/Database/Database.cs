// TODO Get user
// TODO Remove User

// TODO Get Tasks
// TODO Add Task
// TODO Remove Task


using System.Text.Json;
using MySqlConnector;

namespace Backend;
public static partial class Database {
    private static string GetSalt(string username) {
        // Get user salt from database
        string sql = "SELECT DISTINCT Salt FROM users WHERE username=@username";
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
            string query = "SELECT id,username,lastLoginTime FROM users WHERE username=@username AND password_hash=@passwordHash";
            using MySqlCommand cmd = new MySqlCommand(query, Connection);

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@passwordHash", hashedPassword);

            User? user = null;

            // Read USER data from Database
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                user = new User() {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    LastLogin = reader.GetDateTime("lastLoginTime")
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
        string query = @"INSERT INTO users (username, password_hash, salt, lastLoginTime)
            VALUES (@username, @passwordHash, @salt, @lastLoginTime);
            SELECT LAST_INSERT_ID();
        ";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        // Calculate random key as salt and hash it
        int salt = new Random().Next(10000, 100000);
        string hashedPassword = HashCalculator.ComputeSHA256Hash(user.PasswordHash + salt.ToString());

        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@passwordHash", hashedPassword);
        cmd.Parameters.AddWithValue("@salt", salt);
        cmd.Parameters.AddWithValue("@lastLoginTime", user.LastLogin);

        // Return created ID from the database
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        if (id == 0) throw new Exception($"Unable to add user! ({user.Username})");

        // Return ID
        return id;
    }
    public static Task<bool> RemoveUserAsync(User user) {
        throw new NotImplementedException();
    }

    public static async Task UpdateUserAsync(User user, bool fireAndForget = false) {

        // Update password only if not null
        string query = "UPDATE users SET username=@username,";
        if (user.PasswordHash is not null) query += " password_hash=@passwordHash,";
        query += " lastLoginTime=@logintime WHERE Id=@id";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@username",user.Username);
        cmd.Parameters.AddWithValue("@logintime", user.LastLogin);
        cmd.Parameters.AddWithValue("@id", user.Id);

        // Update password only if not null
        if (user.PasswordHash is not null) cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        
        if (fireAndForget) {
            _ = Task.Run(cmd.ExecuteNonQueryAsync);
        } else {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected != 1) throw new Exception($"Unable to update user! {user.Id}");
        }
    }

    public static async Task<List<TodoTask>> GetTasksAsync(int userId) {
        string query = "SELECT * FROM tasks WHERE id=@id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@id", userId);

        // Read data from Database
        List<TodoTask> tasks = [];
        var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            TodoTask task = new() {
                Id = reader.GetInt32("id"),
                OwnerId = reader.GetInt32("user"),
                Name = reader.GetString("name"),
                //task.Status = reader.GetInt16("password_hash");
                StartDate = reader.GetDateTime("start"),
                EndDate = reader.GetDateTime("end")
            };

            tasks.Add(task);
        }

        return tasks;
    }

    // TODO
    public static async Task<TodoTask> CreateTaskAsync(int userId) {

        TodoTask task = new() {

        };
        string query = "UPDATE tasks SET FROM tasks WHERE id=@id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@id", userId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected != 1) throw new Exception($"Unable to add task for user! {userId}");

        return task;
    }
}


