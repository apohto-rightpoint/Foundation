using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core.Html;
using EPiServer.DataAbstraction;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Foundation.Cms;
using Foundation.Cms.Categories;
using Foundation.Cms.Pages;
using Foundation.Cms.Personalization;
using Foundation.Cms.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foundation.Features.Blog.BlogItem
{
    public class BlogItemController : PageController<BlogItemPage>
    {
        private readonly ICmsTrackingService _trackingService;
        private readonly BlogItemControllerService _controllerService;

        public int PreviewTextLength { get; set; }

        public BlogItemController(ICmsTrackingService trackingService, BlogItemControllerService controllerService)
        {
            _trackingService = trackingService;
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        public async Task<ActionResult> Index(BlogItemPage currentPage)
        {
            await _trackingService.PageViewed(HttpContext, currentPage);
            PreviewTextLength = 200;

            var model = _controllerService.GetViewModel(currentPage, PreviewTextLength);

            var editHints = ViewData.GetEditHints<ContentViewModel<BlogItemPage>, BlogItemPage>();
            editHints.AddConnection(m => m.CurrentContent.Category, p => p.Category);
            editHints.AddConnection(m => m.CurrentContent.StartPublish, p => p.StartPublish);
            
            return View(model);
        }
    }
}