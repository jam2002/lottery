using Lottery.Core.Algorithm;
using System.Text;
using System.Web.Mvc;

namespace Lottery.Web.Controllers
{
    public class PredictController : Controller
    {
        // GET: Index
        [HttpGet]
        public ActionResult Index()
        {
            StringBuilder sb = new StringBuilder();
            Calculator.ClearCache();
            Calculator calculator = new Calculator("cqssc", "anytwo", 30, "5", t => sb.Append(t));
            calculator.Start();

            ViewBag.Predict = sb.ToString();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1: 五连续算法；2：杀最冷算法</param>
        /// <param name="action">1：验证过去1000期；2：预测下一期</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Load(int type, int act)
        {
            StringBuilder sb = new StringBuilder();
            Calculator.ClearCache();
            Calculator calculator = new Calculator("cqssc", "anytwo", 30, type == 1 ? "5" : "-5", t => sb.Append(t));
            if (act == 1)
            {
                calculator.Validate();
            }
            else
            {
                calculator.Start();
            }

            ViewBag.Predict = sb.ToString();

            return View("Index");
        }
    }
}