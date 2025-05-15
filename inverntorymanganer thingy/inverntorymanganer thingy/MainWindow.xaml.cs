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
        private readonly InventoryManager _manager;

        public MainWindow()
        {
            InitializeComponent();
            _manager = new InventoryManager();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListSelector.ItemsSource = _manager.ListNames.ToList();
            ListSelector.SelectedItem = _manager.ListNames.FirstOrDefault() ?? "Default";
        }

        private void RefreshGrid(string term = null)
        {
            ProductsGrid.ItemsSource = _manager.Search(term, OnlyPerishablesCheck.IsChecked == true);
        }

        private void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            RefreshGrid(SearchBox.Text);
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            OnlyPerishablesCheck.IsChecked = false;
            RefreshGrid();
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(QuantityBox.Text, out int qty)) throw new Exception("Ongeldig aantal");
                string unit = (UnitBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "st";
                Product p;

                if (CategoryBox.Text.ToLower() == "perishable" && ExpirationPicker.SelectedDate.HasValue)
                {
                    p = new PerishableProduct(NameBox.Text, CategoryBox.Text, qty, unit, ExpirationPicker.SelectedDate.Value);
                }
                else
                {
                    p = new Product(NameBox.Text, CategoryBox.Text, qty, unit);
                }

                _manager.Add(p);
                RefreshUI();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout bij toevoegen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnEditClicked(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is not Product sel)
            {
                MessageBox.Show("Selecteer een product om te bewerken.", "Geen selectie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!int.TryParse(QuantityBox.Text, out int qty)) throw new Exception("Ongeldig aantal");
                Product p;

                if (sel is PerishableProduct && ExpirationPicker.SelectedDate.HasValue)
                    p = new PerishableProduct(NameBox.Text, CategoryBox.Text, qty, ExpirationPicker.SelectedDate.Value) { Id = sel.Id };
                else
                    p = new Product(NameBox.Text, CategoryBox.Text, qty) { Id = sel.Id };

                _manager.Update(p);
                RefreshGrid();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout bij bewerken", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is not Product sel)
            {
                MessageBox.Show("Selecteer een product om te verwijderen.", "Geen selectie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Weet u zeker dat u '{sel.Name}' wilt verwijderen?", "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _manager.Remove(sel.Id);
                RefreshGrid();
                ClearInputs();
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product sel)
            {
                NameBox.Text = sel.Name;
                CategoryBox.Text = sel.Category;
                QuantityBox.Text = sel.Quantity.ToString();

                var unitItem = UnitBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item =>
                    item.Content.ToString().Equals(sel.Unit, StringComparison.OrdinalIgnoreCase));
                if (unitItem != null) UnitBox.SelectedItem = unitItem;

                if (sel is PerishableProduct pp)
                    ExpirationPicker.SelectedDate = pp.ExpirationDate;
                else
                    ExpirationPicker.SelectedDate = null;
            }
        }


        private void ClearInputs()
        {
            NameBox.Clear();
            CategoryBox.Clear();
            QuantityBox.Clear();
            ExpirationPicker.SelectedDate = null;
        }

        private void OnListChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListSelector.SelectedItem is string name)
            {
                _manager.SwitchList(name);
                RefreshGrid();
            }
        }

        private void OnCreateListClicked(object sender, RoutedEventArgs e)
        {
            string name = NewListBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                _manager.CreateList(name);
                ListSelector.ItemsSource = _manager.ListNames.ToList();
                ListSelector.SelectedItem = name;
                NewListBox.Clear();
            }
            else
            {
                MessageBox.Show("Geef een naam op voor de nieuwe lijst.", "Lege naam", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshUI()
        {
            ListSelector.ItemsSource = _manager.ListNames.ToList();
            ListSelector.SelectedItem = _manager.CurrentListName;
            RefreshGrid(SearchBox.Text);
        }



    }
}
