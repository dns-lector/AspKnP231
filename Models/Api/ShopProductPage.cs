namespace AspKnP231.Models.Api
{
    public class ShopProductPage
    {
        public ShopProductModel Product { get; set; } = null!;

        public IEnumerable<ShopProductModel> Recommended { get; set; } = [];
    }
}
/*
interface ProductPageType {
    product: ProductType,
    recommended: Array<ProductType>,
}
 */