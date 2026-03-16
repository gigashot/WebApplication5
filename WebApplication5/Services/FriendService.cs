using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Models.DTOs;
using WebApplication5.Models.Entities;
using WebApplication5.Repositories;
using WebApplication5.Repositories.Interfaces;
using WebApplication5.Services.Interfaces;

namespace WebApplication5.Services
{
    public class FriendService : IFriendService
    {
        private readonly EncryptAppDbContext _context;
        private readonly IFriendRepository _friendRepository;
        private readonly IConnectionRepository _connectionRepository;

        public FriendService()
        {
            _context = new EncryptAppDbContext();
            _friendRepository = new FriendRepository(_context);
            _connectionRepository = new ConnectionRepository(_context);
        }

        public async Task<ApiResponse> SendRequest(int senderId, int receiverId)
        {
            if (senderId == receiverId)
            {
                return ApiResponse.Error("You cannot send a friend request to yourself.");
            }

            if (await _friendRepository.AreFriends(senderId, receiverId))
            {
                return ApiResponse.Error("You are already friends with this user.");
            }

            var duplicatePending = await _context.FriendRequests
                .AnyAsync(fr =>
                    ((fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                     (fr.SenderId == receiverId && fr.ReceiverId == senderId)) &&
                    fr.Status == "pending");

            if (duplicatePending)
            {
                return ApiResponse.Error("A pending friend request already exists.");
            }

            var friendRequest = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            return ApiResponse.Ok("Friend request sent.");
        }

        public async Task<List<FriendRequestDto>> GetPendingRequests(int userId)
        {
            var requests = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == userId && fr.Status == "pending")
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return requests.Select(fr => new FriendRequestDto
            {
                RequestId = fr.RequestId,
                SenderId = fr.SenderId,
                SenderUsername = fr.Sender.Username,
                Status = fr.Status,
                CreatedAt = fr.CreatedAt
            }).ToList();
        }

        public async Task<ApiResponse> AcceptRequest(int userId, int requestId)
        {
            var request = await _context.FriendRequests.FindAsync(requestId);

            if (request == null)
            {
                return ApiResponse.Error("Friend request not found.");
            }

            if (request.ReceiverId != userId)
            {
                return ApiResponse.Error("You are not authorized to accept this request.");
            }

            if (request.Status != "pending")
            {
                return ApiResponse.Error("This request has already been processed.");
            }

            request.Status = "accepted";
            await _context.SaveChangesAsync();

            await _friendRepository.CreateFriendship(request.SenderId, request.ReceiverId);

            return ApiResponse.Ok("Friend request accepted.");
        }

        public async Task<ApiResponse> RejectRequest(int userId, int requestId)
        {
            var request = await _context.FriendRequests.FindAsync(requestId);

            if (request == null)
            {
                return ApiResponse.Error("Friend request not found.");
            }

            if (request.ReceiverId != userId)
            {
                return ApiResponse.Error("You are not authorized to reject this request.");
            }

            if (request.Status != "pending")
            {
                return ApiResponse.Error("This request has already been processed.");
            }

            request.Status = "rejected";
            await _context.SaveChangesAsync();

            return ApiResponse.Ok("Friend request rejected.");
        }

        public async Task<List<FriendDto>> GetFriends(int userId)
        {
            var friendUsers = await _friendRepository.GetFriends(userId);

            if (!friendUsers.Any())
            {
                return new List<FriendDto>();
            }

            var friendUserIds = friendUsers.Select(u => u.UserId).ToList();
            var onlineUserIds = await _connectionRepository.GetOnlineUserIds(friendUserIds);

            return friendUsers.Select(u => new FriendDto
            {
                UserId = u.UserId,
                Username = u.Username,
                PublicKey = u.PublicKey,
                IsOnline = onlineUserIds.Contains(u.UserId)
            }).ToList();
        }
    }
}
