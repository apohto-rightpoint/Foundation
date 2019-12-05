using EPiServer.Commerce.Order;
using EPiServer.Web.Mvc;
using Foundation.Commerce.Models.Pages;
using Foundation.Commerce.Order.Services;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount.OrderDetails
{
    public class OrderDetailsController : PageController<OrderDetailsPage>
    {
        private readonly IOrdersService _ordersService;
        private readonly IOrderRepository _orderRepository;
        private readonly OrderDetailsControllerService _controllerService;

        public OrderDetailsController(IOrdersService ordersService,
            IOrderRepository orderRepository,
            OrderDetailsControllerService controllerService)
        {
            _ordersService = ordersService;
            _orderRepository = orderRepository;
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        [HttpGet]
        public ActionResult Index(OrderDetailsPage currentPage, int orderGroupId = 0) => View(_controllerService.GetViewModel(orderGroupId, currentPage));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveOrder(int orderGroupId = 0)
        {
            if (orderGroupId == 0)
            {
                return Json(new { result = true });
            }

            var success = _ordersService.ApproveOrder(orderGroupId);

            return success ? Json(new { Status = true, Message = "" }) : Json(new { Status = false, Message = "Failed to process your payment." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReturn(int orderGroupId, int shipmentId, int lineItemId, decimal returnQuantity, string reason)
        {
            ReturnFormStatus formStatus = _ordersService.CreateReturn(orderGroupId, shipmentId, lineItemId, returnQuantity, reason);
            return Json(new
            {
                Result = true,
                ReturnFormStatus = formStatus.ToString()
            });
        }

        [HttpPost]
        public ActionResult ChangePrice(int orderGroupId, int shipmentId, int lineItemId, decimal placedPrice, OrderDetailsPage currentPage)
        {
            var issues = _ordersService.ChangeLineItemPrice(orderGroupId, shipmentId, lineItemId, placedPrice);
            var model = _controllerService.GetViewModel(orderGroupId, currentPage);
            model.ErrorMessage = GetValidationMessages(issues);
            return PartialView("Index", model);
        }

        [HttpPost]
        public ActionResult ChangeQuantity(int orderGroupId, int shipmentId, int lineItemId, decimal quantity, OrderDetailsPage currentPage)
        {
            var issues = _ordersService.ChangeLineItemQuantity(orderGroupId, shipmentId, lineItemId, quantity);
            var model = _controllerService.GetViewModel(orderGroupId, currentPage);
            model.ErrorMessage = GetValidationMessages(issues);
            return PartialView("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNote(int orderGroupId, string note)
        {
            var order = _orderRepository.Load<IPurchaseOrder>(orderGroupId);
            var orderNote = _ordersService.AddNote(order, "Customer Manual Note", note);
            _orderRepository.Save(order);
            return Json(orderNote);
        }

        private static string GetValidationMessages(Dictionary<ILineItem, List<ValidationIssue>> validationIssues)
        {
            var messages = new List<string>();
            foreach (var validationIssue in validationIssues)
            {
                var warning = new StringBuilder();
                warning.Append($"Line Item with code {validationIssue.Key.Code} ");
                validationIssue.Value.Aggregate(warning, (current, issue) => current.Append(issue).Append(", "));
                messages.Add(warning.ToString().TrimEnd(',', ' '));
            }

            return string.Join(".", messages);
        }
    }
}