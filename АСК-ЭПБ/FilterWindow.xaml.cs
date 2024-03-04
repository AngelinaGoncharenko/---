using System.Collections.Generic;
using System.Windows;

namespace АСК_ЭПБ
{
    /// <summary>
    /// Логика взаимодействия для FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        public string SelectedFilter { get; private set; }

        public FilterWindow(List<string> filterOptions)
        {
            InitializeComponent();
            filterComboBox.ItemsSource = filterOptions;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            SelectedFilter = filterComboBox.SelectedItem as string;
            Close();
        }
    }
}
