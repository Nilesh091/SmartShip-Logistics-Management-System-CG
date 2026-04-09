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

    public async Task<User?> GetByIdAsync(Guid id)
    {
      return await _context.Users
          .Include(u => u.RefreshTokens)
          .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<User>> GetAllAsync()
    {
      return await _context.Users.ToListAsync();
    }

    public async Task AddAsync(User user)
    {
      await _context.Users.AddAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
      _context.Users.Update(user);
    }

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }
  }
}

