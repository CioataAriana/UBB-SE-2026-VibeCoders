using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoardRent.DTOs;
using BoardRent.Utils;

namespace BoardRent.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserProfileDto>> GetProfileAsync(Guid userId);
        Task<ServiceResult<bool>> UpdateProfileAsync(Guid userId, UserProfileDto dto);
        Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<string> UploadAvatarAsync(Guid userId, string filePath); 
        Task RemoveAvatarAsync(Guid userId);
    }
}
