


using System.Diagnostics;
using System.Text.Json;

namespace Backend;

public static partial class ApiMethods {
    public static async Task CreateTask(HttpContext context) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            // Make sure new task name exists
            string? taskName = context.Request.Form["name"];
            if (taskName is null) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("""Field "name" missing!""");
                return;
            }

            // Create new task object to be inserted in DB
            TodoTask task = new() {
                OwnerId = userId,
                StartDateUTC = DateTime.UtcNow,
                Name = taskName,
                Description = context.Request.Form["description"],
                Status = TodoTask.TaskStatus.InProgress,
            };
            
            // Add task to DB, and get its ID
            task.Id = await Database.CreateTaskAsync(task);

            // Return created task to client
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsJsonAsync(task);

            if (Program.DEBUG) Console.WriteLine($"Added task for UserId: {task.OwnerId} - ({task.Name})");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            if (ex is TaskNameInUseException) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            } else {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            
            await context.Response.WriteAsync(ex.Message);
        }
    }

    public static async Task DeleteTask(HttpContext context, int taskId) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            // Delete task from DB
            await Database.DeleteTaskAsync(userId, taskId);

            context.Response.StatusCode = StatusCodes.Status200OK;
            if (Program.DEBUG) Console.WriteLine($"User:{userId} removed task:{taskId}");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    // Resets endDate increments times_completed and sets task status to 0 = In Progress
    public static async Task ResetTask(HttpContext context, int taskId) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            // Get user ID from sessions list
            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            TodoTask task = new() {
                Id = taskId,
                OwnerId = userId,
                Status = TodoTask.TaskStatus.InProgress,
                EndDateUTC = null
            };

            // Use boolean to increment timesCompleted
            await Database.UpdateTaskAsync(task, true);

            context.Response.StatusCode = StatusCodes.Status200OK;
            if (Program.DEBUG) Console.WriteLine($"User:{userId} updated task:{taskId}");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }
    public static async Task UpdateTask(HttpContext context, int taskId) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            TodoTask? task = await context.Request.ReadFromJsonAsync<TodoTask>();
            if (task is null) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Unable to parse data from body");
                return;
            }

            task.Id = taskId;
            task.OwnerId = userId;

            bool updateTimesCompleted = false;
            if (task.Status == TodoTask.TaskStatus.Done) {
                updateTimesCompleted = true;
                task.EndDateUTC = DateTime.UtcNow;
            }


            // Update task in DB
            await Database.UpdateTaskAsync(task, updateTimesCompleted);

            context.Response.StatusCode = StatusCodes.Status200OK;
            if (Program.DEBUG) Console.WriteLine($"User:{userId} updated task:{taskId}");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    public static async Task GetTasks(HttpContext context, string? filter, int lastTaskId = 0, bool decend = false) {
        try {
            if (!await SessionManager.Authorized(context)) return;
            
            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));
            // Get tasks from DB with parameters: filter, lastTaskId
            // LastTaskId can be used to fetch next 10 available tasks, since the limit is set to 10 tasks only per fetch
            List<TodoTask> tasks = await Database.GetTasksAsync(userId, filter, lastTaskId, decend);

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(tasks);
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }    
    }
    public static async Task GetTask(HttpContext context, int taskId = 0) {
        try {
            if (!await SessionManager.Authorized(context)) return;
            
            // Get useId from SessionManager
            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            // Get single task from DB
            TodoTask? task = await Database.GetTaskAsync(userId, taskId);

            if (task is null) {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(task);
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }    
    }
}