



public static class SessionManager {

    // In seconds
    private const int Timeout = 30;
    private static Dictionary<Guid,DateTime> Session = new() {};

    public static void AddSession(Guid guid) {
        Session.Add(guid,DateTime.Now);
    }
    public static bool IsTokenSessionValid(string session) {
        try {
            Guid guid = Guid.Parse(session);

            // Throws error if not found
            KeyValuePair<Guid,DateTime> pair = Session.First(x => x.Key == guid);

            // Check session timer
            if (DateTime.Now < pair.Value.AddSeconds(Timeout)) return true;

            // Remove from session storage
            Session.Remove(guid);

            // Token expired. Recheck user authentication from Database!
            return false;
        } catch (Exception) {
            return false;
        }
        
    }
}