using Microsoft.EntityFrameworkCore;
using OrdersMinimalAPI.EfCore;
using OrdersMinimalAPI.Model;
using OrdersMinimalAPI.Response;
using OrdersMinimalAPI.Request;

namespace OrdersMinimalAPI.RouteGroup
{
    public static class OrdersMapGroup
    {
        public static RouteGroupBuilder OrdersAPI(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (AppDbContext dbContext) => 
            {
                var orders = await dbContext.Orders.ToListAsync();

                List<OrderResponse> ordersResponse = new();

                foreach (var order in orders)
                {
                    ordersResponse.Add(new OrderResponse(order));
                }

                return Results.Ok(ordersResponse);
            });

            group.MapPost("/", async(OrderAddRequest request, AppDbContext dbContext) => 
            {
                var datetime = DateTime.Now.ToString("yyyyMMddHHmm");
                Order order = new()
                {
                    OrderNumber = String.Concat("Order_", datetime),
                    CustomerName = request.CustomerName,
                    OrderDate = request.OrderDate,
                    TotalAmount = request.TotalAmount
                };
                await dbContext.Orders.AddAsync(order);
                await dbContext.SaveChangesAsync();
                OrderResponse response = new(order);

                return Results.Created($"/{order.OrderId}", response);
            });

            group.MapGet("/{id}", async(Guid id, AppDbContext dbContext) => 
            { 
                var order = await dbContext.Orders.FindAsync(id);

                if (order == null)
                {
                    return Results.Problem(statusCode: 404, detail: "Order doesn't exist");
                }

                OrderResponse response = new(order);

                return Results.Ok(response);
            });

            group.MapPatch("/{id}", async(Guid id, OrderUpdateRequest request, AppDbContext dbContext) => 
            {
                var order = await dbContext.Orders.FindAsync(id);

                if (order == null)
                {
                    return Results.Problem(statusCode: 400, detail: "Order doesn't exist while updating data");
                }

                order.CustomerName = request.CustomerName;
                order.TotalAmount = request.TotalAmount;

                await dbContext.SaveChangesAsync();
                OrderResponse response = new(order);

                return Results.Ok(response);

            });

            group.MapDelete("/{id}", async(Guid id, AppDbContext dbContext) => 
            {
                var order = await dbContext.Orders.FindAsync(id);

                if (order == null)
                {
                    return Results.Problem(statusCode: 404, detail: "Order doesn't exist");
                }

                dbContext.Orders.Remove(order);

                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });

            group.MapGet("{orderId}/items", async(Guid orderId, AppDbContext dbContext) => 
            {
                var orderItems = await dbContext.OrderItems.Where(o => o.OrderId.Equals(orderId)).ToListAsync();

                List<OrderItemResponse> orderItemResponses = new();

                foreach (var item in orderItems)
                {
                    orderItemResponses.Add(new OrderItemResponse(item));
                }

                return Results.Ok(orderItemResponses);
            });

            group.MapPost("{orderId}/items", async(Guid orderId, OrderItemAddRequest request, AppDbContext dbContext) =>
            {
                OrderItem orderItem = new()
                {
                    OrderId = orderId,
                    ProductName = request.ProductName,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice,
                    TotalPrice = (request.Quantity * request.UnitPrice)
                };

                await dbContext.OrderItems.AddAsync(orderItem);
                await dbContext.SaveChangesAsync();

                OrderItemResponse response = new(orderItem);

                return Results.Created($"{orderId}/items/{orderItem.OrderItemId}", response);
            });

            group.MapGet("{orderId}/items/{id}", async(Guid orderId, Guid id, AppDbContext dbContext) =>
            {
                var orderItem = await dbContext.OrderItems.Where(o => o.OrderId.Equals(orderId))
                    .Where(o => o.OrderItemId.Equals(id)).FirstOrDefaultAsync();

                if (orderItem == null)
                {
                    return Results.Problem(statusCode: 404, detail: "Order item doesn't exist");
                }

                OrderItemResponse response = new(orderItem);

                return Results.Ok(response);
            });

            group.MapPatch("{orderId}/items/{id}", async (Guid orderId, Guid id, OrderItemUpdateRequest request, AppDbContext dbContext) =>
            {
                var orderItem = await dbContext.OrderItems.Where(o => o.OrderId.Equals(orderId))
                    .Where(o => o.OrderItemId.Equals(id)).FirstOrDefaultAsync();

                if (orderItem == null)
                {
                    return Results.Problem(statusCode: 400, detail: "Order item doesn't exist while updating data");
                }

                orderItem.ProductName = request.ProductName;
                orderItem.UnitPrice = request.UnitPrice;
                orderItem.Quantity = request.Quantity;
                orderItem.TotalPrice = (request.UnitPrice * request.Quantity);

                await dbContext.SaveChangesAsync();

                OrderItemResponse response = new(orderItem);

                return Results.Ok(response);
            });

            group.MapDelete("{orderId}/items/{id}", async (Guid orderId, Guid id, AppDbContext dbContext) =>
            {
                var orderItem = await dbContext.OrderItems.Where(o => o.OrderId.Equals(orderId))
                    .Where(o => o.OrderItemId.Equals(id)).FirstOrDefaultAsync();

                if (orderItem == null)
                {
                    return Results.Problem(statusCode: 404, detail: "Order item doesn't exist");
                }

                dbContext.OrderItems.Remove(orderItem);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });

            return group;
        }
    }
}
