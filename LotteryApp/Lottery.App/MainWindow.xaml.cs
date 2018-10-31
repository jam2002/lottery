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
            string[] gameArgs = new string[] { "front", "middle", "after" };
            Dynamic23[] dynamics = gameArgs.Select(c => new Dynamic23
            {
                BetCycle = 7,
                BetIndex = 0,
                LastBet = null,
                Number = 2,
                TakeNumber = 10,
                GameName = "tuple",
                GameArgs = c,
                LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"]),
                Dispatcher = (u, v) => UpdateUI(c, u, v)
            }).ToArray();

            gameArgs = new string[] { "all" };
            Dynamic23[] singles = gameArgs.Select(c => new Dynamic23
            {
                BetCycle = 4,
                BetIndex = 0,
                LastBet = null,
                Number = 2,
                GameName = "tuple",
                GameArgs = c,
                LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"]),
                Dispatcher = (u, v) => UpdateUI(c, u, v)
            }).ToArray();

            string[] gameNames = new string[] { "front4", "after4" };
            Dynamic23[] adjacents = gameNames.Select(c => new Dynamic23
            {
                BetCycle = 7,
                BetIndex = 0,
                LastBet = null,
                Number = 2,
                TakeNumber = 10,
                GameName = "tuple",
                GameArgs = c,
                LotteryName = ConfigurationManager.AppSettings["LotteryName"],
                Dispatcher = (u, v) => UpdateUI(c, u, v)
            }).ToArray();

            Dictionary<string, IPlan> dic = dynamics.Concat(singles.OfType<IPlan>()).Concat(adjacents.OfType<IPlan>()).ToDictionary(c => c.GetKey(), c => c);
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
                    case "front":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "middle":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "after":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "all":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "front4":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "after4":
                        descBox = this.txtFiftyDesc;
                        valueBox = this.txtFiftyHost.Child as System.Windows.Forms.RichTextBox;
                        break;
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
