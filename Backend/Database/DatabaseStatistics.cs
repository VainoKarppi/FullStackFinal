
using System.Data;
using System.Text.Json;
using MySqlConnector;

namespace Backend;
public static partial class Database {
    
    public static async Task<Statistic> GetStatistics(int userId) {

        string query = @$"SELECT SUM(STATUS = 0) AS TasksInProgress, SUM(times_completed) AS TasksCompleted FROM tasks WHERE owner_id=@owner_id";
        
        Statistic statistic = new();

        using (MySqlCommand cmd = new MySqlCommand(query, Connection)) {
            
            cmd.Parameters.AddWithValue("@owner_id", userId);

            using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                statistic.TasksCompleted = reader.GetInt32("TasksCompleted");
                statistic.TasksInProgress = reader.GetInt32("TasksInProgress");
            }
            await cmd.DisposeAsync();
        }

        query = @$"SELECT SUM(STATUS = 0) AS ActivitiesInProgress, SUM(times_completed) AS ActivitiesCompleted FROM activities WHERE owner_id=@owner_id";
        using (MySqlCommand cmd = new MySqlCommand(query, Connection)) {
            cmd.Parameters.AddWithValue("@owner_id", userId);

            using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                statistic.ActivitiesCompleted = reader.GetInt32("ActivitiesCompleted");
                statistic.ActivitiesInProgress = reader.GetInt32("ActivitiesInProgress");
            }
            await cmd.DisposeAsync();
        }

        return statistic;
    }
}
