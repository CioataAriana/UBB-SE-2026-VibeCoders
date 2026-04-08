using System.Threading.Tasks;
using BoardRent.DTOs;
using BoardRent.Utils;

namespace BoardRent.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto);

        Task<ServiceResult<UserProfileDto>> LoginAsync(LoginDto dto);

        Task<ServiceResult<bool>> LogoutAsync();

        Task<ServiceResult<string>> ForgotPasswordAsync();

    }
}


