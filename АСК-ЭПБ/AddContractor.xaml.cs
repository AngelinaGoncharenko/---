using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;

namespace АСК_ЭПБ
{
    /// <summary>
    /// Логика взаимодействия для AddContractor.xaml
    /// </summary>
    public partial class AddContractor : Window
    {
        private Entities context;

        public AddContractor()
        {
            InitializeComponent();
            context = new Entities();
            LoadDataContractors();
            this.WindowState = WindowState.Maximized;
        }

        private void LoadDataContractors()
        {
            dataGridContractor.ItemsSource = context.CONTRACTOR.ToList();
        }

        private void Click_ContractorCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Contractor_Document(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                FileName.Content = fileName;
            }
        }
        private void SaveContractor(object sender, RoutedEventArgs e)
        {
            try
            {
                CONTRACTOR newContractor = new CONTRACTOR
                {
                    NAME_CONTRACTOR = NameContractor.Text,
                    SUBCONTRACT = FileName.ToString(),
                    CONTACTS = Contacts.Text
                };

                context.CONTRACTOR.Add(newContractor);
                context.SaveChanges();

                MessageBox.Show("Подрядчик успешно добавлен.");
                LoadDataContractors();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
        private void Cancel_AddContractor(object sender, RoutedEventArgs e)
        {
            NameContractor.Text = "";
            Contacts.Text = "";
        }


        private void DeleteContractor_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridContractor.SelectedItem != null)
            {
                try
                {
                    var selectedContractor = (CONTRACTOR)dataGridContractor.SelectedItem;
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить подрядчика '{selectedContractor.NAME_CONTRACTOR}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var context = new Entities())
                        {
                            var contractorToDelete = context.CONTRACTOR.FirstOrDefault(c => c.CONTRACTORID == selectedContractor.CONTRACTORID);

                            if (contractorToDelete != null)
                            {
                                context.CONTRACTOR.Remove(contractorToDelete);
                                context.SaveChanges();
                                dataGridContractor.ItemsSource = context.CONTRACTOR.ToList();

                                MessageBox.Show("Подрядчик успешно удален.");
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
                MessageBox.Show("Выберите строку для удаления.");
            }
        }
        private void PrintContractor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                PrintDialog pD = new PrintDialog();
                if (pD.ShowDialog() == true)
                {
                    pD.PrintVisual(this, "Отчет. Подрядчики");
                }
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        private void SearchTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            using (var context = new Entities())
            {
                var searchText = searchContractorTextBox.Text;
                var searchResult = context.CONTRACTOR
                    .Where(x =>
                        x.CONTRACTORID.ToString().Contains(searchText) ||
                        x.NAME_CONTRACTOR.Contains(searchText) ||
                        x.SUBCONTRACT.Contains(searchText) ||
                        x.CONTACTS.Contains(searchText)
                    ).ToList();

                dataGridContractor.ItemsSource = searchResult;
            }
        }

        private void SearchContractor_Cancel(object sender, RoutedEventArgs e)
        {
            searchContractorTextBox.Text = "";

            using (var context = new Entities())
            {
                dataGridContractor.ItemsSource = context.CONTRACTOR.ToList();
            }
        }
        private void DownloadDocumentContractor_Click(object sender, RoutedEventArgs e)
        {
            var selectedContractor = dataGridContractor.SelectedItem as CONTRACTOR;
            if (selectedContractor != null)
            {
                var filePath = selectedContractor.SUBCONTRACT;
                if (!string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        var saveFileDialog = new SaveFileDialog();
                        saveFileDialog.FileName = System.IO.Path.GetFileName(filePath);
                        saveFileDialog.Filter = "All files (*.*)|*.*";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            File.Copy(filePath, saveFileDialog.FileName, true);
                            MessageBox.Show("Договор подряда успешно сохранен.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении договора подряда: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Договор подряда не найден.");
                }
            }
            else
            {
                MessageBox.Show("Выберите подрядчика для загрузки договора подряда.");
            }
        }

        private void EditContractor_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
