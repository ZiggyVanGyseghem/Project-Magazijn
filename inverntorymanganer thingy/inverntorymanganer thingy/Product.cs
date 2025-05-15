using System;
using System.Text.Json.Serialization;

namespace MagazijnBeheersysteem.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = "st"; // default eenheid

        [JsonIgnore]
        public virtual bool IsPerishable => false;

        public Product() { }

        public Product(string name, string category, int qty, string unit)
        {
            Name = name;
            Category = category;
            Quantity = qty;
            Unit = unit;
        }
    }

    public class PerishableProduct : Product
    {
        public DateTime ExpirationDate { get; set; }

        [JsonIgnore]
        public override bool IsPerishable => true;

        public PerishableProduct() { }

        public PerishableProduct(string name, string category, int qty, string unit, DateTime exp)
            : base(name, category, qty, unit)
        {
            ExpirationDate = exp;
        }
    }
}
