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
        public string m_FilePath;
        public MainWindow()
        {
            InitializeComponent();
        }

        public List<GammaRay> GetData()
        {
            var result = new List<GammaRay>();
            var context = new AppDbContext();
            foreach (var item in context.GammaRay)
            {
                result.Add(new GammaRay
                {
                    DataStamp = item.DataStamp,
                    MD = item.MD,
                    GRBX = item.GRBX
                });
            }
            recordsCount.Content = result.Count().ToString();
            return result;
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
                for (int j = 0; j < Math.Round(interpolateCount); j++)
                {
                    result.Add(new GammaRay() { 
                        DataStamp = result[result.Count -1].DataStamp + timeStep,
                        MD = result[result.Count - 1].MD + step,
                        GRBX = result[result.Count - 1].GRBX + grbxStep
                    });
                }
            }
            result.Add(gammaRay[gammaRay.Count - 1]);
            return result;
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(m_FilePath)) return;
            var input = GetData();
            input = Interpolate(0.1, input);
            var output = new List<OutputData>();
            foreach (var item in input)
            {
                output.Add(new OutputData() { 
                    MD = (float)item.MD,
                    DataStamp = ((DateTimeOffset)item.DataStamp).ToUnixTimeSeconds(),
                    GRBX = (float)item.GRBX
                });
            }
            using (StreamWriter sw = new StreamWriter(m_FilePath, false, System.Text.Encoding.Default))
            {
                var writer = new CsvWriter(sw, System.Globalization.CultureInfo.CurrentCulture);
                writer.WriteRecords(output);
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
