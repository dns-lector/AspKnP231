using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspKnP231.Models.Shop.Admin
{
    public class AdminDiscountFormModel
    {
        [FromForm(Name = "discount-title")]
        public String Title { get; set; } = null!;

        [FromForm(Name = "discount-description")]
        public String Description { get; set; } = null!;

        [FromForm(Name = "discount-percent")]
        public double? Percent { get; set; }

        [FromForm(Name = "discount-price")]
        public double? Price { get; set; }

        [FromForm(Name = "discount-start")]
        public DateTime Start { get; set; }

        [FromForm(Name = "discount-finish")]
        public DateTime? Finish { get; set; }

    }
}
