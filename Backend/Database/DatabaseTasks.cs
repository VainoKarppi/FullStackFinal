
using System.Data;
using MySqlConnector;

namespace Backend;
public static partial class Database {
    
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

    public static async Task DeleteTaskAsync(int userId, int taskId) {
        string query = "DELETE FROM tasks WHERE owner_id=@owner_id AND task_id=@task_id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", userId);
        cmd.Parameters.AddWithValue("@task_id",taskId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected == 0) throw new Exception($"Unable to remove task! USER:{userId},  TASK:{taskId}");

        Log($"Removed task ID:{taskId}", LogCodes.TaskRemoved, userId);
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

    public static async Task UpdateTaskAsync(TodoTask task) {
        // Update values only if not null
        string query = "UPDATE tasks SET" ;
        if (task.Name is not null) query += " name=@name";
        if (task.Description is not null) query += ", description=@description";
        if (task.Status is not null) query += ", status=@status";
        if (task.EndDateUTC is not null) query += ", end_date_utc=@end_date_utc";
        query += " WHERE task_id=@task_id";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@task_id", task.Id);
        cmd.Parameters.AddWithValue("@name", task.Name);
        cmd.Parameters.AddWithValue("@description", task.Description);
        cmd.Parameters.AddWithValue("@end_date_utc", task.EndDateUTC);
        cmd.Parameters.AddWithValue("@status", task.Status);


        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected != 1) throw new Exception($"Unable to update task! {task.Id}");
    
        Log($"Updated task. Name:{task.Name != null}, Description:{task.Description != null}, End:{task.EndDateUTC != null}", LogCodes.TaskUpdated, task.OwnerId);
    }
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

        Log($"Created new task ID:{task.Id}", LogCodes.TaskCreated, task.OwnerId);
        return id;
    }
}
