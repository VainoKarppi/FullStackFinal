


// Session manager is used to check authentication for the client.
// This way we dont have to recheck the user authentication from the datbase everytime user does something.
// On client removal Session Token needs to be cleared from the list

public static class SessionManager {

    // In seconds
    private const int Timeout = 30;
    private static Dictionary<Guid,DateTime> Sessions = new() {};

    public static void AddSession(Guid guid) {
        Sessions.Add(guid,DateTime.Now);
    }
    public static bool IsTokenSessionValid(string session) {
        try {
            Guid guid = Guid.Parse(session);

            // Throws error if not found
            KeyValuePair<Guid,DateTime> pair = Sessions.First(x => x.Key == guid);

            // Check session timer
            if (DateTime.Now < pair.Value.AddSeconds(Timeout)) return true;

            // Remove from session storage
            Sessions.Remove(guid);

            // Token expired. Recheck user authentication from Database!
            return false;
        } catch (Exception) {
            return false;
        }
        
    }
}