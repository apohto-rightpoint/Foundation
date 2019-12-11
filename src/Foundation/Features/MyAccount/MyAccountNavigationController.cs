using EPiServer;
using EPiServer.Core;
using Foundation.Commerce.ViewModels.Header;
using Foundation.Demo.Models;
using System;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount
{
    public class MyAccountNavigationController : Controller
    {
        private readonly IContentLoader _contentLoader;
        private readonly MyAccountNavigationControllerService _controllerService;

        public MyAccountNavigationController(
            IContentLoader contentLoader,
            MyAccountNavigationControllerService controllerService)
        {
            _contentLoader = contentLoader;
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        public ActionResult MyAccountMenu(MyAccountPageType id)
        {
            var startPage = _contentLoader.Get<DemoHomePage>(ContentReference.StartPage);
            if (startPage == null)
            {
                return new EmptyResult();
            }

            var menuItems = startPage.ShowCommerceHeaderComponents ? startPage.MyAccountCommerceMenu : startPage.MyAccountCmsMenu;

            var model = _controllerService.GetViewModel(id, menuItems, startPage);

            if (menuItems == null)
            {
                return PartialView("_ProfileSidebar", model);
            }

            return PartialView("_ProfileSidebar", model);
        }
    }
}