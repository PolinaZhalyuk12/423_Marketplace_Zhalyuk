using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            this.IsVisibleChanged += Page_IsVisibleChanged;
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                DataGridCategory.ItemsSource = db.Category.ToList();
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
                LoadData();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var cat = (sender as Button)?.DataContext as Category;
            if (cat == null) return;
            NavigationService?.Navigate(new AddCategoryPage(cat));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridCategory.SelectedItems.Cast<Category>().ToList();
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
                    foreach (var c in selected)
                    {
                        var entity = db.Category.Find(c.ID);
                        if (entity != null) db.Category.Remove(entity);
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