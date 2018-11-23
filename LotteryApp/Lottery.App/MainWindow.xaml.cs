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
            string[] gameArgs = new string[] { "0", "1", "2" };
            Dynamic23[] singles = gameArgs.Select((c, i) => new Dynamic23
            {
                UseGeneralTrend = true,
                EnableSinglePattern = false,
                RespectRepeat = false,
                BetIndex = 0,
                LastBet = null,
                Number = 1,
                GameName = "single",
                GameArgs = string.Concat("all.", c),
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "single", c), u, v)
            }).ToArray();

            string[] gameNames = new string[] { "front", "middle", "after" };
            Dynamic23[] tuples = gameNames.Select((c, i) => new Dynamic23
            {
                EnableSinglePattern = i > 0,
                BetIndex = 0,
                LastBet = null,
                Number = 2,
                GameName = "tuple",
                GameArgs = c,
                LotteryName = lotteryName,
                Dispatcher = (u, v) => UpdateUI(string.Join(".", "tuple", c), u, v)
            }).ToArray();

            Dictionary<string, IPlan> dic = singles.Concat(tuples).OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
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
                    case "single.0":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.1":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.2":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.front":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.middle":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.after":
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
