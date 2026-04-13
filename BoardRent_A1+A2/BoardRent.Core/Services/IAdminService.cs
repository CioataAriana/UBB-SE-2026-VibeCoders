namespace BoardRent.Services
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Utils;

    public interface IAdminService
    {
        Task<ServiceResult<PaginatedResult<UserProfileDataTransferObject>>> GetAllUsersAsync(
            int pageNumber,
            int pageSize);

        Task<ServiceResult<bool>> SuspendUserAsync(Guid userIdentifier);

        Task<ServiceResult<bool>> UnsuspendUserAsync(Guid userIdentifier);

        Task<ServiceResult<bool>> ResetPasswordAsync(Guid userIdentifier, string newPassword);

        Task<ServiceResult<bool>> UnlockAccountAsync(Guid userIdentifier);
    }
}