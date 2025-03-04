﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Enums;
using VideoDiningApp.Models;
using Microsoft.Extensions.Logging;

namespace VideoDiningApp.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            order.GroupOrderId = Guid.NewGuid();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                .Include(o => o.Participants)
                .ToListAsync();
        }

        public async Task<Order> GetOrderById(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserId(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrder(int id, Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null) return false;

            existingOrder.FoodItems = order.FoodItems;
            existingOrder.TotalAmount = order.TotalAmount;
               existingOrder.PaymentStatus = order.PaymentStatus;
            existingOrder.IsDelivered = order.IsDelivered;

            _context.Orders.Update(existingOrder);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentStatus(int orderId, PaymentStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            _logger.LogInformation($"Updating Order ID: {orderId} to {status}");

            order.PaymentStatus = status;
            _context.Orders.Update(order);
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation($"Update Result: {result}");

            return result > 0;
        }

        public async Task<Order> GetOrderByUserAndGroupId(int userId, Guid groupOrderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.UserId == userId && o.GroupOrderId == groupOrderId);
        }

        public async Task<List<Order>> GetOrdersByGroupId(Guid groupOrderId)
        {
            return await _context.Orders.Where(o => o.GroupOrderId == groupOrderId).ToListAsync();
        }

        public async Task<bool> MarkGroupOrderAsPaid(Guid groupOrderId)
        {
            var groupOrders = await GetOrdersByGroupId(groupOrderId);
            bool allPaid = groupOrders.All(order => order.PaymentStatus == PaymentStatus.COMPLETED);

            if (!allPaid)
            {
                foreach (var order in groupOrders)
                {
                    if (order.PaymentStatus != PaymentStatus.COMPLETED)
                    {
                        order.PaymentStatus = PaymentStatus.COMPLETED;
                        _context.Orders.Update(order);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return allPaid;
        }

        public async Task<bool> SavePaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                var order = await _context.Orders.FindAsync(payment.OrderId);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.COMPLETED;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                }
            }

            return result > 0;
        }
    }
}
