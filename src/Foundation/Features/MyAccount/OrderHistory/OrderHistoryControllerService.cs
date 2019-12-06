using System;
using System.Collections.Generic;
using System.Linq;
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
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;

namespace Foundation.Features.MyAccount.OrderHistory
{
    public class OrderHistoryControllerService
    {
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderRepository _orderRepository;
        private readonly IContentLoader _contentLoader;
        private readonly ICartService _cartService;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly UrlResolver _urlResolver;

        public OrderHistoryControllerService(IAddressBookService addressBookService,
            IOrderRepository orderRepository,
            IContentLoader contentLoader,
            ICartService cartService,
            IOrderGroupFactory orderGroupFactory,
            UrlResolver urlResolver)
        {
            _addressBookService = addressBookService ?? throw new ArgumentNullException(nameof(addressBookService));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _orderGroupFactory = orderGroupFactory ?? throw new ArgumentNullException(nameof(orderGroupFactory));
            _urlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
        }

        public OrderHistoryViewModel GetViewModel(OrderHistoryPage currentPage, int? page, int? size)
        {
            var pageNum = page ?? 1;
            var pageSize = size ?? 10;
            var orders = _orderRepository.Load<IPurchaseOrder>(PrincipalInfo.CurrentPrincipal.GetContactId(), _cartService.DefaultCartName);
            var purchaseOrders = orders
                                .OrderByDescending(x => x.Created)
                                .Skip((pageNum - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var viewModel = new OrderHistoryViewModel(currentPage)
            {
                CurrentContent = currentPage,
                Orders = new List<OrderViewModel>()
            };

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
                    ShippingAddresses = new List<AddressModel>()
                };

                foreach (var orderAddress in purchaseOrder.Forms.SelectMany(x => x.Shipments).Select(s => s.ShippingAddress))
                {
                    var shippingAddress = _addressBookService.ConvertToModel(orderAddress);
                    orderViewModel.ShippingAddresses.Add(shippingAddress);
                }

                viewModel.Orders.Add(orderViewModel);
            }
            viewModel.OrderDetailsPageUrl =
             UrlResolver.Current.GetUrl(_contentLoader.Get<CommerceHomePage>(ContentReference.StartPage).OrderDetailsPage);

            viewModel.PagingInfo.PageNumber = pageNum;
            viewModel.PagingInfo.TotalRecord = orders.Count();
            viewModel.PagingInfo.PageSize = pageSize;
            viewModel.OrderHistoryUrl = currentPage.StaticLinkURL;

            return viewModel;
        }

        public string GetPaymentPlanUrl(IPurchaseOrder purchaseOrder, int cycleMode, int cycleLength)
        {
            var cart = _orderRepository.Create<ICart>(Guid.NewGuid().ToString());
            cart.CopyFrom(purchaseOrder, _orderGroupFactory);
            var orderReference = _orderRepository.SaveAsPaymentPlan(cart);
            _orderRepository.Delete(cart.OrderLink);
            var paymentPlan = _orderRepository.Load<IPaymentPlan>(orderReference.OrderGroupId);
            paymentPlan.CycleMode = (PaymentPlanCycle)cycleMode;
            paymentPlan.CycleLength = cycleLength;
            paymentPlan.StartDate = DateTime.UtcNow;
            paymentPlan.IsActive = true;

            var principal = PrincipalInfo.CurrentPrincipal;
            AddNoteToOrder(paymentPlan, $"Note: New payment plan placed by {principal.Identity.Name}.", OrderNoteTypes.System, principal.GetContactId());
            paymentPlan.AdjustInventoryOrRemoveLineItems((__, _) => { });
            _orderRepository.Save(paymentPlan);

            //create first order
            orderReference = _orderRepository.SaveAsPurchaseOrder(paymentPlan);
            var newPurchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            OrderGroupWorkflowManager.RunWorkflow((OrderGroup)newPurchaseOrder, OrderGroupWorkflowManager.CartCheckOutWorkflowName);
            var noteDetailPattern = "New purchase order placed by {0} in {1} from payment plan {2}";
            var noteDetail = string.Format(noteDetailPattern, principal.Identity.Name, "VNext site", (paymentPlan as PaymentPlan).Id);
            AddNoteToOrder(newPurchaseOrder, noteDetail, OrderNoteTypes.System, principal.GetContactId());
            _orderRepository.Save(newPurchaseOrder);

            paymentPlan.LastTransactionDate = DateTime.UtcNow;
            paymentPlan.CompletedCyclesCount++;
            _orderRepository.Save(paymentPlan);
            var homePage = _contentLoader.Get<CommerceHomePage>(ContentReference.StartPage);
            var paymentPlanPageUrl = _urlResolver.GetUrl(homePage.PaymentPlanDetailsPage) + $"?paymentPlanId={paymentPlan.OrderLink.OrderGroupId}";

            return paymentPlanPageUrl;
        }

        private void AddNoteToOrder(IOrderGroup order, string noteDetails, OrderNoteTypes type, Guid customerId)
        {
            if (order == null)
            {
                throw new ArgumentNullException("purchaseOrder");
            }
            var orderNote = order.CreateOrderNote();

            if (!orderNote.OrderNoteId.HasValue)
            {
                var newOrderNoteId = -1;

                if (order.Notes.Count != 0)
                {
                    newOrderNoteId = Math.Min(order.Notes.ToList().Min(n => n.OrderNoteId.Value), 0) - 1;
                }

                orderNote.OrderNoteId = newOrderNoteId;
            }

            orderNote.CustomerId = customerId;
            orderNote.Type = type.ToString();
            orderNote.Title = noteDetails.Substring(0, Math.Min(noteDetails.Length, 24)) + "...";
            orderNote.Detail = noteDetails;
            orderNote.Created = DateTime.UtcNow;
        }
    }
}