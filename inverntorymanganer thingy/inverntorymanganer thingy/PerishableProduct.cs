using System;

namespace MagazijnBeheersysteem.Models
{
    public class PerishableProduct : Product
    {
        public DateTime ExpirationDate { get; set; }

        public PerishableProduct() { }

        public PerishableProduct(string name, string category, int qty, DateTime exp)
            : base(name, category, qty)
        {
            ExpirationDate = exp;
        }
    }
}
