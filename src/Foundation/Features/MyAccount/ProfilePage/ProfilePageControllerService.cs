using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.Web.Routing;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Customer.ViewModels;
using Foundation.Commerce.Models.Pages;
using Foundation.Commerce.Order.Services;
using Foundation.Commerce.Order.ViewModels;
using Mediachase.Commerce.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foundation.Features.MyAccount.ProfilePage
{
    public class ProfilePageControllerService
    {
        private readonly UrlResolver _urlResolver;
        private readonly IContentLoader _contentLoader;
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;

        public ProfilePageControllerService(UrlResolver urlResolver,
            IContentLoader contentLoader,
            IAddressBookService addressBookService,
            IOrderRepository orderRepository,
            ICartService cartService)
        {
            _urlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _addressBookService = addressBookService ?? throw new ArgumentNullException(nameof(addressBookService));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        public SocialCommerceProfilePageViewModel GetViewModel(Social.Models.Pages.ProfilePage currentPage, string userName, ICustomerService customerService)
        {
            var viewModel = new SocialCommerceProfilePageViewModel(currentPage)
            {
                Orders = GetOrderHistoryViewModels(),
                Addresses = GetAddressViewModels(),
                SiteUser = customerService.GetSiteUser(userName),
                CustomerContact = new Commerce.Customer.FoundationContact(customerService.GetCurrentContact().Contact),
                OrderDetailsPageUrl = _urlResolver.GetUrl(_contentLoader.Get<CommerceHomePage>(ContentReference.StartPage).OrderDetailsPage)
            };

            return viewModel;
        }

        private IList<AddressModel> GetAddressViewModels() => _addressBookService.List();

        private List<OrderViewModel> GetOrderHistoryViewModels()
        {
            var purchaseOrders = _orderRepository.Load<IPurchaseOrder>(PrincipalInfo.CurrentPrincipal.GetContactId(), _cartService.DefaultCartName)
                .OrderByDescending(x => x.Created).ToList();

            if (purchaseOrders.Count > 3)
            {
                purchaseOrders = purchaseOrders.Take(3).ToList();
            }


            var viewModel = new List<OrderViewModel>();

            foreach (var purchaseOrder in purchaseOrders)
            {
                // Assume there is only one form per purchase.
                var form = purchaseOrder.GetFirstForm();
                var billingAddress = new AddressModel();
                var payment = form.Payments.FirstOrDefault();
                if (payment != null)
                {
                    billingAddress = _addressBookService.ConvertToModel(payment.BillingAddress);
                }
                var orderViewModel = new OrderViewModel
                {
                    PurchaseOrder = purchaseOrder,
                    Items = form.GetAllLineItems().Select(lineItem => new OrderHistoryItemViewModel
                    {
                        LineItem = lineItem,
                    }).GroupBy(x => x.LineItem.Code).Select(group => group.First()),
                    BillingAddress = billingAddress,
                    ShippingAddresses = new List<AddressModel>(),
                    OrderGroupId = purchaseOrder.OrderLink.OrderGroupId
                };

                foreach (var orderAddress in purchaseOrder.Forms.SelectMany(x => x.Shipments).Select(s => s.ShippingAddress))
                {
                    var shippingAddress = _addressBookService.ConvertToModel(orderAddress);
                    orderViewModel.ShippingAddresses.Add(shippingAddress);
                }

                viewModel.Add(orderViewModel);
            }

            return viewModel;
        }
    }
}