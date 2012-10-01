using System.Web.Mvc;

namespace UI.Controllers
{
    public class ProgramController:Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}