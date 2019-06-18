using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicForms.ActionResults;
using DynamicForms.Razor;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DynamicForms.Web.Controllers
{
    [Route("report")]
    public class ReportController : Controller
    {
        private readonly IViewRenderService _viewRenderService;

        public ReportController(IViewRenderService viewRenderService)
        {
            _viewRenderService = viewRenderService;
        }

        [Route("about")]
        // GET: /<controller>/
        public IActionResult Index()
        {
            //var about = _viewRenderService.View("Home/About");
            //var contact = _viewRenderService.View("Home/Contact");
            return new HtmlResult(_viewRenderService.DisplayForModel(new payload(), new { EditMode=true, CreateMode=false }));
        }


        public class payload
        {
            public string Name { get; set; }
        }
    }
}
