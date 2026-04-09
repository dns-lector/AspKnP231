namespace AspKnP231.Data.Entities
{
    // Товарна група (розділ)
    public record ShopSection
    {
        public Guid      Id          { get; set; }
                                     
        public Guid?     ParentId    { get; set; }
                                     
        public String    Title       { get; set; } = null!;
                         
        public String    Description { get; set; } = null!;
                         
        public String    Slug        { get; set; } = null!;
                         
        public String    ImageUrl    { get; set; } = null!;

        public DateTime? DeletedAt   { get; set; }

        // додано 2026-04-09
        public int OrderInPrice { get; set; } = 10000;


        public ICollection<ShopProduct> Products { get; set; } = [];

    }
}
