using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IOtpCodeRepository
    {
        Task AddAsync(OtpCode otpCode);
        Task<OtpCode?> GetByEmailAndCodeAsync(string email, string code);
        Task<OtpCode?> GetByEmailAsync(string email);
        Task DeleteAsync(OtpCode otpCode);
        Task SaveChangesAsync();
    }
}
