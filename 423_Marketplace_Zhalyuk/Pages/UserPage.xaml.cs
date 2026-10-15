using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            UpdateUsers();
        }

        private void UpdateUsers()
        {
            if (!IsInitialized) return;

            try
            {
                using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
                {
                    List<User> currentUsers = db.User
                        .Include(u => u.Role)
                        .ToList();

                    if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                    {
                        string q = fioFilterTextBox.Text.ToLower();
                        currentUsers = currentUsers
                            .Where(x => x.FIO != null && x.FIO.ToLower().Contains(q))
                            .ToList();
                    }

                    if (onlyAdminCheckBox.IsChecked == true)
                    {
                        currentUsers = currentUsers
                            .Where(x => x.Role != null && x.Role.Name == "Admin")
                            .ToList();
                    }

                    if (sortComboBox.SelectedIndex == 0)
                        currentUsers = currentUsers.OrderBy(x => x.FIO).ToList();
                    else
                        currentUsers = currentUsers.OrderByDescending(x => x.FIO).ToList();

                    ListUser.ItemsSource = currentUsers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void clearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
        }
    }
}