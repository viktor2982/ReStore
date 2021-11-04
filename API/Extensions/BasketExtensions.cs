using System.Linq;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class BasketExtensions
    {
        public static BasketDTO MapBasketToDTO(this Basket basket)
        {
            if (basket == null) return null;
            
            return new BasketDTO()
            {
                Id = basket.Id,
                BuyerId = basket.BuyerId,
                Items = basket.Items.Select(item => new BasketItemDTO()
                {
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    Price = item.Product.Price,
                    PictureUrl = item.Product.PictureUrl,
                    Type = item.Product.Type,
                    Brand = item.Product.Brand,
                    Quantity = item.Quantity
                }).ToList()
            };
        }

        public static IQueryable<Basket> RetrieveBasketWithItems(this IQueryable<Basket> query, string buyerId)
        {
            return query
                .Include(i => i.Items)
                    .ThenInclude(p => p.Product)
                .Where(b => b.BuyerId == buyerId);
        }
    }
}