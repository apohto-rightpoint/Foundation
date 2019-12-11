using System;
using EPiServer.SpecializedProperties;
using Foundation.Commerce;
using Foundation.Commerce.Models.Pages;
using Foundation.Commerce.ViewModels.Header;
using Foundation.Demo.Models;
using System.Linq;
using EPiServer;
using EPiServer.Framework.Localization;
using EPiServer.Web.Routing;
using Foundation.Cms;
using Foundation.Commerce.Customer.Services;

namespace Foundation.Features.MyAccount
{
    public class MyAccountNavigationControllerService
    {
        private readonly LocalizationService _localizationService;
        private readonly CookieService _cookieService = new CookieService();
        private readonly IOrganizationService _organizationService;
        private readonly ICustomerService _customerService;
        private readonly UrlResolver _urlResolver;
        private readonly IContentLoader _contentLoader;

        public MyAccountNavigationControllerService(LocalizationService localizationService,
            CookieService cookieService,
            IOrganizationService organizationService,
            ICustomerService customerService,
            UrlResolver urlResolver,
            IContentLoader contentLoader)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _urlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
        }

        public MyAccountNavigationViewModel GetViewModel(MyAccountPageType id, LinkItemCollection menuItems, DemoHomePage startPage)
        {
            var selectedSubNav = _cookieService.Get(Constant.Fields.SelectedNavSuborganization);
            var organization = _organizationService.GetCurrentFoundationOrganization();
            var canSeeOrganizationNav = _customerService.CanSeeOrganizationNav();

            var model = new MyAccountNavigationViewModel
            {
                Organization = canSeeOrganizationNav ? _organizationService.GetOrganizationModel(organization) : null,
                CurrentOrganization = canSeeOrganizationNav ? !string.IsNullOrEmpty(selectedSubNav) ?
                    _organizationService.GetOrganizationModel(_organizationService.GetSubFoundationOrganizationById(selectedSubNav)) :
                    _organizationService.GetOrganizationModel(organization) : null,
                CurrentPageType = id,
                OrganizationPage = startPage.OrganizationMainPage,
                SubOrganizationPage = startPage.SubOrganizationPage,
                MenuItemCollection = new LinkItemCollection()
            };

            if (menuItems != null)
            {
                var wishlist = _contentLoader.Get<WishListPage>(startPage.WishlistPage);
                menuItems = menuItems.CreateWritableClone();

                if (model.Organization != null)
                {
                    if (wishlist != null)
                    {
                        var url = wishlist.LinkURL.Contains("?")
                            ? wishlist.LinkURL.Split('?').First()
                            : wishlist.LinkURL;
                        var item = menuItems.FirstOrDefault(x => x.Href.Substring(1).Equals(url));
                        if (item != null)
                        {
                            menuItems.Remove(item);
                        }
                    }

                    menuItems.Add(new LinkItem
                    {
                        Href = _urlResolver.GetUrl(startPage.QuickOrderPage),
                        Text = _localizationService.GetString("/Dashboard/Labels/QuickOrder", "Quick Order")
                    });
                }
                else if (organization != null)
                {
                    if (wishlist != null)
                    {
                        var url = wishlist.LinkURL.Contains("?")
                            ? wishlist.LinkURL.Split('?').First()
                            : wishlist.LinkURL;
                        var item = menuItems.FirstOrDefault(x => x.Href.Substring(1).Equals(url));
                        if (item != null)
                        {
                            item.Text = _localizationService.GetString("/Dashboard/Labels/OrderPad", "Order Pad");
                        }
                    }
                }

                model.MenuItemCollection.AddRange(menuItems);
            }

            return model;
        }
    }
}