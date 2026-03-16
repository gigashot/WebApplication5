using System.Threading.Tasks;
using WebApplication5.Models.DTOs;

namespace WebApplication5.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse> Register(RegisterDto dto);
        Task<ApiResponse<LoginResponseDto>> Login(LoginDto dto);
    }
}
