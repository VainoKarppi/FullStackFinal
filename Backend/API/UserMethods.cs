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

        // Check if username is already used in tokens. Else check from DB
        Guid? sessionToken = SessionManager.GetGuidByUsername(username);
        if (sessionToken is not null) {
            SessionManager.UpdateSession((Guid)sessionToken);
            context.Response.StatusCode = StatusCodes.Status208AlreadyReported;
            await context.Response.WriteAsJsonAsync(new {
                Message = "Already logged in",
                sessionToken,
                TokenExpirationUTC = DateTime.UtcNow.AddSeconds(SessionManager.Timeout)
            });
            return;
        }

        // Get user from Database if exsists. Returns null if not found!
        User? user = await Database.GetUserAsync(username, password);

        // Check if Unauthorized. Username or password is incorrect
        if (user is null) {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid credientials");
            return;
        }

        // Generate new session token.
        Guid newSessionToken = Guid.NewGuid();
        SessionManager.AddSession((int)user.Id!, username, newSessionToken);
    

        // Return User in response with Token as parameter
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(new {
            user.Id,
            user.Username,
            user.LastLoginUTC,
            sessionToken = newSessionToken,
            TokenExpirationUTC = DateTime.UtcNow.AddSeconds(SessionManager.Timeout)
        });

        Console.WriteLine($"User Logged in! {user.Id} ({user.Username})");

        // Update LastLogin Data. Fire and Forget
        user.LastLoginUTC = DateTime.UtcNow;
        user.Password = null; // Dont update password
        user.Username = null; // Dont update username
        await Database.UpdateUserAsync(user,true);
    }
    public static async Task Logout(HttpContext context) {
        try {
            if (!await SessionManager.Authorized(context)) return;

            // Remove session token from sessions
            Guid token = SessionManager.GetTokenFromHeader(context.Request.Headers);
            SessionManager.RemoveSession(token);

            if (Program.DEBUG) Console.WriteLine($"User Logged out! {token}");
            // Let frontend handle the auto page forwarding
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        } catch (Exception ex) {
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
    public static async Task DeleteUser(HttpContext context) {
        try {
            if (!await SessionManager.Authorized(context)) return;

            Guid token = SessionManager.GetTokenFromHeader(context.Request.Headers);

            // Get user ID from sessions list
            int userId = SessionManager.GetUserIdByGuid(token);

            // Remove user from Database
            await Database.DeleteUserAsync(userId);

            // Remove token from sessions
            SessionManager.RemoveSession(token);

            context.Response.StatusCode = StatusCodes.Status200OK;

            if (Program.DEBUG) Console.WriteLine($"Removed user! ID:{userId}");
        } catch (Exception ex) {
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
    public static async Task UpdateUser(HttpContext context) {
        try {
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            // Read new userdata from body
            User? userData = await context.Request.ReadFromJsonAsync<User?>();
            if (userData is null) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Unable to parse data from body");
                return;
            }
            
            // Create new user object which we add the details
            User user = new() {
                Id = userId,
                Username = userData.Username,
                Password = userData.Password
            };
            // Update database user data
            await Database.UpdateUserAsync(user);
            
            context.Response.StatusCode = StatusCodes.Status204NoContent;

            if (Program.DEBUG) Console.WriteLine($"Updated user: {userId}");
        } catch (Exception ex) {
            if (Program.DEBUG) Console.WriteLine(ex);
            if (ex is UsernameInUseException) {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsync("Username already in use");
                return;
            }
            await context.Response.WriteAsync(ex.Message);
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

            // Check if username and passwords are long enough
            if (username.Length < 5) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Username too short! (Must be at least 5 characters)");
                return;
            }
            if (password.Length < 8) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Password too short! (Must be at least 8 characters)");
                return;
            }

            // Create User To Database
            var user = new User(username, password);
            user.Id = await Database.AddUserAsync(user);


            // Add auto generated session token to session manager.
            // Session token can be used up to 30 seconds before needing to renew and rechecking from database!
            // This way we can lower the amount of user authorization checks from DB
            SessionManager.AddSession((int)user.Id!, username, (Guid)user.SessionToken!);

            // Return User in response with Token as parameter
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsJsonAsync(new {
                user.Id,
                user.Username,
                user.LastLoginUTC,
                user.SessionToken,
                TokenExpirationUTC = DateTime.UtcNow.AddSeconds(SessionManager.Timeout)
            });
            if (Program.DEBUG) Console.WriteLine($"Created new account. ID:{user.Id}, NAME:{user.Username}");
        } catch (Exception ex) {
            if (Program.DEBUG) Console.WriteLine(ex);
            if (ex is UsernameInUseException) {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsync("Username already in use");
                return;
            }
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