using System.Text.Json;

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
        SessionManager.AddSession(sessionToken);


        // Return User in response with Token as parameter
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(new {user.Id, user.Username, user.LastLogin, sessionToken});

        // Update LastLogin Data. Fire and Forget
        user.LastLogin = DateTime.Now;
        await Database.UpdateUserAsync(user,true);
    }

    public static void Logout(HttpContext context) {

        string bearerToken = SessionManager.GetTokenFromHeader(context.Request.Headers);
        SessionManager.RemoveSession(Guid.Parse(bearerToken));

        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }


    public static async Task Register(HttpContext context) {
        try {
            // Read username and password from request
            string username = context.Request.Form["username"]!;
            string password = context.Request.Form["password"]!;

            // Get user from Database if exsists. Returns null if not found!
            var user = new User(username, password);
            user.Id = await Database.AddUserAsync(user);


            // Add auto generated session token to session manager.
            // Session token can be used up to 30 seconds before needing to renew and rechecking from database!
            // This way we can lower the amount of user authorization checks from DB
            SessionManager.AddSession((Guid)user.SessionToken!);

            user.PasswordHash = null; // Dont resend password hash back

            // Return User in response with Token as parameter
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsJsonAsync(new {user.Id, user.Username, user.LastLogin, user.SessionToken});
        } catch (Exception ex) {
            Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }


    public static async void GetTasks(HttpContext context, int userId) {
        // Ensure the user is authenticated
        if (!context.User.Identity!.IsAuthenticated) {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        List<TodoTask> tasks = await Database.GetTasksAsync(userId);
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(JsonSerializer.Serialize(tasks));
    }

    public static async void CreateTask(HttpContext context, int userId) {
        var username = context.Request.Form["username"];
        var passwordHash = context.Request.Form["password"];

        TodoTask task = await Database.CreateTaskAsync(userId);
        context.Response.StatusCode = StatusCodes.Status201Created;
        await context.Response.WriteAsJsonAsync(JsonSerializer.Serialize(task));
    }
}