using System;

namespace MagazijnBeheersysteem.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }

        public Product() { }

        public Product(string name, string category, int qty)
        {
            Name = name;
            Category = category;
            Quantity = qty;
        }
    }
}
