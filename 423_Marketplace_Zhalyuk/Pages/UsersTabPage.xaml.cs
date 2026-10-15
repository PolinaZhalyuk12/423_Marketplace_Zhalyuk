using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            this.IsVisibleChanged += Page_IsVisibleChanged;
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                DataGridUser.ItemsSource = db.User
                    .Include(u => u.Role)
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
            NavigationService?.Navigate(new AddUserPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button)?.DataContext as User;
            if (user == null) return;
            NavigationService?.Navigate(new AddUserPage(user));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridUser.SelectedItems.Cast<User>().ToList();
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
                    foreach (var u in selected)
                    {
                        var entity = db.User.Find(u.ID);
                        if (entity != null) db.User.Remove(entity);
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