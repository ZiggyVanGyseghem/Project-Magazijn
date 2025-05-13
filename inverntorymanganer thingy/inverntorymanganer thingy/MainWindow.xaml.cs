using System;
using System.Linq;
using System.Windows;
using MagazijnBeheersysteem.Managers;
using MagazijnBeheersysteem.Models;

namespace MagazijnBeheersysteem
{
    public partial class MainWindow : Window
    {
        private readonly InventoryManager _manager;

        public MainWindow()
        {
            InitializeComponent();
            _manager = new InventoryManager();
            RefreshGrid();
        }

        private void RefreshGrid(string term = null)
        {
            ProductsGrid.ItemsSource = _manager.Search(term);
        }

        private void OnSearchClicked(object sender, RoutedEventArgs e)
            => RefreshGrid(SearchBox.Text);

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            RefreshGrid();
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(QuantityBox.Text, out var qty))
                    throw new Exception("Ongeldig aantal");

                Product p = CategoryBox.Text.Equals("Perishable", StringComparison.OrdinalIgnoreCase)
                    && ExpirationPicker.SelectedDate.HasValue
                    ? new PerishableProduct(NameBox.Text, CategoryBox.Text, qty, ExpirationPicker.SelectedDate.Value)
                    : new Product(NameBox.Text, CategoryBox.Text, qty);

                _manager.Add(p);
                RefreshGrid();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout bij toevoegen",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnEditClicked(object sender, RoutedEventArgs e)
        {
            if (!(ProductsGrid.SelectedItem is Product sel))
            {
                MessageBox.Show("Selecteer een product om te bewerken.",
                                "Geen selectie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!int.TryParse(QuantityBox.Text, out var qty))
                    throw new Exception("Ongeldig aantal");

                Product p = sel is PerishableProduct
                    && ExpirationPicker.SelectedDate.HasValue
                    ? new PerishableProduct(NameBox.Text, CategoryBox.Text, qty, ExpirationPicker.SelectedDate.Value) { Id = sel.Id }
                    : new Product(NameBox.Text, CategoryBox.Text, qty) { Id = sel.Id };

                _manager.Update(p);
                RefreshGrid();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout bij bewerken",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (!(ProductsGrid.SelectedItem is Product sel))
            {
                MessageBox.Show("Selecteer een product om te verwijderen.",
                                "Geen selectie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Weet u zeker dat u '{sel.Name}' wilt verwijderen?",
                "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _manager.Remove(sel.Id);
                RefreshGrid();
                ClearInputs();
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product sel)
            {
                NameBox.Text = sel.Name;
                CategoryBox.Text = sel.Category;
                QuantityBox.Text = sel.Quantity.ToString();
                ExpirationPicker.SelectedDate =
                    sel is PerishableProduct pp ? pp.ExpirationDate : (DateTime?)null;
            }
        }

        private void ClearInputs()
        {
            NameBox.Clear();
            CategoryBox.Clear();
            QuantityBox.Clear();
            ExpirationPicker.SelectedDate = null;
        }
    }
}
