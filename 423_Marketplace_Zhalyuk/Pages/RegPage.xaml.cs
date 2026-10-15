using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBox.SelectedIndex = 0;
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

        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHint.Visibility = string.IsNullOrEmpty(txtbxLog.Text)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHint.Visibility = string.IsNullOrEmpty(passBxFrst.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHint.Visibility = string.IsNullOrEmpty(passBxScnd.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHint.Visibility = string.IsNullOrEmpty(txtbxFIO.Text)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void lblLogHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }

        private void lblPassHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }

        private void lblPassSecHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }

        private void lblFioHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxFIO.Focus();
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                var existingUser = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == txtbxLog.Text);

                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (passBxFrst.Password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать не менее 6 символов!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool en = true;
                bool number = false;

                for (int i = 0; i < passBxFrst.Password.Length; i++)
                {
                    char c = passBxFrst.Password[i];

                    if (c >= '0' && c <= '9')
                        number = true;
                    else if (!((c >= 'A' && c <= 'Z') ||
                               (c >= 'a' && c <= 'z')))
                        en = false;
                }

                if (!en)
                {
                    MessageBox.Show("Пароль должен содержать только английские буквы и цифры!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!number)
                {
                    MessageBox.Show("Пароль должен содержать хотя бы одну цифру!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (passBxFrst.Password != passBxScnd.Password)
                {
                    MessageBox.Show("Пароли не совпадают!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedRole = (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Buyer";

                var role = db.Role
                    .AsNoTracking()
                    .FirstOrDefault(r => r.Name == selectedRole);

                if (role == null)
                {
                    MessageBox.Show("Не удалось найти роль в базе данных!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newUser = new User
                {
                    Login = txtbxLog.Text,
                    Password = GetHash(passBxFrst.Password),
                    RoleID = role.ID,
                    FIO = txtbxFIO.Text,
                    CreatedAt = DateTime.Now
                };

                db.User.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Пользователь успешно зарегистрирован!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                txtbxLog.Clear();
                passBxFrst.Clear();
                passBxScnd.Clear();
                txtbxFIO.Clear();
                comboBox.SelectedIndex = 0;

                NavigationService?.Navigate(new AuthPage());
            }
        }
    }
}