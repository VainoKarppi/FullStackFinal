

[Serializable]
internal class UsernameInUseException : Exception {
    public UsernameInUseException() {}
    public UsernameInUseException(string? message) : base(message) {}
    public UsernameInUseException(string? message, Exception? innerException) : base(message, innerException) {}
}

internal class TaskNameInUseException : Exception{
    public TaskNameInUseException() {}
    public TaskNameInUseException(string? message) : base(message) {}
    public TaskNameInUseException(string? message, Exception? innerException) : base(message, innerException) {}
}

internal class ActivityNameInUseException : Exception{
    public ActivityNameInUseException() {}
    public ActivityNameInUseException(string? message) : base(message) {}
    public ActivityNameInUseException(string? message, Exception? innerException) : base(message, innerException) {}
}