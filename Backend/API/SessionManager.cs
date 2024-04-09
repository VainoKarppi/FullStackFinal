


// Session manager is used to check authentication for the client.
// This way we dont have to recheck the user authentication from the datbase everytime user does something.
// On client removal Session Token needs to be cleared from the list


using System.Text.Json;

namespace Backend;

public static class SessionManager {
    public const int Timeout = 5*60; // In seconds

    // First element in object array is dateTime when token expires.
    // Seconds element in object array is the 
    private static readonly Dictionary<Guid,(int, DateTime)> Sessions = [];
    private static List<Guid> DevSessions = [Guid.Parse(Program.Configuration.GetValue<string>("DevToken")!)];

    public static int GetUserIdByGuid(Guid guid) {
        return Sessions[guid].Item1;
    }
    public static void AddSession(int userId, Guid guid, bool devSession = false) {
        Sessions.Add(guid,(userId,DateTime.Now));
        if (devSession) DevSessions.Add(guid);
    }

    public static bool RemoveSession(Guid guid) {
        if (DevSessions.Contains(guid)) return true; // Dont remove CLI generated tokens
        return Sessions.Remove(guid);
    }
    public static void ValidateSessionToken(Guid sessionGuid, int? userId) {
    
        // Return true for dev sessions
        if (DevSessions.Contains(sessionGuid)) return;

        // Throws error if not found
        // Get by user id and by token
        KeyValuePair<Guid,(int,DateTime)> pair = Sessions.First(x => x.Key == sessionGuid);

        // Make sure user requester ID matches
        if (userId is not null && pair.Value.Item1 != userId) throw new InvalidUserIdException();

        // Check session timer. Returns true if SESSION IS VALID
        if (DateTime.Now > pair.Value.Item2.AddSeconds(Timeout))  {
            // Remove from session storage
            RemoveSession(sessionGuid);
            throw new TokenExpiredException(); // Token has expired!
        }
    }

    public static string GetTokenFromHeader(IHeaderDictionary headers) {
        headers.TryGetValue("Authorization", out var sessionToken);

        string? bearerToken = sessionToken.ToString().Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(bearerToken)) throw new TokenNotFoundException();

        return bearerToken;
    }

    // Use usedId to allow only to give access for the specific client only
    // Eg. /tasks/create/{userId}
    public static async Task<bool> Authorized(HttpContext context, int? userId = null) {
        try {
            // Check if session token is provided in request header
            string bearerToken = GetTokenFromHeader(context.Request.Headers);
            
            Guid sessionGuid = Guid.Parse(bearerToken);
            
            // Make sure bearer token is in use! (List of active tokens for active users)
            ValidateSessionToken(sessionGuid, userId);

            // Update session timeout
            if (!DevSessions.Contains(sessionGuid))
                Sessions[sessionGuid] = (Sessions[sessionGuid].Item1, DateTime.Now);

            return true;
        } catch (Exception ex) {
            // Write response Exceptions
            if (ex is InvalidUserIdException) {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid userId");
                return false;
            }
            if (ex is InvalidOperationException || ex is ArgumentNullException) {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token");
                return false;
            }
            if (ex is TokenNotFoundException) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Bearer token not found");
                return false;
            }
            if (ex is TokenExpiredException) {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token has expired");
                return false;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(ex.Message);
            
            return false;
        }
    }
}