using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
  public class RefreshTokenRepository : IRefreshTokenRepository
  {
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
      _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
      return await _context.RefreshTokens
          .Include(r => r.User)
          .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked);
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
      await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }
  }
}
