using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null)
                _currentUser = selectedUser;

            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                cmbRole.ItemsSource = db.Role.ToList();
            }

            DataContext = _currentUser;
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО!");
            if (cmbRole.SelectedItem == null)
                errors.AppendLine("Выберите роль!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            _currentUser.RoleID = ((Role)cmbRole.SelectedItem).ID;
            _currentUser.Role = null;

            if (!string.IsNullOrWhiteSpace(TBPass.Text) &&
                TBPass.Text.Length >= 6)
            {
                _currentUser.Password = GetHash(TBPass.Text);
            }

            try
            {
                using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
                {
                    if (_currentUser.ID == 0)
                    {
                        _currentUser.CreatedAt = DateTime.Now;
                        db.User.Add(_currentUser);
                    }
                    else
                    {
                        db.Entry(_currentUser).State = System.Data.Entity.EntityState.Modified;
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
            TBLogin.Text = "";
            TBPass.Text = "";
            TBFio.Text = "";
            TBEmail.Text = "";
            TBPhoto.Text = "";
            cmbRole.SelectedItem = null;
        }
    }
}