using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace АСК_ЭПБ
{
    /// <summary>
    /// Логика взаимодействия для BaseWindow.xaml
    /// </summary>
    public partial class BaseWindow : Window
    {
        private Entities _context;
        private Users currentUser;

        public BaseWindow(Users user)
        {
            InitializeComponent();
            _context = new Entities();
            currentUser = user;
            LoadData();
            this.WindowState = WindowState.Maximized;
            CheckUserRole();
        }

        public void CheckUserRole()
        {
            var userRole = _context.UserRoles.FirstOrDefault(ur => ur.USERROLEID == currentUser.USERID);

            if (userRole != null)
            {
                if (userRole.USERROLEID == 1) // Администратор
                {
                    UnlockAllButtons();
                }
                else if (userRole.USERROLEID == 2) // Директор
                {
                    UnlockDirectorButtons();
                }
                else if (userRole.USERROLEID == 3) // Оператор
                {
                    UnlockOperatorButtons();
                }
            }

        }

        private void UnlockAllButtons()
        {
            btnCreate.IsEnabled = true;
            btnEquipment.IsEnabled = true;
            btnContractor.IsEnabled = true;
            btnUser.IsEnabled = true;
            btnDelete.IsEnabled = true;
        }

        private void UnlockDirectorButtons()
        {
            btnCreate.IsEnabled = true;
            btnEquipment.IsEnabled = true;
            btnContractor.IsEnabled = true;
            btnUser.IsEnabled = false;
            btnDelete.IsEnabled = true;
        }

        private void UnlockOperatorButtons()
        {
            btnCreate.IsEnabled = true;
            btnEquipment.IsEnabled = false;
            btnContractor.IsEnabled = false;
            btnUser.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        public void LoadData()
        {
            dataGrid.ItemsSource = _context.GetWorkRequests().ToList();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            Create createRequest = new Create();
            createRequest.ShowDialog();
        }
        private void Equipment_Click(object sender, RoutedEventArgs e)
        {
            AddEquipment addEquipment = new AddEquipment();
            addEquipment.ShowDialog();
        }

        private void Contractor_Click(object sender, RoutedEventArgs e)
        {
            AddContractor addContractor = new AddContractor();
            addContractor.ShowDialog();
        }
        private void User_Click(object sender, RoutedEventArgs e)
        {
            UsersWindow usersWindow = new UsersWindow();
            usersWindow.ShowDialog();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                try
                {
                    var selectedWorkRequest = (АСК_ЭПБ.GetWorkRequests_Result)dataGrid.SelectedItem;
                    var confirmationMessage = $"Вы уверены, что хотите удалить заявку {selectedWorkRequest.WORK_REQUESTID}?";

                    var result = MessageBox.Show(confirmationMessage, "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var context = new Entities())
                        {
                            var docsToDelete = context.DOCUMENTATION.Where(doc => doc.WORK_REQUESTID == selectedWorkRequest.WORK_REQUESTID);
                            context.DOCUMENTATION.RemoveRange(docsToDelete);

                            var requestToDelete = context.WORK_REQUEST.FirstOrDefault(wr => wr.WORK_REQUESTID == selectedWorkRequest.WORK_REQUESTID);
                            if (requestToDelete != null)
                            {
                                context.WORK_REQUEST.Remove(requestToDelete);
                                context.SaveChanges();
                                dataGrid.ItemsSource = context.GetWorkRequests().ToList();
                                MessageBox.Show("Заявка успешно удалена.");
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
        private void FilterEquipment_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new Entities())
            {
                var distinctEquipment = context.GetWorkRequests()
                    .Select(x => x.NAME_EQUIPMENT)
                    .Distinct()
                    .ToList();

                var equipmentWindow = new FilterWindow(distinctEquipment);
                equipmentWindow.ShowDialog();

                var selectedEquipment = equipmentWindow.SelectedFilter;
                if (!string.IsNullOrEmpty(selectedEquipment))
                {
                    var filteredWorkRequests = context.GetWorkRequests()
                        .Where(x => x.NAME_EQUIPMENT == selectedEquipment)
                        .ToList();

                    dataGrid.ItemsSource = filteredWorkRequests;
                }
            }
        }

        private void FilterContractor_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new Entities())
            {
                var distinctContractors = context.GetWorkRequests()
                    .Select(x => x.NAME_CONTRACTOR)
                    .Distinct()
                    .ToList();

                var contractorWindow = new FilterWindow(distinctContractors);
                contractorWindow.ShowDialog();

                var selectedContractor = contractorWindow.SelectedFilter;
                if (!string.IsNullOrEmpty(selectedContractor))
                {
                    var filteredWorkRequests = context.GetWorkRequests()
                        .Where(x => x.NAME_CONTRACTOR == selectedContractor)
                        .ToList();

                    dataGrid.ItemsSource = filteredWorkRequests;
                }
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (var context = new Entities())
            {
                var searchText = searchTextBox.Text;
                var searchResult = context.GetWorkRequests()
                    .Where(x =>
                        x.WORK_REQUESTID.ToString().Contains(searchText) ||
                        x.DATA_CREATION.ToString().Contains(searchText) ||
                        x.URL_WORK_REQUEST.Contains(searchText) ||
                        x.URL_WORK_REQUEST.Contains(searchText) ||
                        x.NAME_EQUIPMENT.Contains(searchText) ||
                        x.NAME_WORK_TYPE.Contains(searchText) ||
                        x.NAME_REQUEST_STATUS.Contains(searchText) ||
                        x.NAME_CONTRACTOR.Contains(searchText)
                    ).ToList();

                dataGrid.ItemsSource = searchResult;
            }
        }

        private void Search_Cancel(object sender, RoutedEventArgs e)
        {
            searchTextBox.Text = "";

            using (var context = new Entities())
            {
                dataGrid.ItemsSource = context.GetWorkRequests().ToList();
            }
        }


        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            try
            {
                var workRequestsData = GetWorkRequests().ToList();

                if (workRequestsData.Any())
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("WorkRequests");

                        // Заголовки таблицы
                        var headers = new List<string>
                        {
                            "WorkRequestID", "DataCreation", "URLWorkRequest",
                            "NameEquipment", "NameWorkType", "NameRequestStatus",
                            "NameContractor"
                        };

                        for (int i = 1; i <= headers.Count; i++)
                        {
                            worksheet.Cell(1, i).Value = headers[i - 1];
                        }

                        // Данные таблицы
                        for (int i = 0; i < workRequestsData.Count; i++)
                        {
                            var workRequest = workRequestsData[i];

                            worksheet.Cell(i + 2, 1).Value = workRequest.WORK_REQUESTID;
                            worksheet.Cell(i + 2, 2).Value = workRequest.DATA_CREATION.ToString("yyyy-MM-dd");
                            worksheet.Cell(i + 2, 3).Value = workRequest.URL_WORK_REQUEST;
                            worksheet.Cell(i + 2, 4).Value = workRequest.NAME_EQUIPMENT;
                            worksheet.Cell(i + 2, 5).Value = workRequest.NAME_WORK_TYPE;
                            worksheet.Cell(i + 2, 6).Value = workRequest.NAME_REQUEST_STATUS;
                            worksheet.Cell(i + 2, 7).Value = workRequest.NAME_CONTRACTOR;
                        }

                        // Сохранение файла Excel
                        var saveFileDialog = new SaveFileDialog
                        {
                            Filter = "Excel files (*.xlsx)|*.xlsx",
                            DefaultExt = "xlsx",
                            FileName = "WorkRequests"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            workbook.SaveAs(saveFileDialog.FileName);
                            MessageBox.Show("Export to Excel completed successfully.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No data to export.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during export to Excel: {ex.Message}");
            }
        }

        private IEnumerable<WorkRequestExportModel> GetWorkRequests()
        {
            var query = _context.GetWorkRequests();

            //модель для экспорта
            var result = query.Select(wr => new WorkRequestExportModel
            {
                WORK_REQUESTID = wr.WORK_REQUESTID,
                DATA_CREATION = wr.DATA_CREATION,
                URL_WORK_REQUEST = wr.URL_WORK_REQUEST,
                NAME_EQUIPMENT = wr.NAME_EQUIPMENT,
                NAME_WORK_TYPE = wr.NAME_WORK_TYPE,
                NAME_REQUEST_STATUS = wr.NAME_REQUEST_STATUS,
                NAME_CONTRACTOR = wr.NAME_CONTRACTOR
            });

            return result;
        }

        private void DownloadDocument_Click(object sender, RoutedEventArgs e)
        {
            var selectedWorkRequest = dataGrid.SelectedItem as GetWorkRequests_Result;
            if (selectedWorkRequest != null)
            {
                var filePath = selectedWorkRequest.URL_WORK_REQUEST;
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
                            MessageBox.Show("Документ успешно сохранен.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении файла: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Документ не найден.");
                }
            }
        }

        private void Output_Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
