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
        private readonly string _folder;
        private readonly Dictionary<string, List<Product>> _lists = new();
        private string _activeListName = "Default";

        public InventoryManager()
        {
            _folder = Path.Combine(Directory.GetCurrentDirectory(), "data");
            if (!Directory.Exists(_folder))
                Directory.CreateDirectory(_folder);

            Load(_activeListName);
        }

        public IEnumerable<string> ListNames =>
            Directory.GetFiles(_folder, "*.json")
                     .Select(f => Path.GetFileNameWithoutExtension(f));

        public string CurrentListName => _activeListName;

        public void CreateList(string listName)
        {
            if (!_lists.ContainsKey(listName))
            {
                _lists[listName] = new List<Product>();
                _activeListName = listName;
                Save();
            }
        }

        public void SwitchList(string listName)
        {
            _activeListName = listName;
            Load(_activeListName);
        }

        public IEnumerable<Product> Products =>
            _lists.ContainsKey(_activeListName) ? _lists[_activeListName] : new List<Product>();

        public void Add(Product p)
        {
            _lists[_activeListName].Add(p);
            Save();
        }

        public void Update(Product p)
        {
            var existing = _lists[_activeListName].FirstOrDefault(x => x.Id == p.Id);
            if (existing != null)
            {
                existing.Name = p.Name;
                existing.Category = p.Category;
                existing.Quantity = p.Quantity;

                if (existing is PerishableProduct ep && p is PerishableProduct np)
                    ep.ExpirationDate = np.ExpirationDate;

                Save();
            }
        }

        public void Remove(Guid id)
        {
            var toRemove = _lists[_activeListName].FirstOrDefault(x => x.Id == id);
            if (toRemove != null)
            {
                _lists[_activeListName].Remove(toRemove);
                Save();
            }
        }

        public IEnumerable<Product> Search(string term, bool onlyPerishables = false)
        {
            IEnumerable<Product> list = Products;

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();
                list = list.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Category.ToLower().Contains(term));
            }

            if (onlyPerishables)
            {
                list = list.Where(p => p is PerishableProduct);
            }

            return list;
        }

        private void Load(string listName)
        {
            var path = Path.Combine(_folder, listName + ".json");

            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var raw = JsonSerializer.Deserialize<List<Product>>(json, opts) ?? new();

                    _lists[listName] = raw.Select(p =>
                        p.Category.Equals("Perishable", StringComparison.OrdinalIgnoreCase)
                        ? JsonSerializer.Deserialize<PerishableProduct>(JsonSerializer.Serialize(p), opts)
                        : p).ToList();
                }
                else
                {
                    _lists[listName] = new List<Product>();
                }
            }
            catch
            {
                _lists[listName] = new List<Product>();
            }
        }

        private void Save()
        {
            try
            {
                var path = Path.Combine(_folder, _activeListName + ".json");
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_lists[_activeListName], opts);
                File.WriteAllText(path, json);
            }
            catch
            {
                // Error writing to file — optional: log
            }
        }
    }
}
