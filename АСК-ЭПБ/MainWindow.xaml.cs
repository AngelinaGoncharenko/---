using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace АСК_ЭПБ
{
    public partial class MainWindow : Window
    {
        private Entities _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new Entities();
        }

        private void Registration(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.ShowDialog();
        }
        private void ShowPasswordChecked(object sender, RoutedEventArgs e)
        {
            Password.Password = "\0"; // Отображение текста
        }

        private void ShowPasswordUnchecked(object sender, RoutedEventArgs e)
        {
            Password.Password = "●"; // Скрытие пароля
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Email.Text = " ";
            Password.Password = " ";
        }

        private void Input(object sender, RoutedEventArgs e)
        {
            string enteredLogin = Email.Text;
            string enteredPassword = Password.Password;

            var user = _context.Users.FirstOrDefault(u => u.EMAIL == enteredLogin);

            if (user != null)
            {
                if (user.PASSWORD == enteredPassword )
                {
                    BaseWindow baseWindow = new BaseWindow(user);
                    baseWindow.Show();
                    Close();
                }
                else
                {
                    Password.Password = "";
                    MessageBox.Show("Неверный пароль. Попробуйте снова.");
                }
                
            }
            else
            {
                Password.Password = "";
                Email.Text = "";
                MessageBox.Show("Пользователь не найден. Введите верные данные для входа или зарегистрируйтесь, чтобы продолжить.");
            }
        }
    }
}
