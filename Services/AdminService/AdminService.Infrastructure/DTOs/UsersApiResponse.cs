using System;
using System.Collections.Generic;

namespace AdminService.Infrastructure.DTOs
{
  public class UsersApiResponse
  {
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<UserDto> Data { get; set; }
  }
}
