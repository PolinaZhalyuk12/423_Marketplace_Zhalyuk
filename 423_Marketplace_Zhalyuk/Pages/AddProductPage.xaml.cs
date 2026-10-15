using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class AddProductPage : Page
    {
        private Product _currentProduct = new Product();

        public AddProductPage(Product selectedProduct)
        {
            InitializeComponent();

            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                CBCategory.ItemsSource = db.Category.ToList();
                CBUser.ItemsSource = db.User.ToList();
            }

            if (selectedProduct != null)
            {
                _currentProduct = selectedProduct;

                CBCategory.SelectedItem = CBCategory.Items
                    .Cast<Category>()
                    .FirstOrDefault(c => c.ID == _currentProduct.CategoryID);

                CBUser.SelectedItem = CBUser.Items
                    .Cast<User>()
                    .FirstOrDefault(u => u.ID == _currentProduct.SellerID);
            }

            DataContext = _currentProduct;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentProduct.Name))
                errors.AppendLine("Укажите название!");
            if (CBCategory.SelectedItem == null)
                errors.AppendLine("Выберите категорию!");
            if (CBUser.SelectedItem == null)
                errors.AppendLine("Выберите продавца!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            _currentProduct.CategoryID = ((Category)CBCategory.SelectedItem).ID;
            _currentProduct.SellerID = ((User)CBUser.SelectedItem).ID;

            try
            {
                using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
                {
                    if (_currentProduct.ID == 0)
                    {
                        _currentProduct.CreatedAt = DateTime.Now;
                        db.Product.Add(_currentProduct);
                    }
                    else
                    {
                        db.Entry(_currentProduct).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBName.Text = "";
            TBPrice.Text = "";
            TBQuantity.Text = "";
            CBCategory.SelectedItem = null;
            CBUser.SelectedItem = null;
        }
    }
}