
using System.Data;
using System.Text.Json;
using MySqlConnector;

namespace Backend;
public static partial class Database {
    
    public static async Task<List<Activity>> GetActivitiesAsync(int userId, string? filter) {
        
        string query = "SELECT * FROM activities WHERE owner_id=@owner_id ";
        if (!string.IsNullOrEmpty(filter)) query += $" AND LOWER(name) LIKE '%{filter.ToLower()}%' ";
        query += "ORDER BY activity_id";
    
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", userId);

        // Read data from Database
        List<Activity> activities = [];
        using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            Activity activity = new() {
                Id = reader.GetInt32("activity_id"),
                OwnerId = reader.GetInt32("owner_id"),
                Name = reader.GetString("name"),
                Description = await reader.IsDBNullAsync("description") ? null : reader.GetString("description"),
                Status = (Activity.ActivityStatus?)reader.GetInt32("status"),
                StartDateUTC = reader.GetDateTime("start_date_utc"),
                EndDateUTC =  await reader.IsDBNullAsync("end_date_utc") ? null : reader.GetDateTime("end_date_utc"),
                DueDateUTC =  await reader.IsDBNullAsync("due_date_utc") ? null : reader.GetDateTime("due_date_utc"),
                TimesCompleted = reader.GetInt32("times_completed")
            };

            activity.IsCompleted = activity.Status is Activity.ActivityStatus.InProgress;
            
            // Retrieve taskIds from the tasks table based on the current activity_id
            List<int> taskIds = [];
            string connectionString = GetConnectionBuilder().Builder.ConnectionString;
            using (MySqlConnection innerConnection = new(connectionString)) {
                await innerConnection.OpenAsync();
                using MySqlCommand taskCmd = new MySqlCommand("SELECT task_id FROM tasks WHERE activity_id = @activity_id", innerConnection);
                taskCmd.Parameters.AddWithValue("@activity_id", activity.Id);
                using MySqlDataReader taskReader = await taskCmd.ExecuteReaderAsync();
                while (await taskReader.ReadAsync()) taskIds.Add(taskReader.GetInt32("task_id"));
                activity.TaskIds = taskIds.ToArray();
            }
            
            // Add activity to list
            activities.Add(activity);
        }

