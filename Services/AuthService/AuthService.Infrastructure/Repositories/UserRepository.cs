using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
  public class UserRepository : IUserRepository
  {
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
      _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
      return await _context.Users
          .Include(u => u.RefreshTokens)
          .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
      await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }
  }
}
