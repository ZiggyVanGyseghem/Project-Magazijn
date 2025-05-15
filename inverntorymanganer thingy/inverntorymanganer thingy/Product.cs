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

        // When item was first added
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Optional expiry
        public DateTime? ExpirationDate { get; set; }

        [JsonIgnore]
        public bool IsPerishable => ExpirationDate.HasValue;

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
                var midPoint = CreatedDate.AddDays(totalDays / 2);
                return DateTime.Now.Date >= midPoint.Date && !IsExpired;
            }
        }

        [JsonIgnore]
        public string ExpirationDateDisplay =>
            ExpirationDate.HasValue ? ExpirationDate.Value.ToString("d") : "";

        [JsonIgnore]
        public string CreatedDateDisplay => CreatedDate.ToString("d");

        public Product() { }

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
