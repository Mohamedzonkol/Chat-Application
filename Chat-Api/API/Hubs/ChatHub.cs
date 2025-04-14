using API.Data;
using API.Dto;
using API.Extensions;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace API.Hubs
{
    public class ChatHub(UserManager<AppUser> userManager, AppDbContext context) : Hub
    {
        public static readonly ConcurrentDictionary<string, OnlineUserDto>
            OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var receivedId = httpContext?.Request.Query["senderId"].ToString();
            var userName = httpContext?.User?.Identity!.Name!;
            var currentUser = await userManager.FindByNameAsync(userName);

            var connectionId = Context.ConnectionId;
            if (OnlineUsers.ContainsKey(userName))
            {
                OnlineUsers[userName].ConnectionId = connectionId;
            }
            else
            {
                var user = new OnlineUserDto
                {
                    ConnectionId = connectionId,
                    UserName = userName,
                    FullName = currentUser!.FullName,
                    Image = currentUser!.ProfileImage,
                };
                OnlineUsers.TryAdd(userName, user);
                await Clients.AllExcept(connectionId).SendAsync("Notify", currentUser);
            }

            if (!string.IsNullOrEmpty(receivedId))
                await LoadMessages(receivedId);
            await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
        }

        public async Task LoadMessages(string recipientId, int pageNumber = 1)
        {
            int pageSize = 10;
            var userName = Context.User!.Identity!.Name!;
            var currentUser = await userManager.FindByNameAsync(userName!);
            if (currentUser is null)
                return;
            List<MessagesResponseDto> messages = await context.Messages.Where(x =>
                    x.ReceiverId == currentUser!.Id && x.SenderId == recipientId || x.SenderId == currentUser!.Id &&
                    x.ReceiverId == recipientId).OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new MessagesResponseDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    CreatedAt = x.CreatedAt,
                    ReceiverId = x.ReceiverId,
                    SenderId = x.SenderId
                }).ToListAsync();
            foreach (var message in messages)
            {
                var msg = await context.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);
                if (msg != null && msg.ReceiverId == currentUser.Id)
                {
                    msg.IsRead = true;
                    //context.Messages.Update(msg);
                    await context.SaveChangesAsync();
                }

            }
            await Clients.User(currentUser.Id).SendAsync("ReceiveMessage", messages);


        }
        public async Task SendMessages(MessagesRequestDto messages)
        {
            var senderId = Context.User!.Identity!.Name;
            var recipientId = messages.ReceiverId;
            var newMessage = new Messages
            {
                Sender = await userManager.FindByNameAsync(senderId!),
                Receiver = await userManager.FindByIdAsync(recipientId),
                Content = messages.Content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Messages.Add(newMessage);
            await context.SaveChangesAsync();

            await Clients.User(recipientId).SendAsync("ReceiveMessage", newMessage);
        }
        public async Task NotifyTyping(string recipientUserName)
        {
            var senderUserName = Context.User!.Identity!.Name;
            if (senderUserName is null)
                return;
            var connectionId = OnlineUsers.Values.FirstOrDefault(x => x.UserName == recipientUserName)?.ConnectionId;
            if (connectionId is not null)
            {
                await Clients.Client(connectionId).SendAsync("NotifyTyping", senderUserName);
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User!.Identity!.Name;
            if (userName is null)
                return;
            OnlineUsers.TryRemove(userName, out _);
            await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
            await base.OnDisconnectedAsync(exception);
        }



        private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
        {
            var userName = Context.User!.GetUserName();
            var onlineUsers = new HashSet<string>(OnlineUsers.Keys);
            var allUsers = await userManager.Users
                .Select(x => new OnlineUserDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    FullName = x.FullName,
                    Image = x.ProfileImage,
                    IsOnline = onlineUsers.Contains(x.UserName),
                    UnreadCount = context.Messages.Count(m => m.ReceiverId == userName && m.SenderId == x.Id && !m.IsRead)
                }).OrderByDescending(o => o.IsOnline)
                .ToListAsync();
            return allUsers;
        }
    }
}
