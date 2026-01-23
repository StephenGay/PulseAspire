using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pulse.ApiService.Hubs;

[TestClass]
public class PresenceServiceTests
{
    [TestMethod]
    public void AddConnection_AddsAndGetOnlineUsersReturnsEntries()
    {
        // Arrange
        var svc = new PresenceService();

        // Act
        svc.AddConnection("user1", "conn1", "Alice");
        svc.AddConnection("user2", "conn2", "Bob");

        var users = svc.GetOnlineUsers().ToList();

        // Assert
        Assert.AreEqual(2, users.Count);
        Assert.IsTrue(users.Any(u => u.UserId == "user1" && u.UserName == "Alice"));
        Assert.IsTrue(users.Any(u => u.UserId == "user2" && u.UserName == "Bob"));
    }

    [TestMethod]
    public void RemoveConnection_RemovesUserWhenNoConnectionsRemain()
    {
        // Arrange
        var svc = new PresenceService();
        svc.AddConnection("user1", "conn1", "Alice");
        svc.AddConnection("user1", "conn2", "Alice");

        // Act - remove first connection
        svc.RemoveConnection("conn1");
        var usersAfterOneRemoved = svc.GetOnlineUsers().ToList();
        Assert.AreEqual(1, usersAfterOneRemoved.Count);

        // Act - remove last connection
        svc.RemoveConnection("conn2");
        var usersAfterAllRemoved = svc.GetOnlineUsers().ToList();
        Assert.AreEqual(0, usersAfterAllRemoved.Count);
    }
}