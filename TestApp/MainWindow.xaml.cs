using CsvHelper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Windows;

using TestApp.Models;

namespace TestApp
{
    public partial class MainWindow : Window
    {
        private List<GammaRay> m_GammaRay;
        private List<OutputData> m_OutputData;

        public string m_FilePath;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void GetData()
        {
            m_GammaRay = new List<GammaRay>();
            var context = new AppDbContext();
            foreach (var item in context.GammaRay)
            {
                m_GammaRay.Add(new GammaRay
                {
                    DataStamp = item.DataStamp,
                    MD = item.MD,
                    GRBX = item.GRBX
                });
            }
            recordsCount.Content = m_GammaRay.Count().ToString();
        }

        public static List<GammaRay> Interpolate(double step, List<GammaRay> gammaRay)
        {
            var result = new List<GammaRay>();
            for (int i = 0; i < gammaRay.Count - 1; i++)
            {
                result.Add(gammaRay[i]);
                var a = gammaRay[i];
                var b = gammaRay[i + 1];
                var timeStep = (b.DataStamp - a.DataStamp) / (b.MD - a.MD) * step;
                var grbxStep = (b.GRBX - a.GRBX) / (b.MD - a.MD) * step;

                var interpolateCount = (b.MD - a.MD) / step - 1;
                for (int j = 0; j < interpolateCount; j++)
                {
                    result.Add(new GammaRay() { 
                        DataStamp = result[result.Count -1].DataStamp + timeStep,
                        MD = result[result.Count - 1].MD + step,
                        GRBX = result[result.Count - 1].GRBX + grbxStep
                    });
                }
            }
            return result;
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(m_FilePath)) return;
            GetData();
            m_GammaRay = Interpolate(0.1, m_GammaRay);
            m_OutputData = new List<OutputData>();
            foreach (var item in m_GammaRay)
            {
                m_OutputData.Add(new OutputData() { 
                    MD = (float)item.MD,
                    DataStamp = ((DateTimeOffset)item.DataStamp).ToUnixTimeSeconds(),
                    GRBX = (float)item.GRBX
                });
            }
            using (StreamWriter sw = new StreamWriter(m_FilePath, true))
            {
                var writer = new CsvWriter(sw, System.Globalization.CultureInfo.CurrentCulture);
                writer.WriteRecords(m_OutputData);
                sw.Close();
            }
            MessageBox.Show("Data succesfuly saved", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog() { 
                Filter = "csv files (*.csv)|*.csv"
            };
            if (dlg.ShowDialog() == true)
            {
                m_FilePath = dlg.FileName;
                filePath.Content = m_FilePath;
            }
        }
    }
}
