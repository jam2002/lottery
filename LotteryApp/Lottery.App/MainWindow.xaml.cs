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
            string[] gameArgs = new string[] { "1", "2" };
            //Dynamic23[] singles = gameArgs.Select((c, i) => new Dynamic23
            //{
            //    EnableSinglePattern = false,
            //    BetIndex = 0,
            //    LastBet = null,
            //    BetCycle = 5,
            //    Number = 1,
            //    TakeNumber = takeNumber,
            //    GameName = "single",
            //    GameArgs = string.Join("." , "all", c),
            //    LotteryName = lotteryName,
            //    Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c), u, v)
            //}).ToArray();

            int[] cycles = new int[] { 15, 50, 100 };
            Dynamic23[] longSingles = cycles.SelectMany((c, i) => new Dynamic23[]
            {
                new Dynamic23
                {
                    BetIndex = 0,
                    LastBet = null,
                    Number = 1,
                    TakeNumber = c,
                    GameName = "single",
                    GameArgs =  "after",
                    LotteryName = lotteryName,
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c), u, v)
                },
                new Dynamic23
                {
                    BetIndex = 0,
                    LastBet = null,
                    Number = 1,
                    WaitInterval =3,
                    TakeNumber = c,
                    GameName = "single",
                    GameArgs = "after",
                    LotteryName = lotteryName,
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c, 3), u, v)
                }
            }).ToArray();

            Dictionary<string, IPlan> dic = longSingles.OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
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
                    case "single.15":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.50":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.100":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.15.3":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.50.3":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.100.3":
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
