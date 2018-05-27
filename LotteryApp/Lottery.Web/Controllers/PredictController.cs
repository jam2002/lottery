using Lottery.Core.Algorithm;
using System.Collections.Generic;
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
        /// <param name="type">1: 五连续算法；2：杀最冷算法；3：五星二码不定位；4：五星三码不定位；5：五星组选形态</param>
        /// <param name="action">1：验证过去1000期；2：预测下一期</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Load(int type, int act, int? number = null)
        {
            number = number ?? 30;
            Dictionary<int, string> argDic = new Dictionary<int, string>
            {
                { 1,"5"},
                { 2,"-5"},
                { 3,"22"},
                { 4,"33"},
                { 5,null},
            };
            Dictionary<int, string> algorthmDic = new Dictionary<int, string>
            {
                { 1,"anytwo"},
                { 2,"anytwo"},
                { 3,"dynamic"},
                { 4,"dynamic"},
                { 5,"fivestar"},
            };

            StringBuilder sb = new StringBuilder();
            Calculator.ClearCache();
            Calculator calculator = new Calculator("cqssc", algorthmDic[type], number.Value, argDic[type], t => sb.Append(t));
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