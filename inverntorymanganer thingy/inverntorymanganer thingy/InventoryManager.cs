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

        public void CreateList(string name)
        {
            if (!_lists.ContainsKey(name))
            {
                _lists[name] = new List<Product>();
                _activeListName = name;
                Save();
            }
        }

        public void SwitchList(string name)
        {
            _activeListName = name;
            Load(_activeListName);
        }

        public IEnumerable<Product> Products =>
            _lists.ContainsKey(_activeListName)
                ? _lists[_activeListName]
                : Enumerable.Empty<Product>();

        public void Add(Product p)
        {
            _lists[_activeListName].Add(p);
            Save();
        }

        public void Update(Product p)
        {
            var existing = _lists[_activeListName].FirstOrDefault(x => x.Id == p.Id);
            if (existing == null) return;

            existing.Name = p.Name;
            existing.Category = p.Category;
            existing.Quantity = p.Quantity;
            existing.Unit = p.Unit;
            existing.ExpirationDate = p.ExpirationDate;
            Save();
        }

        public void Remove(Guid id)
        {
            var toRm = _lists[_activeListName].FirstOrDefault(x => x.Id == id);
            if (toRm != null)
            {
                _lists[_activeListName].Remove(toRm);
                Save();
            }
        }

        /// <summary>
        /// filterMode: 0=All, 1=Perishables only, 2=Non-perishables only
        /// </summary>
        public IEnumerable<Product> Search(string term, int filterMode)
        {
            IEnumerable<Product> list = Products;

            if (filterMode == 1)
                list = list.Where(p => p.IsPerishable);
            else if (filterMode == 2)
                list = list.Where(p => !p.IsPerishable);

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();
                list = list.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Category.ToLower().Contains(term));
            }

            return list;
        }

        private void Load(string listName)
        {
            var path = Path.Combine(_folder, listName + ".json");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                _lists[listName] = JsonSerializer.Deserialize<List<Product>>(json, opts)
                                   ?? new List<Product>();
            }
            else
            {
                _lists[listName] = new List<Product>();
            }
        }

        private void Save()
        {
            var path = Path.Combine(_folder, _activeListName + ".json");
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_lists[_activeListName], opts);
            File.WriteAllText(path, json);
        }
    }
}
