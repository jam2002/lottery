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
                new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "33" },
                new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "34" },
                new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "22" },
                new InputOptions {  Number =30, LotteryName = "xjssc|after", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = "xjssc|middle", GameName = "groupThree" },
                new InputOptions {  Number =30, LotteryName = "xjssc|front", GameName = "groupThree" }
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

        [HttpGet]
        public ActionResult Validate()
        {
            InputOptions[] options = new InputOptions[]
            {
                 new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "34", RetrieveNumber =10000 }
            };
            ValidationResult r = Validator.Validate(options);
            StringBuilder builer = new StringBuilder();
            builer.Append(r.ToReadString(true));
            ViewBag.Predict = builer.ToString();
            return View("Index");
        }
    }
}