using System;
using System.Windows;
using System.Windows.Threading;

namespace _423_Marketplace_Zhalyuk
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = false;

        public MainWindow()
        {
            InitializeComponent();

            string savedTheme = Properties.Settings.Default.Theme;
            if (savedTheme == "Dark")
            {
                _isDarkTheme = true;
                var uri = new Uri("DictionaryDark.xaml", UriKind.Relative);
                ResourceDictionary dict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }

            MainFrame.Navigate(new Pages.AuthPage());
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                DateTimeNow.Text = DateTime.Now.ToString();
            };
            timer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?",
                    "Message", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            string dictionary = _isDarkTheme ? "DictionaryDark.xaml" : "Dictionary.xaml";

            var uri = new Uri(dictionary, UriKind.Relative);

            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

            Application.Current.Resources.Clear();

            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

            Properties.Settings.Default.Theme = _isDarkTheme ? "Dark" : "Light";
            Properties.Settings.Default.Save();
        }
    }
}