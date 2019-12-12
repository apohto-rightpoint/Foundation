using EPiServer.Web.Mvc;
using Foundation.Commerce.Order.ViewModels;
using System;
using System.Web.Mvc;

namespace Foundation.Features.MyOrganization.Orders
{
    [Authorize]
    public class OrdersController : PageController<Commerce.Models.Pages.OrdersPage>
    {
        private readonly OrdersControllerService _controllerService;

        public OrdersController(OrdersControllerService controllerService)
        {
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        public ActionResult Index(Commerce.Models.Pages.OrdersPage currentPage)
        {
            var viewModel = _controllerService.GetViewModel(currentPage);

            return View(viewModel);
        }

        public ActionResult QuickOrder(Commerce.Models.Pages.OrdersPage currentPage)
        {
            var viewModel = new OrdersPageViewModel { CurrentContent = currentPage };
            return View(viewModel);
        }
    }
}