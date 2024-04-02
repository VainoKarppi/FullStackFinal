using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;



internal class Program
{
    public static IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthentication();

        var app = builder.Build();


        app.MapPost("/register", async (context) =>
        {
            try
            {

                // Read username and password from request
                var username = context.Request.Form["username"];
                var passwordHash = context.Request.Form["password"];

                // Get user from Database if exsists. Returns null if not found!
                var user = new User(username, passwordHash);
                int id = await Database.AddUser(user);
                user.Id = id;

                // Check if Unauthorized. Username or password is incorrect
                if (user is null)
                {
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
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsJsonAsync(JsonSerializer.Serialize(user));

                // Update LastLogin Data.
                Database.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(ex.Message);
            }
        });

        app.MapPost("/login", async (context) =>
        {
            // Read username and password from request
            var username = context.Request.Form["username"];
            var passwordHash = context.Request.Form["password"];

            // Get user from Database if exsists. Returns null if not found!
            User? user = await Database.GetUserAsync(username, passwordHash);

            // Check if Unauthorized. Username or password is incorrect
            if (user is null)
            {
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
        });

        // Endpoint to access protected resource using session token
        app.MapGet("/protected", async (context) =>
        {
            // Check if session token is provided in request header
            context.Request.Headers.TryGetValue("Authorization", out var sessionToken);
            if (!string.IsNullOrEmpty(sessionToken) && SessionManager.IsTokenSessionValid(sessionToken))
            {
                // Authorized, return the protected resource
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Welcome to the protected resource!");
            }
            else
            {
                // Unauthorized if session token is missing or invalid
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
        });

        app.MapGet("/", () => "Hello World!");






        app.Run();
    }
}