using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Backend;
public static partial class ApiMethods {
    public static async Task Home(HttpContext context) {
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync("API WORKING");
    }
    

}