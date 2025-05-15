using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MagazijnBeheersysteem.Managers;
using MagazijnBeheersysteem.Models;

namespace MagazijnBeheersysteem
{
    public partial class MainWindow : Window
    {
        private readonly InventoryManager _manager = new InventoryManager();
        public ObservableCollection<Product> DisplayedProducts { get; } = new ObservableCollection<Product>();

        public MainWindow()
        {
            InitializeComponent();

            // Bind once—no more repeated ItemsSource assignments
            ProductsGrid.ItemsSource = DisplayedProducts;

            // Wire events AFTER InitializeComponent
            Loaded += MainWindow_Loaded;
            SearchBox.TextChanged += (s, e) => RefreshGrid();
            FilterBox.SelectionChanged += (s, e) => RefreshGrid();
            ListSelector.SelectionChanged += (s, e) => RefreshGrid();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate UI only once controls exist
            ListSelector.ItemsSource = _manager.ListNames.ToList();
            if (!_manager.ListNames.Contains(_manager.CurrentListName))
                _manager.CreateList(_manager.CurrentListName);
            ListSelector.SelectedItem = _manager.CurrentListName;

            FilterBox.SelectedIndex = 0;
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            // Guard against controls not yet ready
            string term = SearchBox?.Text ?? "";
            int mode = FilterBox?.SelectedIndex ?? 0;

            var items = _manager.Search(term, mode);

            DisplayedProducts.Clear();
            foreach (var p in items)
                DisplayedProducts.Add(p);
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            FilterBox.SelectedIndex = 0;
            RefreshGrid();
        }

        private void OnCreateListClicked(object sender, RoutedEventArgs e)
        {
            var name = NewListBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;

            _manager.CreateList(name);
            NewListBox.Text = "";
            ListSelector.ItemsSource = _manager.ListNames.ToList();
            ListSelector.SelectedItem = name;
            RefreshGrid();
        }

        private void OnDeleteListClicked(object sender, RoutedEventArgs e)
        {
            if (ListSelector.SelectedItem is string name && _manager.ListNames.Count() > 1)
            {
                var ok = MessageBox.Show(
                    $"Verwijder lijst '{name}'?",
                    "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning
                );
                if (ok == MessageBoxResult.Yes)
                {
                    _manager.DeleteList(name);
                    ListSelector.ItemsSource = _manager.ListNames.ToList();
                    ListSelector.SelectedItem = _manager.CurrentListName;
                    RefreshGrid();
                }
            }
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            // 1) Validate Name
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Naam is verplicht.", "Ontbrekend veld",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Validate Category
            if (string.IsNullOrWhiteSpace(CategoryBox.Text))
            {
                MessageBox.Show("Categorie is verplicht.", "Ontbrekend veld",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3) Validate Quantity
            if (!int.TryParse(QuantityBox.Text, out int qty))
            {
                MessageBox.Show("Aantal moet een geheel getal zijn.", "Ongeldig aantal",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4) Read unit and expiration
            var unit = (UnitBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "st";
            DateTime? exp = ExpirationPicker.SelectedDate;

            // 5) Create and add
            var p = new Product(NameBox.Text, CategoryBox.Text, qty, unit, exp);
            _manager.Add(p);

            // 6) Refresh UI and clear inputs
            RefreshGrid();
            ClearInputs();
        }


        private void OnEditClicked(object sender, RoutedEventArgs e)
        {
            // Must have a selected product
            if (ProductsGrid.SelectedItem is not Product sel)
            {
                MessageBox.Show("Selecteer eerst een product om te bewerken.", "Geen selectie",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1) Validate Name
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Naam is verplicht.", "Ontbrekend veld",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Validate Category
            if (string.IsNullOrWhiteSpace(CategoryBox.Text))
            {
                MessageBox.Show("Categorie is verplicht.", "Ontbrekend veld",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3) Validate Quantity
            if (!int.TryParse(QuantityBox.Text, out int qty))
            {
                MessageBox.Show("Aantal moet een geheel getal zijn.", "Ongeldig aantal",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4) Read unit and expiration
            var unit = (UnitBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "st";
            DateTime? exp = ExpirationPicker.SelectedDate;

            // 5) Construct updated product (preserving Id)
            var p = new Product(NameBox.Text, CategoryBox.Text, qty, unit, exp)
            {
                Id = sel.Id
            };
            _manager.Update(p);

            // 6) Refresh UI and clear inputs
            RefreshGrid();
            ClearInputs();
        }


        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is not Product sel) return;
            _manager.Remove(sel.Id);
            RefreshGrid();
            ClearInputs();
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product sel)
            {
                NameBox.Text = sel.Name;
                CategoryBox.Text = sel.Category;
                QuantityBox.Text = sel.Quantity.ToString();
                UnitBox.SelectedItem = UnitBox.Items
                                         .Cast<ComboBoxItem>()
                                         .FirstOrDefault(x => x.Content.ToString() == sel.Unit);
                ExpirationPicker.SelectedDate = sel.ExpirationDate;
            }
        }

        private void ClearInputs()
        {
            NameBox.Text = "";
            CategoryBox.Text = "";
            QuantityBox.Text = "";
            UnitBox.SelectedIndex = 0;
            ExpirationPicker.SelectedDate = null;
        }
        // Fired when the selected list in the ComboBox changes
        private void OnListChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListSelector.SelectedItem is string name)
            {
                _manager.SwitchList(name);
                RefreshGrid();
            }
        }

        // Fired when the filter ComboBox changes (Alle / Bederfelijk / Niet-bederfelijk)
        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshGrid();
        }

        // Fired when the “Zoek” button is clicked
        private void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            RefreshGrid();
        }

    }
}
