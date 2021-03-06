﻿using Lottery.Core.Plan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private int[] clearMinutes = new int[] { 15, 45 };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> titleDefine = new Dictionary<string, string>
            {
                { "qq", "QQ计划"},
                { "ts", "腾讯计划"},
                { "cq", "重庆计划"},
                { "xj", "新疆计划"},
                { "md", "美东计划"},
                { "dj", "东京计划"},
                { "gd", "广东计划"},
                { "jx", "江西计划"}
            };
            string lottery = ConfigurationManager.AppSettings.Get("lottery");
            this.Title = titleDefine[lottery];
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"plans.{lottery}.json");
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

            Dictionary<string, Dynamic> dic = config.Common.OfType<Dynamic>().ToDictionary(c => c.GetKey(), c => c);
            PlanInvoker.Current.Init(dic);
        }

        private void UpdateUI(int index, string desc, string value)
        {
            int count = listView.Items.Count + seclistView.Items.Count + thrlistView.Items.Count + othlistView.Items.Count;
            if (index < count)
            {
                this.Dispatcher.Invoke(() =>
                {
                    int uiIndex = index / 10;
                    string prefix = uiIndex == 0 ? string.Empty : (uiIndex == 1 ? "sec" : (uiIndex == 2 ? "thr" : "oth"));
                    int realIndex = index - uiIndex * 10 + 1;

                    ListView view = this.FindName($"{prefix}listView") as ListView;
                    TextBlock txtTitle = view.FindName($"{prefix}txtTitle{realIndex}") as TextBlock;
                    TextBox descBox = view.FindName($"{prefix}txtDesc{realIndex}") as TextBox;
                    WindowsFormsHost host = view.FindName($"{prefix}valueHost{realIndex}") as WindowsFormsHost;
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
