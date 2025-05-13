using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MagazijnBeheersysteem.Models;

namespace MagazijnBeheersysteem.Managers
{
    public class InventoryManager
    {
        private const string DataFolder = "data";
        private const string DataFile = "products.json";
        private readonly string _filePath;
        private List<Product> _products = new List<Product>();

        public InventoryManager()
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), DataFolder);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, DataFile);
            Load();
        }

        public IEnumerable<Product> Search(string term = null)
        {
            if (string.IsNullOrWhiteSpace(term))
                return _products;

            term = term.ToLower();
            return _products.Where(p =>
               p.Name.ToLower().Contains(term) ||
               p.Category.ToLower().Contains(term));
        }

        public void Add(Product p)
        {
            _products.Add(p);
            Save();
        }

        public void Update(Product p)
        {
            var existing = _products.FirstOrDefault(x => x.Id == p.Id);
            if (existing == null) return;

            existing.Name = p.Name;
            existing.Category = p.Category;
            existing.Quantity = p.Quantity;

            if (existing is PerishableProduct ep && p is PerishableProduct np)
                ep.ExpirationDate = np.ExpirationDate;

            Save();
        }

        public void Remove(Guid id)
        {
            var toRemove = _products.FirstOrDefault(x => x.Id == id);
            if (toRemove != null)
            {
                _products.Remove(toRemove);
                Save();
            }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                var json = File.ReadAllText(_filePath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Eerst deserialize naar Product, zet perishable terug om
                var list = JsonSerializer.Deserialize<List<Product>>(json, opts);
                _products = list
                    .Select(p => p.Category.Equals("Perishable", StringComparison.OrdinalIgnoreCase)
                        ? JsonSerializer.Deserialize<PerishableProduct>(JsonSerializer.Serialize(p), opts)
                        : p)
                    .ToList();
            }
            catch
            {
                _products = new List<Product>();
            }
        }

        private void Save()
        {
            try
            {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_products, opts);
                File.WriteAllText(_filePath, json);
            }
            catch { /* optioneel loggen */ }
        }
    }
}
