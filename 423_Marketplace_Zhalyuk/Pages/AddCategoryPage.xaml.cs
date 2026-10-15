using System;
using System.Windows;
using System.Windows.Controls;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class AddCategoryPage : Page
    {
        private Category _currentCategory = new Category();

        public AddCategoryPage(Category selectedCategory)
        {
            InitializeComponent();

            if (selectedCategory != null)
                _currentCategory = selectedCategory;

            DataContext = _currentCategory;
        }

        private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentCategory.Name))
            {
                MessageBox.Show("Укажите название категории!");
                return;
            }

            try
            {
                using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
                {
                    if (_currentCategory.ID == 0)
                        db.Category.Add(_currentCategory);
                    else
                        db.Entry(_currentCategory).State = System.Data.Entity.EntityState.Modified;

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
            TBCategoryName.Text = "";
        }
    }
}