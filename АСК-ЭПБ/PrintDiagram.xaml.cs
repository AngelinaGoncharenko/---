using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.EntityFrameworkCore;

namespace АСК_ЭПБ
{
    public partial class PrintDiagram : Window
    {
        private Entities context;
        private IQueryable<EQUIPMENT> equipmentWithRequests;

        public PrintDiagram()
        {
            InitializeComponent();
            context = new Entities();
            ChartService.ChartAreas.Add(new ChartArea("Main"));
            var currentSeries = new Series()
            {
                IsValueShownAsLabel = true
            };
            ChartService.Series.Add(currentSeries);

            CmbChartTypes.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

            CmbEquipment.DisplayMemberPath = "NAME_EQUIPMENT";

            equipmentWithRequests = GetEquipmentWithRequests();

            CmbEquipment.ItemsSource = equipmentWithRequests.ToList();

            CmbChartTypes.SelectionChanged += ComboBoxChartType_SelectionChanged;
            CmbEquipment.SelectionChanged += CmbEquipment_SelectionChanged;
        }

        private IQueryable<EQUIPMENT> GetEquipmentWithRequests()
        {
            return context.EQUIPMENT
                .Where(e => context.WORK_REQUEST.Any(wr => wr.EQUIPMENTID == e.EQUIPMENTID))
                .AsQueryable();
        }

        private void UpdateChart()
        {
            if (CmbEquipment.SelectedItem is EQUIPMENT selectedEquipment &&
                    CmbChartTypes.SelectedItem is SeriesChartType selectedChartType)
                {
                    Series currentSeries = ChartService.Series.FirstOrDefault();
                    currentSeries.ChartType = selectedChartType;
                    currentSeries.Points.Clear();

                    Debug.WriteLine($"Selected Equipment: {selectedEquipment.NAME_EQUIPMENT}");
                    Debug.WriteLine($"Selected Chart Type: {selectedChartType}");

                    var workRequests = context.WORK_REQUEST
                        .Where(wr => wr.EQUIPMENTID == selectedEquipment.EQUIPMENTID)
                        .ToList();

                    foreach (var workRequest in workRequests)
                    {
                        double value = workRequest.DATA_CREATION.Year;

                        Debug.WriteLine($"Value: {value}, Count: {workRequests.Count}");
                        currentSeries.Points.AddXY(value, workRequests.Count);
                    }
                }
        }

        private void ComboBoxChartType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateChart();
        }

        private void CmbEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateChart();
        }

        private void Print(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                PrintDialog pD = new PrintDialog();
                if (pD.ShowDialog() == true)
                {
                    pD.PrintVisual(this, "Отчет. Кол-во заявок по ЭПБ");
                }
            }
            finally
            {
                this.IsEnabled = true;
            }
        }
    }
}
