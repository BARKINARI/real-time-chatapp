using ChatAppServer.Api.Data;
using ChatAppServer.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppServer.Api.Hubs;

public class ChatHub(ApplicationDbContext db) : Hub
{
    public static Dictionary<string, Guid> Users = new();
    public async Task Connect(Guid userId)
    {
        Users.Add(Context.ConnectionId, userId);
        User? user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.Status = "online";
            await db.SaveChangesAsync();
            await Clients.All.SendAsync("Users", user);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid userId;
        Users.TryGetValue(Context.ConnectionId, out userId);
        Users.Remove(Context.ConnectionId);
        User? user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.Status = "offline";
            await db.SaveChangesAsync();
            await Clients.All.SendAsync("Users", user);
        }
    }
}
