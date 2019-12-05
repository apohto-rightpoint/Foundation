using EPiServer.Web.Mvc;
using Foundation.Cms.Personalization;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foundation.Features.Locations.LocationListPage
{
    public class LocationListPageController : PageController<Find.Cms.Models.Pages.LocationListPage>
    {
        private readonly LocationListPageControllerService _controllerService;
        private readonly ICmsTrackingService _trackingService;

        public LocationListPageController(LocationListPageControllerService controllerService,
            ICmsTrackingService trackingService)
        {
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
            _trackingService = trackingService;
        }

        public async Task<ActionResult> Index(Find.Cms.Models.Pages.LocationListPage currentPage)
        {
            await _trackingService.PageViewed(HttpContext, currentPage);

            var model = _controllerService.GetViewModel(currentPage, Request.QueryString);

            return View(model);
        }
    }
}