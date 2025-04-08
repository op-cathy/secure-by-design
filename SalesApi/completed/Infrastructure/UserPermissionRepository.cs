using SalesApi.Domain.DomainPrimitives;
using SalesApi.Domain.Services;

namespace SalesApi.Infrastructure;

public class UserPermissionRepository : IUserPermissionRepository
{
    private readonly Dictionary<UserId, MarketId> _userPermissions = new()
    {
        {new UserId("auth0|655c7e9a022f6b2083b15dc5"), new MarketId("no")},
        {new UserId("ozrjG9OAXgswPYYYmeQaDQZVPLDR3p9y@clients"), new MarketId("no")},
    };

    public Task<List<MarketId>> GetUserMarketPermissions(UserId userId)
    {
        // The following line is for ease of use when doing the client workshop.
        // In real scenarios, it should be removed
        return Task.FromResult(new List<MarketId> { new("no") });
        
        var marketId = _userPermissions.GetValueOrDefault(userId);
        return Task.FromResult(new List<MarketId> { marketId });
    }
}