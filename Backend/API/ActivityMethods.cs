using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Backend;
public static partial class ApiMethods {
    public static async Task CreateActivity(HttpContext context) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            // Read username and password from request
            string? name = context.Request.Form["name"];
            string? description = context.Request.Form["description"];
            string? tasks = context.Request.Form["tasks"];
            string? due = context.Request.Form["due"];

            // Convert ids to int array
            int[] taskIds = tasks!.Split(',').Select(int.Parse).ToArray() ?? [];

            bool dateParsed = DateTime.TryParse(due, out DateTime dueTime);

            Activity activity = new() {
                Name = name,
                OwnerId = userId,
                Description = description,
                TaskIds = taskIds,
                Status = Activity.ActivityStatus.InProgress,
                StartDateUTC = DateTime.UtcNow,
                DueDateUTC = dateParsed ? dueTime : null,
            };

            // Create activity to DB
            activity.Id = await Database.CreateActivityAsync(activity);

            // Return created task to client
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsJsonAsync(activity);
            
            if (Program.DEBUG) Console.WriteLine($"User:{userId} create activity:{activity.Id}");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    public static async Task DeleteActivity(HttpContext context, int activityId) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            // Delete activity from DB
            await Database.DeleteActivityAsync(userId, activityId);

            context.Response.StatusCode = StatusCodes.Status200OK;
            if (Program.DEBUG) Console.WriteLine($"User:{userId} removed activity:{activityId}");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    public static async Task UpdateActivity(HttpContext context, int activityId) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            Activity? activity = await context.Request.ReadFromJsonAsync<Activity>();
            if (activity is null) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Unable to parse data from body");
                return;
            }

            activity.Id = activityId;
            activity.OwnerId = userId;

            bool updateTimesCompleted = false;
            if (activity.Status == Activity.ActivityStatus.Done) {
                updateTimesCompleted = true;
                activity.EndDateUTC = DateTime.UtcNow;
            }

            // Update task in DB
            // Increment times completed
            await Database.UpdateActivityAsync(activity, updateTimesCompleted);

            context.Response.StatusCode = StatusCodes.Status200OK;
            if (Program.DEBUG) Console.WriteLine($"User:{userId} updated activity:{activityId}");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    public static async Task ResetActivity(HttpContext context, int activityId) {
        try {
            // Check authentication. Returns error code automatically, if not authenticated
            if (!await SessionManager.Authorized(context)) return;

            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            Activity activity = new() {
                Id = activityId,
                OwnerId = userId,
                DueDateUTC = DateTime.UtcNow.AddDays(7),
                StartDateUTC = DateTime.UtcNow
            };
            
            // Set enddate=null, status = inProgress, Duedate = 1 week
            // TODO add dueDate as a variable
            await Database.ResetActivityAsync(activity);

            context.Response.StatusCode = StatusCodes.Status200OK;
            if (Program.DEBUG) Console.WriteLine($"User:{userId} updated activity:{activityId}");
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    public static async Task GetActivities(HttpContext context, string? filter) {
        try {
            if (!await SessionManager.Authorized(context)) return;
            
            int userId = SessionManager.GetUserIdByGuid(SessionManager.GetTokenFromHeader(context.Request.Headers));

            List<Activity> activities = await Database.GetActivitiesAsync(userId, filter);

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(activities);
        } catch (Exception ex) {
            // Return error to client
            if (Program.DEBUG) Console.WriteLine(ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        } 
    }
}