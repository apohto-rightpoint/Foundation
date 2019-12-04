using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core;
using EPiServer.Core.Html;
using EPiServer.Filters;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Foundation.Cms;
using Foundation.Cms.Categories;
using Foundation.Cms.Extensions;
using Foundation.Cms.Pages;
using Foundation.Cms.Personalization;
using Foundation.Cms.ViewModels;
using Geta.EpiCategories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foundation.Features.Blog.BlogListPage
{
    public class BlogListPageController : PageController<Cms.Pages.BlogListPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICmsTrackingService _trackingService;
        private readonly BlogListPageControllerService _controllerService;

        public BlogListPageController(IContentLoader contentLoader,
            ICmsTrackingService trackingService,
            IPageRouteHelper pageRouteHelper,
            BlogListPageControllerService controllerService)
        {
            _contentLoader = contentLoader;
            _trackingService = trackingService;
            _controllerService = controllerService;
        }

        public async Task<ActionResult> Index(Cms.Pages.BlogListPage currentPage)
        {
            await _trackingService.PageViewed(HttpContext, currentPage);
            var model = new BlogListPageViewModel(currentPage);
            model.SubNavigation = _controllerService.GetSubNavigation(currentPage);

            var pageId = currentPage.ContentLink.ID;
            var pagingInfo = new PagingInfo
            {
                PageId = pageId
            };

            var categoryQuery = Request.QueryString["category"] ?? string.Empty;
            var viewModel = _controllerService.GetViewModel(currentPage, pagingInfo, categoryQuery);
            model.Blogs = viewModel.Blogs;
            model.PagingInfo = pagingInfo;
            return View(model);
        }

        #region BlogListBlock

        public ActionResult GetItemList(PagingInfo pagingInfo)
        {
            var currentPage = _contentLoader.Get<PageData>(new PageReference(pagingInfo.PageId)) as Cms.Pages.BlogListPage;

            if (currentPage == null)
            {
                return new EmptyResult();
            }
            var categoryQuery = Request.QueryString["category"] ?? string.Empty;
            var model = _controllerService.GetViewModel(currentPage, pagingInfo, categoryQuery);

            return PartialView("~/Features/Blog/BlogListPage/_BlogList.cshtml", model);
        }

        public ActionResult Preview(PageData currentPage, BlogListPageViewModel blogModel)
        {
            var pd = (BlogItemPage)currentPage;
            var previewTextLength = 200;

            var model = new BlogItemPageModel(pd)
            {
                Tags = _controllerService.GetTags(pd),
                PreviewText = _controllerService.GetPreviewText(pd, previewTextLength),
                ShowIntroduction = blogModel.ShowIntroduction,
                ShowPublishDate = blogModel.ShowPublishDate,
                Template = blogModel.CurrentContent.Template,
                PreviewOption = blogModel.CurrentContent.PreviewOption,
                StartPublish = currentPage.StartPublish ?? DateTime.UtcNow
            };

            return PartialView("Preview", model);
        }

        #endregion
    }
}
