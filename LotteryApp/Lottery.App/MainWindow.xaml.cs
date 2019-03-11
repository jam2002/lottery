using Lottery.Core.Plan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Lottery.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlanConfig config;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plans.json");
            using (StreamReader sr = new StreamReader(path))
            {
                string content = sr.ReadToEnd();
                config = JsonConvert.DeserializeObject<PlanConfig>(content);
            }

            for (int i = 0; i < config.Common.Length; i++)
            {
                int c = i;
                config.Common[i].Dispatcher = (u, v) => UpdateUI(c, u, v);
            }

            listView.ItemsSource = new ObservableCollection<Dynamic23>(config.Common);

            Dictionary<string, IPlan> dic = config.Common.OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
            PlanInvoker.Current.Init(dic);
        }

        private void UpdateUI(int index, string desc, string value)
        {
            this.Dispatcher.Invoke(() =>
            {
                var c = config.Common[index];
                c.Value = value ?? string.Empty;

                if (DateTime.Now.Minute == 30)
                {
                    c.Desc = desc;
                }
                else
                {
                    c.Desc += desc + Environment.NewLine;
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PlanInvoker.Current.Close();
        }
    }
}
