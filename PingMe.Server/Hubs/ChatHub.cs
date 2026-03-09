using Microsoft.AspNetCore.SignalR;
using PingMe.Server.Data;
using PingMe.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace PingMe.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
            _context = context;
        }

        public async Task AddUser(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                user.IsOnline = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Store username in context for later
                Context.Items["username"] = username;

                var chatMessage = new ChatMessage
                {
                    Sender = username,
                    Type = MessageType.JOIN,
                    Timestamp = DateTime.UtcNow,
                    Content = $"{username} joined the chat"
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("UserJoined", chatMessage);
            }
        }

        public async Task SendMessage(ChatMessage chatMessage)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == chatMessage.Sender);
            if (user != null)
            {
                chatMessage.Timestamp = DateTime.UtcNow;
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("MessageReceived", chatMessage);
            }
        }

        public async Task SendPrivateMessage(ChatMessage chatMessage)
        {
            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Username == chatMessage.Sender);
            var recipient = await _context.Users.FirstOrDefaultAsync(u => u.Username == chatMessage.Recipient);

            if (sender != null && recipient != null)
            {
                chatMessage.Timestamp = DateTime.UtcNow;
                chatMessage.Type = MessageType.PRIVATE_MESSAGE;

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                // In SignalR, we can send to a specific connection or a group named after the username
                await Clients.User(recipient.Username).SendAsync("PrivateMessageReceived", chatMessage);
                await Clients.Caller.SendAsync("PrivateMessageReceived", chatMessage);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("username", out var usernameObj) && usernameObj is string username)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    user.IsOnline = false;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    var chatMessage = new ChatMessage
                    {
                        Sender = username,
                        Type = MessageType.LEAVE,
                        Timestamp = DateTime.UtcNow,
                        Content = $"{username} left the chat"
                    };

                    _context.ChatMessages.Add(chatMessage);
                    await _context.SaveChangesAsync();

                    await Clients.All.SendAsync("UserLeft", chatMessage);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
