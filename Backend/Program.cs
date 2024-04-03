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



internal class Program {
    public static IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    private static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        Database.ConnectToDatabase();

        var app = builder.Build();

        // Add Methods
        app.MapPost("/task/create/{userId}", ApiMethods.CreateTask);
        app.MapGet("/tasks/{userId}", ApiMethods.GetTasks);
        app.MapPost("/register", ApiMethods.Register);
        app.MapPost("/login", ApiMethods.Login);
        app.MapGet("/", ApiMethods.Home);

        // Endpoint to access protected resource using session token
        app.MapGet("/protected", async (context) =>
        {
            // Check if session token is provided in request header
            context.Request.Headers.TryGetValue("Authorization", out var sessionToken);
            if (!string.IsNullOrEmpty(sessionToken) && SessionManager.IsTokenSessionValid(sessionToken!))
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







        app.Run();
    }
}