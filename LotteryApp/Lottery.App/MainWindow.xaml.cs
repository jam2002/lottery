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
            string[] gameNames = new string[] { "single", "tripple", "tuple" };
            Dynamic23[] dynamics = gameNames.SelectMany(c => new Dynamic23[] {
                new Dynamic23
                {
                    RespectRepeat = false,
                    BetIndex = 0,
                    LastBet = null,
                    Number = c == "single" ? 1 : 2,
                    GameName = c,
                    GameArgs = "after",
                    LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"]),
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", c, "WithoutRespectRepeat"), u, v)
                },
                new Dynamic23
                {
                    RespectRepeat = true,
                    BetIndex = 0,
                    LastBet = null,
                    Number = c == "single" ? 1 : 2,
                    GameName = c,
                    GameArgs = "after",
                    LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"]),
                    Dispatcher = (u, v) => UpdateUI(string.Join(".", c, "RespectRepeat"), u, v)
                }
            }).ToArray();

            Dictionary<string, IPlan> dic = dynamics.OfType<IPlan>().ToDictionary(c => c.GetKey(), c => c);
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
                    case "single.WithoutRespectRepeat":
                        descBox = this.txtFrontDesc;
                        valueBox = this.txtFrontHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tripple.WithoutRespectRepeat":
                        descBox = this.txtMiddleDesc;
                        valueBox = this.txtMiddleHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.WithoutRespectRepeat":
                        descBox = this.txtAfterDesc;
                        valueBox = this.txtAfterHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "single.RespectRepeat":
                        descBox = this.txtOneAwardDesc;
                        valueBox = this.txtOneAwardHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tripple.RespectRepeat":
                        descBox = this.txtFiveDesc;
                        valueBox = this.txtFiveHost.Child as System.Windows.Forms.RichTextBox;
                        break;
                    case "tuple.RespectRepeat":
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
