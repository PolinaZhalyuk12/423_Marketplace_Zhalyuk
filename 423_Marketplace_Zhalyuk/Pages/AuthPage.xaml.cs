using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
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

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHintLogin.Visibility = string.IsNullOrEmpty(TextBoxLogin.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintPass.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) ||
                string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == TextBoxLogin.Text
                                      && u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show($"Пользователь успешно найден! Добро пожаловать, {user.FIO}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                string roleName = user.Role?.Name ?? "";
                switch (roleName)
                {
                    case "Admin":
                        NavigationService?.Navigate(new AdminPage());
                        break;
                    case "Seller":
                        NavigationService?.Navigate(new UserPage());
                        break;
                    default:
                        NavigationService?.Navigate(new UserPage());
                        break;
                }
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }
        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }
    }
}