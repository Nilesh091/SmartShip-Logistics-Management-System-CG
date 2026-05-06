using System;

namespace AuthService.Application.Exceptions
{
  public class AuthServiceException : Exception
  {
    public AuthServiceException(string message) : base(message)
    {
    }

    public AuthServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}