namespace BoardRent.Services
{
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Utils;

    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest);

        Task<ServiceResult<UserProfileDataTransferObject>> LoginAsync(LoginDataTransferObject loginRequest);

        Task<ServiceResult<bool>> LogoutAsync();

        Task<ServiceResult<string>> ForgotPasswordAsync();
    }
}