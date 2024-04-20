
using System.Data;
using System.Text.Json;
using MySqlConnector;

namespace Backend;
public static partial class Database {
    
    public static async Task<Statistic> GetStatistics(int userId) {

        string query = @$"INSERT INTO activities (owner_id, name, description, start_date_utc, due_date_utc, status)
            VALUES (@owner_id, @name, @description, @start_date_utc, @due_date_utc, @status);
            SELECT LAST_INSERT_ID();
        ";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);
        
        cmd.Parameters.AddWithValue("@owner_id", userId);

        Statistic statistic = new Statistic();
        
        using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            TodoTask task = new() {
                Id = reader.GetInt32("task_id"),
                OwnerId = reader.GetInt32("owner_id"),
                ActivityId = await reader.IsDBNullAsync("activity_id") ? null : reader.GetInt32("activity_id"),
                Name = reader.GetString("name"),
                Description = await reader.IsDBNullAsync("description") ? null : reader.GetString("description"),
                Status = (TodoTask.TaskStatus?)reader.GetInt32("status"),
                StartDateUTC = reader.GetDateTime("start_date_utc"),
                EndDateUTC =  await reader.IsDBNullAsync("end_date_utc") ? null : reader.GetDateTime("end_date_utc")
            };

            task.IsCompleted = task.Status is TodoTask.TaskStatus.InProgress;

            tasks.Add(task);
        }
        Statistic statistic = new Statistic();
        return statistic;
    }
}
