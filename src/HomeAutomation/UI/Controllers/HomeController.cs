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
        SceneRepository _sceneRepository = new SceneRepository();
        public ActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();
            model.Devices = _repository.GetAll();
            model.Scenes = _sceneRepository.GetAll();
            return View(model);
        }
        public ActionResult Test()
        {
            HomeViewModel model = new HomeViewModel();
            model.Devices = _repository.GetAll();
            model.Scenes = _sceneRepository.GetAll();
            return View(model);            
        }
    }

    public class HomeViewModel
    {
        public IList<Device> Devices { get; set; }

        public IList<Scene> Scenes { get; set; }
    }
}
