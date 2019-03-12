using Lottery.Core.Plan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

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

            listView.ItemsSource = new ObservableCollection<Dynamic23>(config.Common);

            Dictionary<string, IPlan> dic = config.Common.OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
            PlanInvoker.Current.Init(dic);
        }

        private void UpdateUI(int index, string desc, string value)
        {
            this.Dispatcher.Invoke(() =>
            {
                var c = config.Common[index];
                if (value != null)
                {
                    c.Value = value;

                    ListBoxItem item = listView.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
                    ContentPresenter cp = FindVisualChild<ContentPresenter>(item);
                    WindowsFormsHost host = cp.ContentTemplate.FindName("valueHost", cp) as WindowsFormsHost;
                    System.Windows.Forms.TextBox txtBox = host.Child as System.Windows.Forms.TextBox;
                    txtBox.Text = c.Value;
                }

                if (clearMinutes.Contains(DateTime.Now.Minute))
                {
                    c.Desc = desc;
                }
                else
                {
                    c.Desc += desc + Environment.NewLine;
                }
            });
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PlanInvoker.Current.Close();
        }
    }
}
