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
            string[] gameArgs = new string[] { "front", "after" };
            Dynamic23[] composites = gameArgs.SelectMany(c => new Dynamic23[]
            {
                new Dynamic23
                {
                    EnableSinglePattern = false,
                    UseGeneralTrend = true,
                    RespectRepeat = false,
                    BetIndex = 0,
                    LastBet = null,
                    Number = 2,
                    TakeNumber = 50,
                    GameName = "tripple",
                    GameArgs = c,
                    LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"]),
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", "tripple", c), u, v)
                },
                new Dynamic23
                {
                    EnableSinglePattern = false,
                    UseGeneralTrend = true,
                    RespectRepeat = false,
                    BetIndex = 0,
                    LastBet = null,
                    Number = 2,
                    TakeNumber = 50,
                    GameName = "tuple",
                    GameArgs = c,
                    LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"]),
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", "tuple", c), u, v)
                }
            }).ToArray();

            string[] gameNames = new string[] { "front", "after" };
            Dynamic23[] singles = gameNames.Select((c, i) => new Dynamic23
            {
                EnableSinglePattern = i == 1,
                UseGeneralTrend = false,
                RespectRepeat = true,
                BetIndex = 0,
                LastBet = null,
                Number = 2,
                GameName = "tuple",
                GameArgs = c,
                LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"]),
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "tuple", c, "respectRepeat"), u, v)
            }).ToArray();

            Dictionary<string, IPlan> dic = composites.Concat(singles).OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
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
                    case "tripple.front":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.front":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tripple.after":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.after":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.front.respectRepeat":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.after.respectRepeat":
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
