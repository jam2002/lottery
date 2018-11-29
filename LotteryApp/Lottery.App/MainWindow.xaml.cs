using Lottery.Core.Plan;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            int takeNumber = int.Parse(ConfigurationManager.AppSettings["GameNumber"]);
            string lotteryName = ConfigurationManager.AppSettings["LotteryName"];
            string[] gameArgs = new string[] { "front", "after" };

            Dynamic23[] singles = gameArgs.Select((c, i) => new Dynamic23
            {
                EnableSinglePattern = i > 0,
                Number = 1,
                TakeNumber = 50,
                GameName = "single",
                GameArgs = c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c), u, v)
            }).ToArray();

            Dynamic23[] repeats = gameArgs.Select((c, i) => new Dynamic23
            {
                EnableSinglePattern = false,
                Number = 1,
                TakeNumber = takeNumber,
                GameName = "repeats",
                GameArgs = c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "repeats", c), u, v)
            }).ToArray();

            gameArgs = new string[] { "middle", "after" };
            Dynamic23[] spans = gameArgs.Select((c, i) => new Dynamic23
            {
                EnableSinglePattern = true,
                Number = 1,
                TakeNumber = takeNumber,
                GameName = "span",
                GameArgs = c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "span", c), u, v)
            }).ToArray();

            Dictionary<string, IPlan> dic = singles.Concat(repeats).Concat(spans).OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
            PlanInvoker.Current.Init(dic);
        }

        private void UpdateUI(string code, string desc, string value)
        {
            this.Dispatcher.Invoke(() =>
            {
                RichTextBox descBox = null;
                System.Windows.Forms.RichTextBox valueBox = null;
                switch (code)
                {
                    case "single.front":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "repeats.front":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "span.middle":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.after":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "repeats.after":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "span.after":
                        descBox = this.txtFiftyDesc;
                        valueBox = this.txtFiftyHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                }

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
