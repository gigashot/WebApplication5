using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models.DTOs;

namespace WebApplication5.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserSearchResultDto>> Search(string query, int excludeUserId);
        Task<LoginResponseDto> GetProfile(int userId);
    }
}
