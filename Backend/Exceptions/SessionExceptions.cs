

[Serializable]
internal class InvalidUserIdException : Exception {
    public InvalidUserIdException() {}
    public InvalidUserIdException(string? message) : base(message) {}
    public InvalidUserIdException(string? message, Exception? innerException) : base(message, innerException) {}
}



internal class TokenExpiredException : Exception{
    public TokenExpiredException() {}
    public TokenExpiredException(string? message) : base(message) {}
    public TokenExpiredException(string? message, Exception? innerException) : base(message, innerException) {}
}

internal class TokenNotFoundException : Exception{
    public TokenNotFoundException() {}
    public TokenNotFoundException(string? message) : base(message) {}
    public TokenNotFoundException(string? message, Exception? innerException) : base(message, innerException) {}
}