// TODO Get user
// TODO Add User
// TODO Remove User

// TODO Get Tasks
// TODO Add Task
// TODO Remove Task


using MySqlConnector;

public static partial class Database {
    private static string? GetSalt(string username) {
        // Get user salt from database
        string sql = "SELECT DISTINCT Salt FROM users WHERE username@username";
        using var command = new MySqlCommand(sql, Connection);
        command.Parameters.AddWithValue("@username", username);

        // Read salt
        var salt = command.ExecuteScalar()!.ToString();

        // Return salt + Server Secret Key combined!
        return salt! + HashCalculator.ServerSecurityHash;
    }

    public static async Task<User?> GetUserAsync(string username, string passwordHash) {
        try {
            // Get User salt and apply this to final password
            string? salt = GetSalt(username) ?? throw new Exception($"Salt not found for user: {username}");
            string finalHashPassword = HashCalculator.ComputeSHA256Hash(passwordHash + salt);

            // Get data for User From database using username and password hash
            string query = "SELECT * FROM users WHERE username@username AND password_hash=@passwordHash";
            using MySqlCommand cmd = new MySqlCommand(query, Connection);

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password_hash", finalHashPassword);

            var user = new User();

            // Read data from Database
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                user.Id = reader.GetInt32("id");
                user.Username = reader.GetString("username");
                user.PasswordHash = reader.GetString("password_hash");
                user.LastLogin = reader.GetDateTime("lastLoginTime");
            }

            // Make sure the data was found!
            if (user.Id is null) return null;

            // Return actual user
            return user;
        } catch (Exception) {
            return null;
        }
    }
    public static async Task<int> AddUser(User user) {
        // Return ID
        return 0;
        throw new NotImplementedException();
    }
    public static Task<bool> RemoveUser(User user) {
        throw new NotImplementedException();
    }

    public static async void UpdateUserAsync(User user) {
        string query = "UPDATE users SET username=@username, password_hash=@passwordHash, lastLoginTime=@logintime WHERE Id=@id";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@username",user.Username);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@logintime", user.LastLogin);
        cmd.Parameters.AddWithValue("@id", user.Id);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected != 1) throw new Exception($"Unable to update user! {user.Id}");
    }

    public static Task<Task> GetTask() {
        throw new NotImplementedException();
    }
}


