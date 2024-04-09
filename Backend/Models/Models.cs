
namespace Backend;

public enum LogCodes {
    UserCreated,
    UserUpdated,
    UserRemoved,
    TaskCreated,
    TaskUpdated,
    TaskRemoved,
}
public class TodoTask() {
    
    public enum TaskStatus {
        InProgress,
        Done,
        Cancelled
    }

    public int Id { get; set; }
    public required int? OwnerId { get; set; }
    public required string? Name { get; set; }
    public string? Description { get; set; }
    public required DateTime StartDateUTC { get; set; }
    public DateTime? EndDateUTC { get; set; }
    public string[]? Tags { get; set; }
    public TaskStatus? Status { get; set; }
    public bool IsCompleted { get; set;} = false;
}

public class User {
    public int? Id { get; set; }
    public string? Username { get; set; }
    public DateTime? LastLoginUTC { get; set; }

    // Password can be either raw password, or hashed password!
    public string? Password { get; set; }
    public Guid? SessionToken { get; set; }
    public string? Salt { get; set; }

    public User() {} 
    public User(string username, string password) {
        Username = username;
        Password = password;
        LastLoginUTC = DateTime.Now;
        SessionToken = Guid.NewGuid();;
    }
}