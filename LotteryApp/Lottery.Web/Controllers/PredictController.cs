using Lottery.Core;
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
            InputOptions[] options = new InputOptions[]
            {
                new InputOptions {  Number =20, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "34" },
                new InputOptions {  Number =20, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "34" },

                new InputOptions {  Number =20, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "22" },
                new InputOptions {  Number =20, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "22" },

                new InputOptions {  Number =30, LotteryName = "xjssc|after", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = "xjssc|middle", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = "xjssc|front", GameName = "groupThree" },

                new InputOptions {  Number =30, LotteryName = "cqssc|after", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = "cqssc|middle", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = "cqssc|front", GameName = "groupThree" }
            };
            OutputResult[] outputs = Calculator.GetResults(options);
            StringBuilder builer = new StringBuilder();
            foreach (OutputResult r in outputs)
            {
                builer.AppendLine(r.ToReadString(true));
                builer.Append("<br/>");
            }
            ViewBag.Predict = builer.ToString();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1: 五连续算法；2：杀最冷算法；3：五星二码不定位；4：五星三码不定位；5：五星组选形态</param>
        /// <param name="action">1：验证过去1000期；2：预测下一期</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Load(int type, int act, int number = 30, string name = "cqssc")
        {
            return View("Index");
        }
    }
}