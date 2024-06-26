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
    public static bool DEBUG = Configuration.GetValue<bool>("Debug");
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

            // Add CORS
            builder.Services.AddCors();

            var app = builder.Build();

            // Add CORS policies
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            //! ADD METHODS
            // Tasks Methods
            app.MapPatch("/tasks/update/{taskId}", ApiMethods.UpdateTask);
            app.MapDelete("/tasks/delete/{taskId}", ApiMethods.DeleteTask);
            app.MapPost("/tasks/create", ApiMethods.CreateTask);
            app.MapGet("/tasks", ApiMethods.GetTasks);
            app.MapGet("/tasks/{taskId}", ApiMethods.GetTask);

            // Activity methods
            app.MapGet("/activities", ApiMethods.GetActivities);
            app.MapPost("/activities/create", ApiMethods.CreateActivity);
            app.MapPatch("/activities/update/{activityId}", ApiMethods.UpdateActivity);
            app.MapPatch("/activities/reset/{activityId}", ApiMethods.ResetActivity);
            app.MapDelete("/activities/delete/{activityId}", ApiMethods.DeleteActivity);
            
            // User Methods
            app.MapPost("/register", ApiMethods.Register);
            app.MapPost("/login", ApiMethods.Login);
            app.MapPost("/logout", ApiMethods.Logout);
            app.MapPatch("/user/update", ApiMethods.UpdateUser);
            app.MapDelete("/user/delete", ApiMethods.DeleteUser);

            app.MapGet("/statistics", ApiMethods.GetStatistics);

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

            //Console.Beep();

            await Task.Delay(1000);
            Console.WriteLine("STARTED API SERVER");

            new Thread(() => {
                Console.WriteLine("\n\n");
                while (true) {
                    string? input = Console.ReadLine()!.ToLower().Trim();;
                    if (input == "exit") break;
                    if (input == "addtesttoken") {
                        Guid guid = Guid.NewGuid();
                        SessionManager.AddSession(999999, "DEV", guid, true); // Add DEV token with all permissions
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