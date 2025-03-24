using Microsoft.AspNetCore.SignalR;
using VideoDiningApp.Services;

namespace VideoDiningApp.Hubs
{
    public class OrderHub : Hub
    {
        private readonly IOrderService _orderService;

        public OrderHub(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task TrackOrder(int orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order != null)
            {
                await Clients.All.SendAsync("ReceiveOrderUpdate", order);
            }
        }
    }
}
