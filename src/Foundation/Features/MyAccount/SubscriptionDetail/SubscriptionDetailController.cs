using EPiServer.Web.Mvc;
using Foundation.Commerce.Models.Pages;
using System;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount.SubscriptionDetail
{
    public class SubscriptionDetailController : PageController<SubscriptionDetailPage>
    {
        private readonly SubscriptionDetailControllerService _controllerService;

        public SubscriptionDetailController(SubscriptionDetailControllerService controllerService)
        {
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        public ActionResult Index(SubscriptionDetailPage currentPage, int paymentPlanId = 0)
        {
            var viewModel = _controllerService.GetViewModel(currentPage, paymentPlanId);

            return View(viewModel);
        }


    }
}