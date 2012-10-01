using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        DeviceRepository _repository = new DeviceRepository();
        public ActionResult Index()
        {

            var model = _repository.GetAll();

            return View(model);
        }
        public ActionResult Program()
        {
            return View();
        }
    }
}
