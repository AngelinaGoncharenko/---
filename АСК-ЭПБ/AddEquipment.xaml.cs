using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace АСК_ЭПБ
{
    /// <summary>
    /// Логика взаимодействия для AddEquipment.xaml
    /// </summary>
    public partial class AddEquipment : Window
    {
        private Entities context;

        public AddEquipment()
        {
            InitializeComponent();
            context = new Entities();
            LoadDataEquipment();
            this.WindowState = WindowState.Maximized;
        }

        private void LoadDataEquipment()
        {
            dataGridEquipment.ItemsSource = context.GetEquipment().ToList();

            var equipmentStates = context.EQUIPMENT_STATE.ToList();
            ComboBoxEquipment.ItemsSource = equipmentStates;
            ComboBoxEquipment.DisplayMemberPath = "NAME_EQUIPMENT_STATE";
            ComboBoxEquipment.SelectedValuePath = "EQUIPMENT_STATEID";
        }

        private void Click_EquipmentCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void RefreshDataGrids()
        {
            string selectedStatus = (statusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedStatus) || selectedStatus == "Все состояния")
            {
                dataGridEquipment.ItemsSource = context.GetEquipment().ToList();
            }
            else
            {
                dataGridEquipment.ItemsSource = context.GetEquipment().Where(e => e.EquipmentState == selectedStatus).ToList();
            }
        }
        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDataGrids();
        }

        private void Equipment_Document(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                FileName.Content = fileName;
            }
        }

        private void SaveEquipment(object sender, RoutedEventArgs e)
        {
            try
            {
                    var selectedEquipmentStateId = (int)ComboBoxEquipment.SelectedValue;
                    var equipmentActivity = EquipmentActivity.Text;

                    var existingEquipmentActivity = context.EQUIPMENT_ACTIVITY.FirstOrDefault(wt => wt.NAME_EQUIPMENT_ACTIVITY == equipmentActivity);
                    int selectedEquipmentActivityId;
                    if (existingEquipmentActivity != null)
                    {
                        selectedEquipmentActivityId = existingEquipmentActivity.EQUIPMENT_ACTIVITYID;
                    }
                    else
                    {
                        var newEquipmentActivity = new EQUIPMENT_ACTIVITY
                        {
                            NAME_EQUIPMENT_ACTIVITY = equipmentActivity
                        };
                        context.EQUIPMENT_ACTIVITY.Add(newEquipmentActivity);
                        context.SaveChanges();
                        selectedEquipmentActivityId = newEquipmentActivity.EQUIPMENT_ACTIVITYID;
                    }

                    EQUIPMENT newEquipment = new EQUIPMENT
                    {
                        NAME_EQUIPMENT = NameEquipment.Text,
                        TECHNICAL_DOCUMENTATION = FileName.Content.ToString(),
                        EQUIPMENT_STATEID = selectedEquipmentStateId,
                        EQUIPMENT_ACTIVITYID = selectedEquipmentActivityId,
                    };

                    context.EQUIPMENT.Add(newEquipment);
                    context.SaveChanges();
                    MessageBox.Show("Оборудование успешно добавлено.");
                    LoadDataEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }


        private void DownloadDocumentEquipment_Click(object sender, RoutedEventArgs e)
        {
            var selectedEquipment = dataGridEquipment.SelectedItem as GetEquipment_Result;
            if (selectedEquipment != null)
            {
                var filePath = selectedEquipment.TECHNICAL_DOCUMENTATION;
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
                            MessageBox.Show("Техническая документация успешно сохранена.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении технической документации: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Техническая документация не найдена.");
                }
            }
            else
            {
                MessageBox.Show("Выберите оборудование для загрузки технической документации.");
            }
        }


        private void DeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridEquipment.SelectedItem != null)
            {
                try
                {
                    var selectedEquipment = (АСК_ЭПБ.GetEquipment_Result)dataGridEquipment.SelectedItem;
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить оборудование '{selectedEquipment.NAME_EQUIPMENT}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var context = new Entities())
                        {
                            var equipmentToDelete = context.EQUIPMENT.FirstOrDefault(eq => eq.EQUIPMENTID == selectedEquipment.EQUIPMENTID);

                            if (equipmentToDelete != null)
                            {
                                context.EQUIPMENT.Remove(equipmentToDelete);
                                context.SaveChanges();
                                dataGridEquipment.ItemsSource = context.GetEquipment().ToList();

                                MessageBox.Show("Оборудование успешно удалено.");
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NameEquipment.Text = "";
            ComboBoxEquipment.SelectedIndex = -1;
            EquipmentActivity.Text = "";
        }

        private void PrintEquipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false; PrintDialog pD = new PrintDialog();
                if (pD.ShowDialog() == true)
                {
                    pD.PrintVisual(this, "Отчет. Обьекты ЭПБ");
                }
            }
            finally
            { this.IsEnabled = true; }
        }



        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (var context = new Entities())
            {
                var searchText = searchEquipmentTextBox.Text;
                var searchResult = context.GetEquipment()
                    .Where(x =>
                        x.EQUIPMENTID.ToString().Contains(searchText) ||
                        x.NAME_EQUIPMENT.Contains(searchText) ||
                        x.TECHNICAL_DOCUMENTATION.Contains(searchText) ||
                        x.EquipmentState.Contains(searchText) ||
                        x.EquipmentActivity.Contains(searchText)
                    ).ToList();

                dataGridEquipment.ItemsSource = searchResult;
            }
        }

        private void SearchEquipment_Cancel(object sender, RoutedEventArgs e)
        {
            searchEquipmentTextBox.Text = "";

            using (var context = new Entities())
            {
                dataGridEquipment.ItemsSource = context.GetEquipment().ToList();
            }
        }

        private void Click_Diagram(object sender, RoutedEventArgs e)
        {
            PrintDiagram printDiagram = new PrintDiagram();
            printDiagram.Show();
        }
    }
}
