using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AestheticStudySpace.Api.Hubs;

/// <summary>
/// SignalR hub scaffold for future real-time presence and shared study rooms.
/// </summary>
[Authorize]
public class PresenceHub : Hub
{
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeft", Context.UserIdentifier);
    }

    public Task UpdatePresence(string roomId, bool isFocused) =>
        Clients.Group(roomId).SendAsync("PresenceUpdated", new { userId = Context.UserIdentifier, isFocused });
}
