using System.Collections.Concurrent;
using Acme.Common.Models;

namespace Acme.Orders.Api.Services;

public class OrderRepository
{
    private readonly ConcurrentDictionary<int, Order> _orders = new();
    private int _nextId;

    public IEnumerable<Order> All() => _orders.Values.OrderBy(o => o.Id);

    public Order? Get(int id) => _orders.TryGetValue(id, out var o) ? o : null;

    public Order Add(Order order)
    {
        order.Id = Interlocked.Increment(ref _nextId);
        _orders[order.Id] = order;
        return order;
    }
}
