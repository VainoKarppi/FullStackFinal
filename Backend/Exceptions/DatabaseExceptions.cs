

[Serializable]
internal class UsernameInUseException : Exception
{
    public UsernameInUseException()
    {
    }

    public UsernameInUseException(string? message) : base(message)
    {
    }

    public UsernameInUseException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}