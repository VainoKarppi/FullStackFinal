


// Session manager is used to check authentication for the client.
// This way we dont have to recheck the user authentication from the datbase everytime user does something.
// On client removal Session Token needs to be cleared from the list


namespace Backend;

public static class SessionManager {

    // In minutes
    private const int Timeout = 5;

    // First element in object array is dateTime when token expires.
    // Seconds element in object array is the 
    private static readonly Dictionary<Guid,DateTime> Sessions = [];

    public static void AddSession(Guid guid) {
        Sessions.Add(guid,DateTime.Now);
    }

    public static bool RemoveSession(Guid guid) {
        return Sessions.Remove(guid);
    }
    public static bool IsTokenSessionValid(string sessionGuid) {
        try {
            // TODO Add another timer to validate user from DB every X minutes

            Guid guid = Guid.Parse(sessionGuid);

            // Throws error if not found
            // Get by user id and by token
            KeyValuePair<Guid,DateTime> pair = Sessions.First(x => x.Key == guid);

            // Check session timer
            if (DateTime.Now < pair.Value.AddMinutes(Timeout)) return true;

            // Remove from session storage
            Sessions.Remove(guid);

            // Token expired. Recheck user authentication from Database!
            return false;
        } catch (Exception) {
            return false;
        }
        
    }

    public static string GetTokenFromHeader(IHeaderDictionary headers) {
        headers.TryGetValue("Authorization", out var sessionToken);

        string? bearerToken = sessionToken.ToString().Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(bearerToken)) throw new KeyNotFoundException("No Bearer token found for header!");

        return bearerToken;
    }

    public static async Task<bool> Authorized(HttpContext context) {
        try {
            // Check if session token is provided in request header
            string bearerToken = GetTokenFromHeader(context.Request.Headers);
            

            // Make sure bearer token is in use! (List of active tokens)
            if (!IsTokenSessionValid(bearerToken)) {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Please login first!");
                return false;
            }

            // Update session timeout
            Sessions[Guid.Parse(bearerToken)] = DateTime.Now;

            return true;
        } catch (Exception ex) {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            // Write response Exception
            if (ex is KeyNotFoundException)
                await context.Response.WriteAsync("Bearer token not found!");
            else
                await context.Response.WriteAsync(ex.Message);

            return false;
        }
    }
}