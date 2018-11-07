using Quartz;

namespace Lottery.Core.Plan
{
    public class SimpleJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            PlanInvoker.Current.StartBet();
            PlanInvoker.Current.ChangeSchedule();
        }
    }
}
