


namespace Backend;

public static partial class ApiMethods {
    public static async Task CreateTask(HttpContext context, int userId) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context, userId)) return;

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
        } catch (Exception ex) {
            // Return error to client
            Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    public static async Task GetTasks(HttpContext context, int userId, string? filter, int lastTaskId = 0) {
        try {
            if (!await SessionManager.Authorized(context, userId)) return;
            
            // Get tasks from DB with parameters: filter, lastTaskId
            // LastTaskId can be used to fetch next 10 available tasks, since the limit is set to 10 tasks only per fetch
            List<TodoTask> tasks = await Database.GetTasksAsync(userId, filter, lastTaskId);

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(tasks);
        } catch (Exception ex) {
            // Return error to client
            Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }    
    }
    public static async Task GetTask(HttpContext context, int userId, int taskId = 0) {
        try {
            if (!await SessionManager.Authorized(context, userId)) return;
            
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
            Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }    
    }
}