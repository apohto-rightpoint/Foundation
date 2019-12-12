using System;
using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Models.Pages;
using Foundation.Commerce.Order.Services;
using Foundation.Commerce.Order.ViewModels;

namespace Foundation.Features.MyOrganization.Orders
{
    public class OrdersControllerService
    {
        private readonly ICustomerService _customerService;
        private readonly IOrdersService _ordersService;
        private readonly IContentLoader _contentLoader;

        public OrdersControllerService(ICustomerService customerService, IOrdersService ordersService, IContentLoader contentLoader)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _ordersService = ordersService ?? throw new ArgumentNullException(nameof(ordersService));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
        }

        public OrdersPageViewModel GetViewModel(OrdersPage currentPage)
        {
            var organizationUsersList = _customerService.GetContactsForOrganization();
            var viewModel = new OrdersPageViewModel
            {
                CurrentContent = currentPage
            };

            var ordersOrganization = new List<OrderOrganizationViewModel>();
            foreach (var user in organizationUsersList)
            {
                ordersOrganization.AddRange(_ordersService.GetUserOrders(user.ContactId));
            }
            viewModel.OrdersOrganization = ordersOrganization;

            viewModel.OrderDetailsPageUrl =
                UrlResolver.Current.GetUrl(_contentLoader.Get<CommerceHomePage>(ContentReference.StartPage).OrderDetailsPage);

            return viewModel;
        }
    }
}