using AspKnP231.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspKnP231.Data
{
    // DAL
    public class DataAccessor(DataContext dataContext)
    {
        private readonly DataContext _dataContext = dataContext;

        public Cart GetOrCreateActiveCart(Guid userId)
        {
            Cart? cart = GetActiveCart(userId);
            // якщо кошику немає - створюємо новий
            if (cart == null)
            {
                cart = new Cart()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreateDt = DateTime.Now,
                };
                _dataContext.Carts.Add(cart);
            }
            return cart;
        }

        public Cart? GetActiveCart(Guid userId)
        {
            return _dataContext
                .Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c =>
                    c.UserId == userId && 
                    c.DeleteDt == null && 
                    c.OrderDt == null);
        }

        public IEnumerable<ShopSection> AllShopSections()
        {
            return _dataContext
                .ShopSections
                .AsNoTracking()
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.OrderInPrice)
                .AsEnumerable();
        }
        public ShopSection? GetShopSectionBySlug(String slug)
        {
            return _dataContext
                .ShopSections
                .Include(s => s.Products)
                .AsNoTracking()
                .FirstOrDefault(s => s.Slug == slug && s.DeletedAt == null);
        }
        public ShopProduct? GetShopProductBySlug(String slugOrId)
        {
            return _dataContext
                .ShopProducts
                .AsNoTracking()
                .FirstOrDefault(p => (p.Slug == slugOrId || p.Id.ToString() == slugOrId) && p.DeletedAt == null);
        }
    }
}
/* DAL (Data Access Layer) - сукупність DAO або їх методів
 * 
 */