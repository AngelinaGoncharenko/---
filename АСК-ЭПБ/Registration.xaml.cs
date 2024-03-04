using System;
using System.Data.SqlClient;
using System.Windows;

namespace АСК_ЭПБ
{
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void SaveUser(object sender, RoutedEventArgs e)
        {
            try
            {
                
                using (SqlConnection connection = new SqlConnection("server=angelina;initial catalog=АСК-ЭПБ;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"))
                {
                    connection.Open();

                    using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM Users WHERE EMAIL = @Email OR PASSWORD = @Password", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Email", EmailUser.Text);
                        checkCommand.Parameters.AddWithValue("@Password", PasswordName.Text);

                        int existingUserCount = (int)checkCommand.ExecuteScalar();

                        if (existingUserCount > 0)
                        {
                            MessageBox.Show("Пользователь с такой почтой или паролем уже существует.");
                            return;
                        }
                    }

                    using (SqlCommand command = new SqlCommand(
                        "INSERT INTO REGISTRATION_REQUST (EMAIL_REQUST, PASSWORD_REQUST, FIRSTNAME_REQUST, LASTNAME_REQUST, MIDDLENAME_REQUST) " +
                        "VALUES (@Email, @Password, @FirstName, @LastName, @MiddleName)", connection))
                    {
                        command.Parameters.AddWithValue("@Email", EmailUser.Text);
                        command.Parameters.AddWithValue("@Password", PasswordName.Text);
                        command.Parameters.AddWithValue("@FirstName", Login.Text);
                        command.Parameters.AddWithValue("@LastName", Password.Text);
                        command.Parameters.AddWithValue("@MiddleName", MiddleName.Text);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Ваши данные на обработке. Ожидайте уведомление об активации аккаунта.");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }


        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
