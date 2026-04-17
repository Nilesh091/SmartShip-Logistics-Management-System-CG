using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class OtpCodeRepository : IOtpCodeRepository
    {
        private readonly AuthDbContext _context;

        public OtpCodeRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OtpCode otpCode)
        {
            await _context.OtpCodes.AddAsync(otpCode);
        }

        public async Task<OtpCode?> GetByEmailAndCodeAsync(string email, string code)
        {
            return await _context.OtpCodes
                .FirstOrDefaultAsync(o => o.Email == email && o.Code == code);
        }

        public async Task<OtpCode?> GetByEmailAsync(string email)
        {
            return await _context.OtpCodes
                .Where(o => o.Email == email)
                .OrderByDescending(o => o.ExpiryTime)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(OtpCode otpCode)
        {
            _context.OtpCodes.Remove(otpCode);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
