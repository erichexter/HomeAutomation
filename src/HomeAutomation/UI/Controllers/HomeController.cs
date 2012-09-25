using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            var model = new List<Device>();
            model.Add(new Device { Name = "Office", Adress = "a8" });
            model.Add(new Device { Name="Stairway",Adress="a6"});
            model.Add(new Device { Name = "Living Rm", Adress = "a9" });
            model.Add(new Device { Name = "Mastr Light", Adress = "a10" });
            model.Add(new Device { Name = "Master Fan", Adress = "a11" });

            return View(model);
        }
    }
}
