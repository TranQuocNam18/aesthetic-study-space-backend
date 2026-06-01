using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AestheticStudySpace.Api.Hubs;

/// <summary>
/// SignalR hub scaffold for future real-time presence and shared study rooms.
/// </summary>
[Authorize]
public class PresenceHub : Hub
{
    private static int _onlineCount;
    private static readonly ConcurrentDictionary<string, int> RoomCounts = new(StringComparer.Ordinal);

    public override async Task OnConnectedAsync()
    {
        Interlocked.Increment(ref _onlineCount);
        await Clients.All.SendAsync("OnlineCountUpdated", _onlineCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref _onlineCount);
        await Clients.All.SendAsync("OnlineCountUpdated", Math.Max(0, _onlineCount));
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        RoomCounts.AddOrUpdate(roomId, 1, (_, v) => v + 1);
        await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier);
        await Clients.Group(roomId).SendAsync("RoomPresenceCountUpdated", RoomCounts[roomId]);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        RoomCounts.AddOrUpdate(roomId, 0, (_, v) => Math.Max(0, v - 1));
        await Clients.Group(roomId).SendAsync("UserLeft", Context.UserIdentifier);
        await Clients.Group(roomId).SendAsync("RoomPresenceCountUpdated", RoomCounts[roomId]);
    }

    public Task UpdatePresence(string roomId, bool isFocused) =>
        Clients.Group(roomId).SendAsync("PresenceUpdated", new { userId = Context.UserIdentifier, isFocused });
}
