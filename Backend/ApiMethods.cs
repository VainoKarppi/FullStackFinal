using System.Text.Json;


public static class ApiMethods {
    public static async void Home(HttpContext context) {

    }
    public static async void Login(HttpContext context) {
        // Read username and password from request
        var username = context.Request.Form["username"];
        var passwordHash = context.Request.Form["password"];

        // Get user from Database if exsists. Returns null if not found!
        User? user = await Database.GetUserAsync(username!, passwordHash!);

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
        user.SessionToken = sessionToken;


        user.LastLogin = DateTime.Now;
        user.PasswordHash = null; // Dont resend password hash back

        // Return User in response with Token as parameter
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(JsonSerializer.Serialize(user));

        // Update LastLogin Data.
        Database.UpdateUserAsync(user);
    }


    public static async void Register(HttpContext context) {
        try {
            // Read username and password from request
            var username = context.Request.Form["username"];
            var passwordHash = context.Request.Form["password"];

            // Get user from Database if exsists. Returns null if not found!
            var user = new User(username!, passwordHash!);
            int id = await Database.AddUserAsync(user);
            user.Id = id;

            // Check if Unauthorized. Username or password is incorrect
            if (user is null) {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            // Generate and add session token.
            // Session token can be used up to 30 seconds before needing to renew and rechecking from database!
            // This way we can lower the amount of user authorization checks
            var sessionToken = Guid.NewGuid();
            SessionManager.AddSession(sessionToken);
            user.SessionToken = sessionToken;


            user.LastLogin = DateTime.Now;
            user.PasswordHash = null; // Dont resend password hash back

            // Return User in response with Token as parameter
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsJsonAsync(JsonSerializer.Serialize(user));

            // Update LastLogin Data.
            Database.UpdateUserAsync(user);
        }
        catch (Exception ex)
        {
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