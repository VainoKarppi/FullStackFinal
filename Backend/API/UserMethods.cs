using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Backend;
public static partial class ApiMethods {

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
        try {
            // Remove session token from sessions
            string bearerToken = SessionManager.GetTokenFromHeader(context.Request.Headers);
            bool removed = SessionManager.RemoveSession(Guid.Parse(bearerToken));
            if (!removed) throw new Exception("Invalid session token");

            Console.WriteLine($"User Logged out! {bearerToken}");
            // Let frontend handle the auto page forwarding
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        } catch (Exception) {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }

    public static async Task UpdateUser(HttpContext context, int userId) {
        try {
            if (!await SessionManager.Authorized(context, userId)) return;

            string? username = context.Request.Form["username"];
            string? password = context.Request.Form["password"];

            User user = new() {
                Id = userId,
                Username = username,
                Password = password
            };
            // Update database user data
            await Database.UpdateUserAsync(user);
            
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        } catch (Exception ex) {
            Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

    }
    public static async Task Register(HttpContext context) {
        try {
            // Read username and password from request
            string? username = context.Request.Form["username"];
            string? password = context.Request.Form["password"];

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


 
}