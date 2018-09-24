using Lottery.Core.Plan;
using System;
using System.Windows;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;

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
                GameArgs = "13",
                LotteryName = string.Concat(ConfigurationManager.AppSettings["LotteryName"], "|", c),
                Dispatcher = null
            }).ToArray();

            Dynamic22 five = new Dynamic22
            {
                BetCycle = int.Parse(ConfigurationManager.AppSettings["BetCycle"]),
                BetIndex = 0,
                LastBet = null,
                GameArgs = "22",
                LotteryName = ConfigurationManager.AppSettings["LotteryName"],
                Number = 2,
                Dispatcher = null
            };

            Dictionary<string, IPlan> dic = dynamics.Concat(new IPlan[] { five }).ToDictionary(c => c.GetKey(), c => c);
            this.invoker = new PlanInvoker(dic);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.invoker.Close();
        }
    }
}
