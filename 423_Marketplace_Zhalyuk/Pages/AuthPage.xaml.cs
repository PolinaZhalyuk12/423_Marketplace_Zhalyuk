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
                    failedAttempts++;
                    MessageBox.Show(
                        $"Пользователь с такими данными не найден!\nНеудачных попыток: {failedAttempts} из 3.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                    if (failedAttempts >= 3)
                    {
                        CaptchaSwitch();
                        CaptchaChange();
                    }
                    return;
                }

                MessageBox.Show(
                    $"Пользователь успешно найден! Добро пожаловать, {user.FIO}",
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

        public void CaptchaSwitch()
        {
            switch (CaptchaPanel.Visibility)
            {
                case Visibility.Visible:
                    TextBoxLogin.Clear();
                    PasswordBox.Clear();

                    CaptchaPanel.Visibility = Visibility.Collapsed;
                    AuthPanel.Visibility = Visibility.Visible;
                    return;

                case Visibility.Collapsed:
                    CaptchaPanel.Visibility = Visibility.Visible;
                    AuthPanel.Visibility = Visibility.Collapsed;
                    return;
            }
        }

        public void CaptchaChange()
        {
            string allowchar = "";
            allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += ",a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += ",1,2,3,4,5,6,7,8,9,0";

            char[] a = { ',' };
            string[] ar = allowchar.Split(a);
            string pwd = "";
            string temp = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];
                pwd += temp;
            }
            captcha.Text = pwd;
            captchaInput.Clear();
        }
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CaptchaChange();
                return;
            }

            failedAttempts = 0;
            CaptchaSwitch();
            MessageBox.Show("Капча пройдена! Теперь попробуйте войти снова.",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste ||
                e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true; 
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