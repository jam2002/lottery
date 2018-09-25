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
        private PlanInvoker invoker;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] suffixes = new string[] { "front", "middle", "after" };
            Dynamic13[] dynamics = suffixes.Select(c => new Dynamic13
            {
                BetCycle = int.Parse(ConfigurationManager.AppSettings["BetCycle"]),
                BetIndex = 0,
                LastBet = null,
                Number = 1,
                GameName = "dynamic",
                GameArgs = "13",
                LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"], "|", c),
                Dispatcher = (u, v) => UpdateUI(c, u, v)
            }).ToArray();

            Dynamic22 five = new Dynamic22
            {
                BetCycle = int.Parse(ConfigurationManager.AppSettings["BetCycle"]),
                BetIndex = 0,
                LastBet = null,
                GameName = "adjacent",
                GameArgs = "22",
                LotteryName = ConfigurationManager.AppSettings["LotteryName"],
                Number = 2,
                Dispatcher = (u, v) => UpdateUI("five", u, v)
            };

            Dictionary<string, IPlan> dic = dynamics.Concat(new IPlan[] { five }).ToDictionary(c => c.GetKey(), c => c);
            this.invoker = new PlanInvoker(dic);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.invoker.Close();
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
                    case "five":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
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
    }
}
