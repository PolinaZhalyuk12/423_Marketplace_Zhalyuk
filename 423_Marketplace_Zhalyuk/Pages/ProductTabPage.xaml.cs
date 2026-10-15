using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class ProductTabPage : Page
    {
        public ProductTabPage()
        {
            InitializeComponent();
            this.IsVisibleChanged += Page_IsVisibleChanged;
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                DataGridProduct.ItemsSource = db.Product
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .ToList();
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
                LoadData();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddProductPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.DataContext as Product;
            if (product == null) return;
            NavigationService?.Navigate(new AddProductPage(product));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridProduct.SelectedItems.Cast<Product>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Выберите записи для удаления!");
                return;
            }

            if (MessageBox.Show($"Удалить записей: {selected.Count}?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
                {
                    foreach (var p in selected)
                    {
                        var entity = db.Product.Find(p.ID);
                        if (entity != null) db.Product.Remove(entity);
                    }
                    db.SaveChanges();
                }
                MessageBox.Show("Данные успешно удалены!");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}