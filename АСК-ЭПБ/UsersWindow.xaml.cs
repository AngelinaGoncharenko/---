using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace АСК_ЭПБ
{
    public partial class UsersWindow : Window
    {
        private Entities _context;
        public UsersWindow()
        {
            InitializeComponent();
            _context = new Entities();
            RefreshDataGrids();
        
        }
        private void RoleComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox roleComboBox = sender as ComboBox;
            if (roleComboBox != null)
            {
                using (SqlConnection connection = new SqlConnection("server=angelina;initial catalog=АСК-ЭПБ;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT * FROM Roles", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem();
                                item.Content = reader["ROLENAME"].ToString();
                                item.Tag = reader["ROLEID"].ToString();
                                roleComboBox.Items.Add(item);
                            }
                        }
                    }
                }
            }
        }
        private void RefreshDataGrids()
        {
            dataGridUsers.ItemsSource = _context.GetUserInfo().ToList();
            dataGridUserRole.ItemsSource = _context.REGISTRATION_REQUST.ToList();

            string selectedRole = (roleFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedRole) || selectedRole == "Все роли")
            {
                dataGridUsers.ItemsSource = _context.GetUserInfo().ToList();
            }
            else
            {
                dataGridUsers.ItemsSource = _context.GetUserInfo().Where(u => u.Роль == selectedRole).ToList();
            }
        }
        private void RoleFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDataGrids();
        }


        private void Click_UsersCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsers.SelectedItem != null)
            {
                try
                {
                    var selectedUser = (GetUserInfo_Result)dataGridUsers.SelectedItem;
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{selectedUser.Фамилия} {selectedUser.Имя}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var context = new Entities())
                        {
                            // Удаляем запись из таблицы UserRoles
                            var userRole = context.UserRoles.FirstOrDefault(ur => ur.USERID == selectedUser.Код_пользователя);
                            if (userRole != null)
                            {
                                context.UserRoles.Remove(userRole);
                            }

                            // Удаляем запись из таблицы Users
                            var userToDelete = context.Users.FirstOrDefault(u => u.USERID == selectedUser.Код_пользователя);
                            if (userToDelete != null)
                            {
                                context.Users.Remove(userToDelete);
                                context.SaveChanges();
                                RefreshDataGrids(); // Обновляем DataGrid
                                MessageBox.Show("Пользователь успешно удален.");
                            }
                            else
                            {
                                MessageBox.Show("Не удалось найти выбранную строку для удаления.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.");
            }

        }
        private void DeleteRegistration_Click(object sender, RoutedEventArgs e)
        {
            var selectedRequest = dataGridUserRole.SelectedItem as REGISTRATION_REQUST;

            if (selectedRequest != null)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить заявку от {selectedRequest.LASTNAME_REQUST} {selectedRequest.FIRSTNAME_REQUST}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.REGISTRATION_REQUST.Remove(selectedRequest);
                    _context.SaveChanges();
                    RefreshDataGrids(); // Обновляем DataGrid
                    MessageBox.Show("Заявка успешно удалена.");
                }
            }
            else
            {
                MessageBox.Show("Выберите заявку для удаления.");
            }
        }


        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchUsersTextBox.Text.ToLower();

            // Фильтруем пользователей по тексту поиска
            var filteredUsers = _context.GetUserInfo().Where(u =>
                u.Фамилия.ToLower().Contains(searchText) ||
                u.Имя.ToLower().Contains(searchText) ||
                u.Отчество.ToLower().Contains(searchText) ||
                u.Почта.ToLower().Contains(searchText) ||
                u.Роль.ToLower().Contains(searchText)
            ).ToList();

            // Обновляем DataGrid с отфильтрованными пользователями
            dataGridUsers.ItemsSource = filteredUsers;
        }


        private void SearchUsers_Cancel(object sender, RoutedEventArgs e)
        {
            // Очищаем TextBox и обновляем DataGrid
            searchUsersTextBox.Text = "";
            RefreshDataGrids();
        }


        private void PrintUsers_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                this.IsEnabled = false; PrintDialog pD = new PrintDialog();
                if (pD.ShowDialog() == true)
                {
                    pD.PrintVisual(this, "Отчет. Пользователи");
                }
            }
            finally
            { this.IsEnabled = true; }
        }

        private void ApproveRegistration_Click(object sender, RoutedEventArgs e)
        {
            var selectedRequest = dataGridUserRole.SelectedItem as REGISTRATION_REQUST;

            if (selectedRequest != null)
            {
                Users newUser = new Users
                {
                    EMAIL = selectedRequest.EMAIL_REQUST,
                    PASSWORD = selectedRequest.PASSWORD_REQUST,
                    FIRSTNAME = selectedRequest.FIRSTNAME_REQUST,
                    LASTNAME = selectedRequest.LASTNAME_REQUST,
                    MIDDLENAME = selectedRequest.MIDDLENAME_REQUST
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                // Получаем выбранную ячейку из DataGrid
                DataGridCell cell = GetCell(dataGridUserRole, dataGridUserRole.SelectedIndex, 5);

                // Получаем ComboBox из содержимого ячейки
                ComboBox roleComboBox = FindVisualChild<ComboBox>(cell);

                if (roleComboBox != null && roleComboBox.SelectedItem != null)
                {
                    // Получаем содержимое выбранной ячейки ComboBoxItem
                    var selectedRoleItem = roleComboBox.SelectedItem as ComboBoxItem;

                    if (selectedRoleItem != null)
                    {
                        // Используем Tag, чтобы получить ROLEID
                        var roleId = selectedRoleItem.Tag;

                        if (roleId != null && int.TryParse(roleId.ToString(), out int roleIdValue))
                        {
                            UserRoles newUserRole = new UserRoles
                            {
                                USERID = newUser.USERID,
                                ROLEID = roleIdValue
                            };

                            _context.UserRoles.Add(newUserRole);
                            _context.SaveChanges();
                        }
                    }
                }
                _context.REGISTRATION_REQUST.Remove(selectedRequest);
                _context.SaveChanges();

                RefreshDataGrids();
            }
        }


        // Метод для получения ячейки в DataGrid
        private DataGridCell GetCell(DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            if (row != null)
            {
                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);

                if (presenter != null)
                {
                    return (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
            }
            return null;
        }

        // Метод для поиска дочернего элемента визуального дерева
        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);

                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
    }
}
