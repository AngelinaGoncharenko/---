using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace АСК_ЭПБ
{
    /// <summary>
    /// Логика взаимодействия для Create.xaml
    /// </summary>
    public partial class Create : Window
    {
        public Create()
        {
            InitializeComponent();
            LoadComboBoxData();
        }

        private void AttachDocument_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                FileName.Content = fileName;
            }
        }
        private void LoadComboBoxData()
        {
            using (var context = new Entities())
            {
                var equipmentList = context.EQUIPMENT.Select(e => new { e.EQUIPMENTID, e.NAME_EQUIPMENT }).ToList();
                equipmentComboBox.ItemsSource = equipmentList;
                equipmentComboBox.DisplayMemberPath = "NAME_EQUIPMENT";
                equipmentComboBox.SelectedValuePath = "EQUIPMENTID";

                var contractorList = context.CONTRACTOR.Select(c => new { c.CONTRACTORID, c.NAME_CONTRACTOR }).ToList();
                contractorComboBox.ItemsSource = contractorList;
                contractorComboBox.DisplayMemberPath = "NAME_CONTRACTOR";
                contractorComboBox.SelectedValuePath = "CONTRACTORID";
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new Entities())
            {
                try
                {
                    var selectedEquipmentId = (int)equipmentComboBox.SelectedValue;
                    var selectedContractorId = (int)contractorComboBox.SelectedValue;
                    var urlWorkRequest = FileName.Content.ToString();
                    var workType = workTypeTextBox.Text;

                    if (string.IsNullOrEmpty(workType))
                    {
                        MessageBox.Show("Укажите вид планируемых работ");
                        return;
                    }

                    var existingWorkType = context.WORK_TYPE.FirstOrDefault(wt => wt.NAME_WORK_TYPE == workType);
                    int selectedWorkTypeId;

                    if (existingWorkType != null)
                    {
                        selectedWorkTypeId = existingWorkType.WORK_TYPEID;
                    }
                    else
                    {
                        var newWorkType = new WORK_TYPE
                        {
                            NAME_WORK_TYPE = workType
                        };
                        context.WORK_TYPE.Add(newWorkType);
                        context.SaveChanges();
                        selectedWorkTypeId = newWorkType.WORK_TYPEID;
                    }

                    var newWorkRequest = new WORK_REQUEST
                    {
                        EQUIPMENTID = selectedEquipmentId,
                        WORK_TYPEID = selectedWorkTypeId,
                        REQUEST_STATUSID = (int)context.REQUEST_STATUS.FirstOrDefault(rs => rs.NAME_REQUEST_STATUS == "Создана")?.REQUEST_STATUSID,
                        CONTRACTORID = selectedContractorId,
                        DATA_CREATION = DateTime.Now,
                        URL_WORK_REQUEST = urlWorkRequest
                    };

                    context.WORK_REQUEST.Add(newWorkRequest);
                    context.SaveChanges();

                    this.Close();

                    MessageBox.Show("Заявка успешно создана");

                    var baseWindow = Application.Current.Windows.OfType<BaseWindow>().FirstOrDefault();
                    if (baseWindow != null)
                    {
                        var dataGrid = (DataGrid)baseWindow.FindName("dataGrid");
                        if (dataGrid != null)
                        {
                            dataGrid.ItemsSource = context.GetWorkRequests().ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения: " + ex.Message);
                }
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
    }
}
