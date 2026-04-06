namespace AspKnP231.Models.Api.Cart
{
    public class CartItemFormModel
    {
        public String ProductId { get; set; } = null!;

        public int Cnt { get; set; }

        public double Price { get; set; }
    }
}
