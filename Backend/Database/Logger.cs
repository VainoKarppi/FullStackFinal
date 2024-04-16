
using MySqlConnector;

namespace Backend;


public static partial class Database {
    public static void Log(string message, LogCodes code, int? userId = null) {
        _ = Task.Run(() => {
            // Add userId to query if exists and not null is used
            string query = "INSERT INTO logs (message,timestamp,code";
            if (userId is not null) query += ",user_id";
            query += ") VALUES (@message,@timestamp,@code";
            if (userId is not null) query += ",@user_id";
            query += ")";

            using MySqlCommand cmd = new MySqlCommand(query, Connection);

            cmd.Parameters.AddWithValue("@message", message);
            cmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@code", (int)code);
            if(userId is not null) cmd.Parameters.AddWithValue("@user_id", userId);
            
            // Dont wait for response, to continue faster
            _ = cmd.ExecuteNonQueryAsync();
        });
    }
}