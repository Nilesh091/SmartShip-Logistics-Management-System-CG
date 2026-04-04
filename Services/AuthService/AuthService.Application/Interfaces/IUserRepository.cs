using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
  public interface IUserRepository
  {
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();
  }
}
