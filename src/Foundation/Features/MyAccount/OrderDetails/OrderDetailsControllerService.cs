using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Foundation.Commerce;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Customer.ViewModels;
using Foundation.Commerce.Models.Pages;
using Foundation.Commerce.Order.Services;
using Foundation.Commerce.Order.ViewModels;
using Mediachase.Commerce.Orders;

namespace Foundation.Features.MyAccount.OrderDetails
{
    public class OrderDetailsControllerService
    {
        private readonly ICustomerService _customerService;
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrdersService _ordersService;

        public OrderDetailsControllerService(ICustomerService customerService,
            IAddressBookService addressBookService,
            IOrderRepository orderRepository,
            IOrdersService ordersService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _addressBookService = addressBookService ?? throw new ArgumentNullException(nameof(addressBookService));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _ordersService = ordersService ?? throw new ArgumentNullException(nameof(ordersService));
        }

        public OrderDetailsViewModel GetViewModel(int orderGroupId, OrderDetailsPage currentPage)
        {
            var orderViewModel = new OrderDetailsViewModel
            {
                CurrentContent = currentPage,
                CurrentCustomer = _customerService.GetCurrentContactViewModel()
            };

            var purchaseOrder = OrderContext.Current.Get<PurchaseOrder>(orderGroupId);
            if (purchaseOrder == null)
            {
                return orderViewModel;
            }

            // Assume there is only one form per purchase.
            var form = purchaseOrder.GetFirstForm();

            var billingAddress = form.Payments.FirstOrDefault() != null
                ? form.Payments.First().BillingAddress
                : new OrderAddress();

            orderViewModel.PurchaseOrder = purchaseOrder;

            orderViewModel.Items = form.Shipments.SelectMany(shipment => shipment.LineItems.Select(lineitem => new OrderDetailsItemViewModel
            {
                LineItem = lineitem,
                Shipment = shipment,
                PurchaseOrder = orderViewModel.PurchaseOrder as PurchaseOrder,
            }
            ));

            orderViewModel.BillingAddress = _addressBookService.ConvertToModel(billingAddress);
            orderViewModel.ShippingAddresses = new List<AddressModel>();

            foreach (var orderAddress in form.Shipments.Select(s => s.ShippingAddress))
            {
                var shippingAddress = _addressBookService.ConvertToModel(orderAddress);
                orderViewModel.ShippingAddresses.Add(shippingAddress);
                orderViewModel.OrderGroupId = purchaseOrder.OrderGroupId;
            }
            if (purchaseOrder[Constant.Quote.QuoteExpireDate] != null &&
                !string.IsNullOrEmpty(purchaseOrder[Constant.Quote.QuoteExpireDate].ToString()))
            {
                DateTime.TryParse(purchaseOrder[Constant.Quote.QuoteExpireDate].ToString(), out var quoteExpireDate);
                if (DateTime.Compare(DateTime.Now, quoteExpireDate) > 0)
                {
                    orderViewModel.QuoteStatus = Constant.Quote.QuoteExpired;
                    try
                    {
                        // Update order quote status to expired
                        purchaseOrder[Constant.Quote.QuoteStatus] = Constant.Quote.QuoteExpired;
                        _orderRepository.Save(purchaseOrder);
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger(GetType()).Error("Failed to update order status to Quote Expired.", ex.StackTrace);
                    }

                }
            }


            if (!string.IsNullOrEmpty(purchaseOrder["QuoteStatus"]?.ToString()) &&
                (purchaseOrder.Status == OrderStatus.InProgress.ToString() ||
                 purchaseOrder.Status == OrderStatus.OnHold.ToString()))
            {
                orderViewModel.QuoteStatus = purchaseOrder["QuoteStatus"].ToString();
            }

            orderViewModel.BudgetPayment = _ordersService.GetOrderBudgetPayment(purchaseOrder);
            return orderViewModel;
        }
    }
}