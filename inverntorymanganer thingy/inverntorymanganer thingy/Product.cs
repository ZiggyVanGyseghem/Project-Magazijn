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

        // ← Move ExpirationDate into the base class
        public DateTime? ExpirationDate { get; set; }

        // You can keep your CreatedDate if you like
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [JsonIgnore]
        public virtual bool IsPerishable => ExpirationDate.HasValue;

        [JsonIgnore]
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value.Date <= DateTime.Now.Date;

        [JsonIgnore]
        public bool IsExpiringSoon
        {
            get
            {
                if (!ExpirationDate.HasValue) return false;
                var totalDays = (ExpirationDate.Value - CreatedDate).TotalDays;
                if (totalDays <= 0) return false;
                var half = CreatedDate.AddDays(totalDays / 2);
                return DateTime.Now.Date >= half.Date && !IsExpired;
            }
        }

        [JsonIgnore]
        public string ExpirationDateDisplay =>
            ExpirationDate.HasValue ? ExpirationDate.Value.ToString("d") : "";

        [JsonIgnore]
        public string CreatedDateDisplay => CreatedDate.ToString("d");

        public Product() { }

        // Constructor now accepts optional expiration
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
