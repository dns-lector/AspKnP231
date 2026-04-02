using Microsoft.AspNetCore.Mvc;

namespace AspKnP231.Models.Shop.Admin
{
    public class AdminDiscountDetailFormModel
    {
        [FromForm(Name = "discount-detail-discount-id")]
        public String DiscountId { get; set; } = null!;


        [FromForm(Name = "discount-detail-product-id")]
        public String ProductId { get; set; } = null!;


        [FromForm(Name = "discount-detail-price")]
        public double? Price { get; set; } = null!;
    }
}
