using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Backend;
public static class ApiMethods {
    public static async Task Home(HttpContext context) {
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync("API WORKING");
    }
    public static async Task Login(HttpContext context) {
        // Read username and password from request
        string? username = context.Request.Form["username"];
        string? password = context.Request.Form["password"];

        // Makme sure username and password exists
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid credientials input");
            return;
        }

        // Get user from Database if exsists. Returns null if not found!
        User? user = await Database.GetUserAsync(username, password);

        // Check if Unauthorized. Username or password is incorrect
        if (user is null) {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Generate and add session token.
        // Session token can be used up to 30 seconds before needing to renew and rechecking from database!
        // This way we can lower the amount of user authorization checks
        var sessionToken = Guid.NewGuid();
        SessionManager.AddSession((int)user.Id!, sessionToken);


        // Return User in response with Token as parameter
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(new {
            user.Id,
            user.Username,
            user.LastLoginUTC,
            sessionToken,
            TokenExpirationUTC = DateTime.UtcNow.AddSeconds(SessionManager.Timeout)
        });

        Console.WriteLine($"User Logged in! {user.Id} ({user.Username})");

        // Update LastLogin Data. Fire and Forget
        user.LastLoginUTC = DateTime.UtcNow;
        await Database.UpdateUserAsync(user,true);
    }
    public static void Logout(HttpContext context) {
        
        // Remove session token from sessions
        string bearerToken = SessionManager.GetTokenFromHeader(context.Request.Headers);
        SessionManager.RemoveSession(Guid.Parse(bearerToken));

        Console.WriteLine($"User Logged out! {bearerToken}");
        // Let frontend handle the auto page forwarding
        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }
    public static async Task Register(HttpContext context) {
        try {
            // Read username and password from request
            string username = context.Request.Form["username"]!;
            string password = context.Request.Form["password"]!;

            // Check if username or password is missing
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Username or password missing from request form");
                return;
            }


            // Create User To Database
            var user = new User(username, password);
            user.Id = await Database.AddUserAsync(user);


            // Add auto generated session token to session manager.
            // Session token can be used up to 30 seconds before needing to renew and rechecking from database!
            // This way we can lower the amount of user authorization checks from DB
            SessionManager.AddSession((int)user.Id!, (Guid)user.SessionToken!);

            user.Password = null; // Dont resend password hash back

            // Return User in response with Token as parameter
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsJsonAsync(new {
                user.Id,
                user.Username,
                user.LastLoginUTC,
                user.SessionToken,
                TokenExpirationUTC = DateTime.UtcNow.AddSeconds(SessionManager.Timeout)
            });
            Console.WriteLine($"Succesfully created account: {user.Id} ({user.Username})");
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            if (ex is ArgumentException) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Username of max 64 chars is only allowed");
                return;
            }
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }


    

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

    public static async Task GetTasks(HttpContext context, int userId, int lastTaskId = 0) {
        try {
            if (!await SessionManager.Authorized(context, userId)) return;
            
            List<TodoTask> tasks = await Database.GetTasksAsync(userId, lastTaskId);
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