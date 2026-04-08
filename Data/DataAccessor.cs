using AspKnP231.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspKnP231.Data
{
    // DAL
    public class DataAccessor(DataContext dataContext)
    {
        private readonly DataContext _dataContext = dataContext;

        public IEnumerable<ShopSection> AllShopSections()
        {
            return _dataContext
                .ShopSections
                .AsNoTracking()
                .Where(s => s.DeletedAt == null)
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