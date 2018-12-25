using Lottery.Core.Plan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lottery.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plans.json");
            PlanConfig config = null;
            using (StreamReader sr = new StreamReader(path))
            {
                string content = sr.ReadToEnd();
                config = JsonConvert.DeserializeObject<PlanConfig>(content);
            }

            for (int i = 0; i < config.Common.Length; i++)
            {
                int c = i;
                string title = config.Common[i].Title;
                config.Common[i].Dispatcher = (u, v) => UpdateUI($"plan.{c}", title, u, v);
            }

            Dictionary<string, IPlan> dic = config.Common.OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
            PlanInvoker.Current.Init(dic);
        }

        private void UpdateUI(string code, string title, string desc, string value)
        {
            this.Dispatcher.Invoke(() =>
            {
                TextBlock titleBlock = null;
                RichTextBox descBox = null;
                System.Windows.Forms.RichTextBox valueBox = null;
                switch (code)
                {
                    case "plan.0":
                        titleBlock = this.txtFrontTitle;
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.1":
                        titleBlock = this.txtMiddleTitle;
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.2":
                        titleBlock = this.txtAfterTitle;
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.3":
                        titleBlock = this.txtOneAwardTitle;
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.4":
                        titleBlock = this.txtFiveTitle;
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.5":
                        titleBlock = this.txtFiftyTitle;
                        descBox = this.txtFiftyDesc;
                        valueBox = this.txtFiftyHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.6":
                        titleBlock = this.txtSixTitle;
                        descBox = this.txtSixDesc;
                        valueBox = this.txtSixHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.7":
                        titleBlock = this.txtSevenTitle;
                        descBox = this.txtSevenDesc;
                        valueBox = this.txtSevenHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.8":
                        titleBlock = this.txtEightTitle;
                        descBox = this.txtEightDesc;
                        valueBox = this.txtEightHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "plan.9":
                        titleBlock = this.txtNineTitle;
                        descBox = this.txtNineDesc;
                        valueBox = this.txtNineHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                }
                titleBlock.Text = title;

                if (DateTime.Now.Minute == 30)
                {
                    descBox.Document.Blocks.Clear();
                }

                if (!string.IsNullOrEmpty(desc))
                {
                    descBox.AppendText(desc);
                    descBox.AppendText(Environment.NewLine);
                }
                if (value != null)
                {
                    valueBox.Clear();
                    valueBox.AppendText(value);
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PlanInvoker.Current.Close();
        }
    }
}
