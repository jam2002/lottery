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
            string[] gameArgs = new string[] { "all", "front", "after" };

            Dynamic23[] singles = gameArgs.Select((c, i) => new Dynamic23
            {
                EnableSinglePattern = i == 2,
                BetCycle = i == 0 ? 5 : 10,
                TakeNumber = i == 0 ? 15 : 50,
                RespectRepeat = false,
                BetIndex = 0,
                LastBet = null,
                Number = 1,
                GameName = "single",
                GameArgs = i == 0 ? "all.1" : c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c), u, v)
            }).ToArray();

            gameArgs = new string[] { "front", "after", "after" };
            Dynamic23[] repeats = gameArgs.Select((c, i) => new Dynamic23
            {
                EnableSinglePattern = i == 2,
                RespectRepeat = true,
                BetIndex = 0,
                LastBet = null,
                Number = 1,
                GameName = "repeats",
                GameArgs = c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "repeats", c, i), u, v)
            }).ToArray();

            Dictionary<string, IPlan> dic = singles.Concat(repeats).OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
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
                    case "single.all":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.front":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.after":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "repeats.front.0":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "repeats.after.1":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "repeats.after.2":
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