        return activities;
    }
    public static async Task<bool> ActivityNameInUse(int? ownerId, string? name) {
        if (ownerId is null || name is null) return false;

        string query = "SELECT EXISTS (SELECT 1 FROM activities WHERE owner_id=@owner_id AND name=@name COLLATE utf8mb4_general_ci) AS row_exists;";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", ownerId);
        cmd.Parameters.AddWithValue("@name", name);

        bool nameInUse = Convert.ToBoolean(await cmd.ExecuteScalarAsync());
        return nameInUse;
    }
        
    public static async Task DeleteActivityAsync(int userId, int taskId) {
        string query = "DELETE FROM tasks WHERE owner_id=@owner_id AND task_id=@task_id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", userId);
        cmd.Parameters.AddWithValue("@task_id",taskId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected == 0) throw new Exception($"Unable to remove task! USER:{userId},  TASK:{taskId}");

        Log($"Removed task ID:{taskId}", LogCodes.TaskRemoved, userId);
    }
    public static async Task<Activity?> GetActivityAsync(int userId, int taskId) {
        string query = "SELECT * FROM tasks WHERE owner_id=@owner_id AND task_id=@task_id";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@owner_id", userId);
        cmd.Parameters.AddWithValue("@task_id",taskId);

        // Read data from Database
        using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            Activity activity = new() {
                Id = reader.GetInt32("task_id"),
                OwnerId = reader.GetInt32("owner_id"),
                Name = reader.GetString("name"),
                Description = await reader.IsDBNullAsync("description") ? null : reader.GetString("description"),
                Status = (Activity.ActivityStatus?)reader.GetInt32("status"),
                StartDateUTC = reader.GetDateTime("start_date_utc"),
                EndDateUTC = await reader.IsDBNullAsync("end_date_utc") ? null : reader.GetDateTime("end_date_utc"),
                DueDateUTC = await reader.IsDBNullAsync("due_date_utc") ? null : reader.GetDateTime("due_date_utc"),
            };

            // Check status / update if needed
            Activity.ActivityStatus? status = null;
            if (activity.DueDateUTC is not null && DateTime.UtcNow > activity.DueDateUTC) {
                status = Activity.ActivityStatus.Failed;
            }
            if (activity.EndDateUTC is not null && DateTime.UtcNow > activity.EndDateUTC && activity.Status != Activity.ActivityStatus.Done) {
                // TODO
            }

            if (status != activity.Status) {
                activity.Status = status;
            }

            activity.IsCompleted = activity.Status is Activity.ActivityStatus.InProgress;

            return activity;
        }

        return null;
    }

    public static async Task UpdateActivityAsync(TodoTask task, bool incrementTimesCompleted = false) {
        // Update values only if not null
        string query = "UPDATE tasks SET" ;

        // Add only the values that were changed to query string
        if (task.Name != null) query += " name=@name";
        if (task.Name != null && (task.Description != null || task.Status != null || task.EndDateUTC != null)) query += ",";

        if (task.Description != null) query += " description=@description";
        if (task.Description != null && (task.Status != null || task.EndDateUTC != null)) query += ",";

        if (task.Status != null) query += " status=@status";
        if (task.Status != null && task.EndDateUTC != null) query += ",";

        if (task.EndDateUTC != null) query += " end_date_utc=@end_date_utc";
        if (incrementTimesCompleted) query += ", times_completed = times_completed + 1";

        query += " WHERE task_id=@task_id";

        using MySqlCommand cmd = new MySqlCommand(query, Connection);

        cmd.Parameters.AddWithValue("@task_id", task.Id);
        cmd.Parameters.AddWithValue("@name", task.Name);
        cmd.Parameters.AddWithValue("@description", task.Description);
        cmd.Parameters.AddWithValue("@end_date_utc", task.EndDateUTC);
        cmd.Parameters.AddWithValue("@status", task.Status);


        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected != 1) throw new Exception($"Unable to update task! {task.Id}");
    
        Log($"Updated task. Name:{task.Name != null}, Description:{task.Description != null}, Status:{task.Status != null}, EndTime:{task.EndDateUTC != null}", LogCodes.TaskUpdated, task.OwnerId);
    }
    public static async Task<int> CreateActivityAsync(Activity activity) {
        if (await ActivityNameInUse(activity.OwnerId, activity.Name)) throw new ActivityNameInUseException("Activity name already in use");
        
        string query = @$"INSERT INTO activities (owner_id, name, description, start_date_utc, due_date_utc, status)
            VALUES (@owner_id, @name, @description, @start_date_utc, @due_date_utc, @status);
            SELECT LAST_INSERT_ID();
        ";
        using MySqlCommand cmd = new MySqlCommand(query, Connection);
        
        cmd.Parameters.AddWithValue("@owner_id", activity.OwnerId);
        cmd.Parameters.AddWithValue("@name", activity.Name);
        cmd.Parameters.AddWithValue("@description", activity.Description);
        cmd.Parameters.AddWithValue("@start_date_utc", activity.StartDateUTC);
        cmd.Parameters.AddWithValue("@due_date_utc", activity.DueDateUTC);
        cmd.Parameters.AddWithValue("@status", activity.Status);

        // Return created ID from the database
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        if (id == 0) throw new Exception($"Unable to add activity for user! (1) ({activity.OwnerId}) - {activity.Name}");

        // Update tasks foreign key
        // TODO might be a slow operation if there is a lot of tasks
        query = "UPDATE tasks SET activity_id=@activity_id WHERE task_id IN ({0})";
        string taskIdList = string.Join(",", activity.TaskIds);
        query = string.Format(query, taskIdList);

        using MySqlCommand cmd2 = new MySqlCommand(query, Connection);
        cmd2.Parameters.AddWithValue("@activity_id", id);

        int rowsAffected = await cmd2.ExecuteNonQueryAsync();
        if (rowsAffected != activity.TaskIds.Length) throw new Exception($"Unable to add activity for user! (2) ({activity.OwnerId}) - {activity.Name}");

        Log($"Created new activity ID:{activity.Id}", LogCodes.ActivityCreated, activity.OwnerId);
        return id;
    }
}
