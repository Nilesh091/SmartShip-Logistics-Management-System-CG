using System;

namespace AuthService.Application.Exceptions
{
  public class GoogleOAuthException : Exception
  {
    public GoogleOAuthException(string message) : base(message)
    {
    }

    public GoogleOAuthException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}