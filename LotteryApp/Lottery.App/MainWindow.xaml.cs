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
            string lotteryName = ConfigurationManager.AppSettings["LotteryName"];
            string[] gameNames = new string[] { "middle", "after" };
            Dynamic23[] tuples = gameNames.SelectMany((c, i) => new Dynamic23[]
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
                    LotteryName = lotteryName,
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", "tripple", c), u, v)
                },
                new Dynamic23
                {
                    RespectRepeat = false,
                    BetIndex = 0,
                    BetCycle = 5,
                    TakeNumber =30,
                    LastBet = null,
                    Number = 1,
                    GameName = "double",
                    GameArgs = c,
                    LotteryName = lotteryName,
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", "double", c), u, v)
                },
                new Dynamic23
                {
                    RespectRepeat = true,
                    BetIndex = 0,
                    LastBet = null,
                    Number = 2,
                    GameName = "tuple",
                    GameArgs = c,
                    LotteryName = lotteryName,
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", "tuple", c, "respectRepeat"), u, v)
                }
            }).ToArray();

            Dictionary<string, IPlan> dic = tuples.OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
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
                    case "tripple.middle":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "double.middle":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.middle.respectRepeat":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tripple.after":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "double.after":
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
