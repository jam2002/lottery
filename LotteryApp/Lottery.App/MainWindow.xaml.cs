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
            int betCycle = int.Parse(ConfigurationManager.AppSettings["BetCycle"]);
            string lotteryName = ConfigurationManager.AppSettings["LotteryName"];
            string[] gameArgs = new string[] { "front", "middle", "after" };

            Dynamic23[] singles = gameArgs.Select((c, i) => new Dynamic23
            {
                Number = 1,
                TakeNumber = takeNumber,
                BetCycle = betCycle,
                GameName = "single",
                GameArgs = c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c), u, v)
            }).ToArray();

            Dynamic23[] shotSingles = gameArgs.Select((c, i) => new Dynamic23
            {
                Number = 1,
                TakeNumber = takeNumber,
                BetCycle = 4,
                GameName = "single",
                GameArgs = c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c, "4"), u, v)
            }).ToArray();

            Dictionary<string, IPlan> dic = singles.Concat(shotSingles).OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
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
                    case "single.front.4":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.middle":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.middle.4":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.after":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.after.4":
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
