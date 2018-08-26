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
            ViewBag.Predict = GetResult("xjssc");
            return View();
        }

        [HttpGet]
        public ActionResult Load(string name)
        {
            ViewBag.Predict = GetResult(name);
            return View("Index");
        }

        private string GetResult(string name)
        {
            InputOptions[] options = new InputOptions[]
            {
                new InputOptions {  Number =60, LotteryName = name, GameName = "dynamic",  GameArgs = "22" },
                new InputOptions {  Number =30, LotteryName = name, GameName = "dynamic",  GameArgs = "22" },

                new InputOptions {  Number =60, LotteryName = name, GameName = "dynamic",  GameArgs = "34" },
                new InputOptions {  Number =30, LotteryName = name, GameName = "dynamic",  GameArgs = "34" },

                new InputOptions {  Number =60, LotteryName = name, GameName = "dynamic",  GameArgs = "33" },

                new InputOptions {  Number =30, LotteryName = $"{name}|after", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = $"{name}|middle", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = $"{name}|front", GameName = "groupThree" }
            };
            OutputResult[] outputs = Calculator.GetResults(options);
            StringBuilder builer = new StringBuilder();
            foreach (OutputResult r in outputs)
            {
                builer.AppendLine(r.ToReadString(true));
                builer.Append("<br/>");
            }
            return builer.ToString();
        }

        [HttpGet]
        public ActionResult Validate(string name, int? number = null, int? type = null)
        {
            number = number.HasValue ? number.Value : 60;
            string args = type == 1 ? "22" : "34";
            InputOptions[] options = new InputOptions[]
            {
                 new InputOptions {  Number =number.Value, LotteryName = name, GameName = "dynamic",  GameArgs = args, RetrieveNumber =10000 }
            };
            ValidationResult r = Validator.Validate(options);
            StringBuilder builer = new StringBuilder();
            builer.Append(r.ToReadString(true));
            ViewBag.Predict = builer.ToString();
            return View("Index");
        }
    }
}