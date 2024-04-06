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


namespace Backend;

internal class Program {
    public static IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    private static async Task Main(string[] args) {
        try {
            var builder = WebApplication.CreateBuilder(args);

            /*
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            */

            Console.Write("Connecting to Database...");
            Database.ConnectToDatabase();
            Console.WriteLine(" Connected!\n");

            var app = builder.Build();

            // Add Methods
            app.MapPost("/tasks/create/{userId}", ApiMethods.CreateTask);
            app.MapGet("/tasks/{userId}", ApiMethods.GetTasks);
            app.MapPost("/register", ApiMethods.Register);
            app.MapPost("/login", ApiMethods.Login);
            app.MapPost("/logout", ApiMethods.Logout);
            app.MapGet("/", ApiMethods.Home);

            // Endpoint to access protected resource using session token
            app.MapGet("/protected", async (context) => {

                // Make sure user is still authenticated by timer
                // Return the Http messages on its own
                if (!await SessionManager.Authorized(context)) return;

                // Authorized, return the protected resource
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Welcome to the protected resource!");
            });

            // Run server on background so we can continue by using CLI
            _ = Task.Run(() => app.Run());

            await Task.Delay(1000);
            Console.WriteLine("STARTED API SERVER");

            new Thread(() => {
                Console.WriteLine("\n\n");
                while (true) {
                    string? input = Console.ReadLine()!.ToLower().Trim();;
                    if (input == "exit") break;
                    if (input == "addtesttoken") {
                        Guid guid = Guid.NewGuid();
                        SessionManager.AddSession(guid);
                        Console.WriteLine($"Test Token is: {guid}");
                    }
                }
            }).Start();
           

        } catch (Exception ex) {
            Console.WriteLine("\n"+ ex.ToString());
            Console.WriteLine($"\n\n{ex.Message}\n");
        }
        
    }
}