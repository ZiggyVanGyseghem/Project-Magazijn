using System;
using System.Text.Json.Serialization;

namespace MagazijnBeheersysteem.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int Quantity { get; set; }
        public string Unit { get; set; } = "st";

        // Optional expiration date
        public DateTime? ExpirationDate { get; set; }

        [JsonIgnore]
        public bool IsPerishable => ExpirationDate.HasValue;

        [JsonIgnore]
        public string ExpirationDateDisplay =>
            ExpirationDate.HasValue ? ExpirationDate.Value.ToString("d") : "";

        public Product() { }

        // Primary constructor
        public Product(string name, string category, int qty, string unit, DateTime? exp = null)
        {
            Name = name;
            Category = category;
            Quantity = qty;
            Unit = unit;
            ExpirationDate = exp;
        }
    }
}
