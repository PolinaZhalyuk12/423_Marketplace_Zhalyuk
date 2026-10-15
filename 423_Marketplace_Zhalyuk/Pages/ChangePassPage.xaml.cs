using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
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

        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = string.IsNullOrEmpty(TbLogin.Text)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintCurrent.Visibility = string.IsNullOrEmpty(CurrentPasswordBox.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintNew.Visibility = string.IsNullOrEmpty(NewPasswordBox.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintConfirm.Visibility = string.IsNullOrEmpty(ConfirmPasswordBox.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void HintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => TbLogin.Focus();

        private void HintCurrent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => CurrentPasswordBox.Focus();

        private void HintNew_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => NewPasswordBox.Focus();

        private void HintConfirm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => ConfirmPasswordBox.Focus();

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbLogin.Text) ||
                string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Все поля обязательны к заполнению!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedOld = GetHash(CurrentPasswordBox.Password);

            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                var user = db.User
                    .FirstOrDefault(u => u.Login == TbLogin.Text
                                      && u.Password == hashedOld);

                if (user == null)
                {
                    MessageBox.Show("Текущий пароль или логин неверный!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (NewPasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Новый пароль должен содержать не менее 6 символов!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool en = true;
                bool number = false;

                for (int i = 0; i < NewPasswordBox.Password.Length; i++)
                {
                    char c = NewPasswordBox.Password[i];

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

                if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Новые пароли не совпадают!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                user.Password = GetHash(NewPasswordBox.Password);
                db.SaveChanges();
            }

            MessageBox.Show("Пароль успешно изменён!",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            NavigationService?.Navigate(new AuthPage());
        }
    }
}