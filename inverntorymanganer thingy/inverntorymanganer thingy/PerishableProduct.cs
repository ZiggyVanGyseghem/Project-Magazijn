using System;
using System.Text.Json.Serialization;

namespace MagazijnBeheersysteem.Models
{
    public class PerishableProduct : Product
    {
        // override the flag, but use the base ExpirationDate
        [JsonIgnore]
        public override bool IsPerishable => true;

        public PerishableProduct() { }

        public PerishableProduct(string name, string category, int qty, string unit, DateTime exp)
            : base(name, category, qty, unit, exp)
        {
        }
    }
}
