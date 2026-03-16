using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models.DTOs;

namespace WebApplication5.Services.Interfaces
{
    public interface IFriendService
    {
        Task<ApiResponse> SendRequest(int senderId, int receiverId);
        Task<List<FriendRequestDto>> GetPendingRequests(int userId);
        Task<ApiResponse> AcceptRequest(int userId, int requestId);
        Task<ApiResponse> RejectRequest(int userId, int requestId);
        Task<List<FriendDto>> GetFriends(int userId);
    }
}
