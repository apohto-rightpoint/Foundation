using EPiServer.Web.Mvc;
using Foundation.Cms.Personalization;
using Foundation.Find.Cms.Locations.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foundation.Features.Locations.LocationItemPage
{
    public class LocationItemPageController : PageController<Find.Cms.Models.Pages.LocationItemPage>
    {
        private readonly LocationItemPageControllerService _controllerService;
        private readonly ICmsTrackingService _trackingService;

        public LocationItemPageController(LocationItemPageControllerService controllerService, ICmsTrackingService trackingService)
        {
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
            _trackingService = trackingService;
        }

        public async Task<ActionResult> Index(Find.Cms.Models.Pages.LocationItemPage currentPage)
        {
            await _trackingService.PageViewed(HttpContext, currentPage);

            var model = _controllerService.GetViewModel(currentPage);

            var editingHints = ViewData.GetEditHints<LocationViewModel, Find.Cms.Models.Pages.LocationItemPage>();
            editingHints.AddFullRefreshFor(p => p.Image);
            editingHints.AddFullRefreshFor(p => p.Tags);

            return View(model);
        }
    }
}