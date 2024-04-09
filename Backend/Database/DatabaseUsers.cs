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
            string query = "SELECT user_id,username,last_login_time_utc FROM users WHERE active=1 AND username=@username AND password_hash=@password_hash";
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
        } catch (Exception) {
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

        Log($"Created user", LogCodes.UserCreated, id);

        // Return ID
        return id;
    }
    public static async Task DeleteUserAsync(int userId) {
        string query = "UPDATE users SET active=0 WHERE user_id=@user_id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@user_id", userId);
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected != 1) throw new Exception($"Unable to mark user as removed! {userId}");

        Log($"Removed user", LogCodes.UserRemoved, userId);
    }

    private static async Task<bool> UsernameInUseAsync(string username) {
        string query = @"SELECT COUNT(*) FROM users WHERE username=@username";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@username", username);
        object? count = await cmd.ExecuteScalarAsync();

        if (count is null) return false;

        return Convert.ToInt32(count) > 0;
    }
    public static async Task UpdateUserAsync(User user, bool fireAndForget = false) {

        // Make sure user.Id is found!
        if (user.Id is null) throw new ArgumentNullException(nameof(user.Id));

        // Check if there is nothing to update
        if (user.Username is null && user.Password is null && user.LastLoginUTC is null) return;

        // Check if new username is already in use
        if(user.Username is not null && await UsernameInUseAsync(user.Username)) throw new UsernameInUseException();

        // Update values only if not null
        string query = "UPDATE users SET" ;
        if (user.Username is not null) query += " username=@username";
        if (user.Password is not null) query += ", password_hash=@password_hash";
        if (user.LastLoginUTC is not null) query += ", last_login_time_utc=@last_login_time_utc";
        query += " WHERE user_id=@user_id";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        // Update values only if not null
        if (user.Username is not null) cmd.Parameters.AddWithValue("@username", user.Username);
        if (user.Password is not null) cmd.Parameters.AddWithValue("@password_hash", user.Password);
        if (user.LastLoginUTC is not null) cmd.Parameters.AddWithValue("@last_login_time_utc", user.LastLoginUTC);
        cmd.Parameters.AddWithValue("@user_id", user.Id);
        
        if (fireAndForget) {
            _ = cmd.ExecuteNonQueryAsync();
        } else {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected != 1) throw new Exception($"Unable to update user! {user.Id}");
        }
        Log($"Updated user. Username:{user.Username != null}, Password:{user.Password != null}, LoginTime:{user.LastLoginUTC != null}", LogCodes.UserUpdated, user.Id);
    }
}