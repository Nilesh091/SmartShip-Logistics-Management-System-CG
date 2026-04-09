using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
  public interface IUserRepository
  {
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
  }
}
