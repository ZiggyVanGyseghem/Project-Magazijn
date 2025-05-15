using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MagazijnBeheersysteem.Managers;
using MagazijnBeheersysteem.Models;

namespace MagazijnBeheersysteem
{
    public partial class MainWindow : Window
    {
        private readonly InventoryManager _manager = new InventoryManager();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate list selector
            var names = _manager.ListNames.ToList();
            ListSelector.ItemsSource = names;
            if (!names.Contains(_manager.CurrentListName))
                _manager.CreateList(_manager.CurrentListName);
            ListSelector.SelectedItem = _manager.CurrentListName;

            // Set default filter
            FilterBox.SelectedIndex = 0;

            // Initial load
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            // Guard against uninitialized controls
            if (SearchBox == null || FilterBox == null || ProductsGrid == null)
                return;

            string term = SearchBox.Text;
            int filterMode = FilterBox.SelectedIndex; // 0=All,1=Perish,2=NonPerish
            ProductsGrid.ItemsSource = _manager.Search(term, filterMode).ToList();
        }

        private void OnSearchClicked(object sender, RoutedEventArgs e) => RefreshGrid();

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            FilterBox.SelectedIndex = 0;
            RefreshGrid();
        }

        private void OnCreateListClicked(object sender, RoutedEventArgs e)
        {
            var name = NewListBox.Text?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                _manager.CreateList(name);
                NewListBox.Text = "";
                ListSelector.ItemsSource = _manager.ListNames.ToList();
                ListSelector.SelectedItem = name;
                RefreshGrid();
            }
        }

        private void OnListChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListSelector.SelectedItem is string name)
            {
                _manager.SwitchList(name);
                RefreshGrid();
            }
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e) => RefreshGrid();

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityBox.Text, out int qty)) return;
            var unit = (UnitBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "st";
            DateTime? exp = ExpirationPicker.SelectedDate;

            var p = new Product(NameBox.Text, CategoryBox.Text, qty, unit, exp);
            _manager.Add(p);
            RefreshGrid();
            ClearInputs();
        }

        private void OnEditClicked(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is not Product sel) return;
            if (!int.TryParse(QuantityBox.Text, out int qty)) return;
            var unit = (UnitBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "st";
            DateTime? exp = ExpirationPicker.SelectedDate;

            var p = new Product(NameBox.Text, CategoryBox.Text, qty, unit, exp) { Id = sel.Id };
            _manager.Update(p);
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
        private void OnDeleteListClicked(object sender, RoutedEventArgs e)
        {
            if (ListSelector.SelectedItem is not string name || string.IsNullOrEmpty(name))
                return;

            // Prevent deleting the last list
            if (_manager.ListNames.Count() <= 1)
            {
                MessageBox.Show("Je moet minstens één lijst bewaren.", "Verwijderen geannuleerd",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Weet je zeker dat je de lijst '{name}' wilt verwijderen?\nAlle producten hierin gaan verloren.",
                "Bevestig verwijderen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _manager.DeleteList(name);
                RefreshGrid();
            }
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
    }
}
