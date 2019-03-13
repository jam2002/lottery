﻿using Lottery.Core.Plan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace Lottery.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlanConfig config;
        private int[] clearMinutes = new int[] { 0, 20, 40 };

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

            Dictionary<string, IPlan> dic = config.Common.OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
            PlanInvoker.Current.Init(dic);
        }

        private void UpdateUI(int index, string desc, string value)
        {
            if (index < 10)
            {
                this.Dispatcher.Invoke(() =>
                {
                    TextBlock txtTitle = listView.FindName($"txtTitle{index + 1}") as TextBlock;
                    TextBox descBox = listView.FindName($"txtDesc{index + 1}") as TextBox;
                    WindowsFormsHost host = listView.FindName($"valueHost{index + 1}") as WindowsFormsHost;
                    System.Windows.Forms.TextBox valueBox = host.Child as System.Windows.Forms.TextBox;

                    var c = config.Common[index];
                    txtTitle.Text = c.Title;

                    if (value != null)
                    {
                        valueBox.Text = value;
                    }

                    if (clearMinutes.Contains(DateTime.Now.Minute))
                    {
                        descBox.Clear();
                    }

                    if (!string.IsNullOrEmpty(desc))
                    {
                        descBox.AppendText(desc);
                        descBox.AppendText(Environment.NewLine);
                    }
                });
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            PlanInvoker.Current.Close();
        }
    }
}
