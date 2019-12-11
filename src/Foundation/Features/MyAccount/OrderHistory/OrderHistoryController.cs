using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Models.Pages;
using Foundation.Commerce.Order.Services;
using Foundation.Features.MyAccount.OrderConfirmation;
using System;
using System.Net;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount.OrderHistory
{
    [Authorize]
    public class OrderHistoryController : OrderConfirmationControllerBase<OrderHistoryPage>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IContentLoader _contentLoader;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly OrderHistoryControllerService _controllerService;

        public OrderHistoryController(IAddressBookService addressBookService,
            IOrderRepository orderRepository,
            ConfirmationService confirmationService,
            ICartService cartService,
            IOrderGroupCalculator orderGroupCalculator,
            IContentLoader contentLoader,
            UrlResolver urlResolver,
            IOrderGroupFactory orderGroupFactory,
            ICustomerService customerService,
            OrderHistoryControllerService controllerService) :
            base(confirmationService, addressBookService, orderGroupCalculator, urlResolver, customerService)
        {
            _orderRepository = orderRepository;
            _contentLoader = contentLoader;
            _orderGroupFactory = orderGroupFactory;
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        [HttpGet]
        public ActionResult Index(OrderHistoryPage currentPage, int? page, int? size)
        {
            var model = _controllerService.GetViewModel(currentPage, page, size);

            return View(model);
        }

        public ActionResult ViewAll()
        {
            return Redirect(UrlResolver.Current.GetUrl(_contentLoader.Get<CommerceHomePage>(ContentReference.StartPage).OrderHistoryPage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAsPaymentPlan(int orderid, int cycleMode, int cycleLength)
        {
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderid);
            if (purchaseOrder == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var paymentPlanPageUrl = _controllerService.GetPaymentPlanUrl(purchaseOrder, cycleMode, cycleLength);
            
            return Redirect(paymentPlanPageUrl);
        }
    }
}