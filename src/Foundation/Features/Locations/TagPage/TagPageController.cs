using EPiServer;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Foundation.Cms.Media;
using Foundation.Cms.Personalization;
using Foundation.Find.Cms.Locations.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foundation.Features.Locations.TagPage
{
    public class TagPageController : PageController<Cms.Pages.TagPage>
    {
        private readonly TagPageControllerService _controllerService;
        private readonly ICmsTrackingService _trackingService;

        public TagPageController(TagPageControllerService controllerService,
            ICmsTrackingService trackingService)
        {
            _controllerService = controllerService;
            _trackingService = trackingService;
        }

        public async Task<ActionResult> Index(Cms.Pages.TagPage currentPage)
        {
            await _trackingService.PageViewed(HttpContext, currentPage);

            var addcat = ControllerContext.RequestContext.GetCustomRouteData<string>("Category");
            var continent = ControllerContext.RequestContext.GetCustomRouteData<string>("Continent");

            var model = _controllerService.GetViewModel(currentPage, continent, addcat);

            return View(model);
        }
    }
}