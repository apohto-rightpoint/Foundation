using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using Foundation.Commerce.Models.Blocks;
using System;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount.OrdersBlock
{
    [Authorize]
    [TemplateDescriptor(Default = true)]
    public class OrdersBlockController : BlockController<OrderHistoryBlock>
    {
        private readonly OrdersBlockControllerService _controllerService;

        public OrdersBlockController(OrdersBlockControllerService controllerService)
        {
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        public override ActionResult Index(OrderHistoryBlock currentBlock)
        {
            var viewModel = _controllerService.GetViewModel(currentBlock);

            return PartialView(viewModel);
        }
    }
}