using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Logging;
using Foundation.Cms;
using Foundation.Commerce;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Customer.ViewModels;
using Foundation.Commerce.Models.Pages;
using Foundation.Commerce.Order.ViewModels;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;

namespace Foundation.Features.MyOrganization.Budgeting
{
    public class BudgetingControllerService
    {
        private readonly IBudgetService _budgetService;
        private readonly IOrganizationService _organizationService;
        private readonly ICurrentMarket _currentMarket;
        private readonly CookieService _cookieService = new CookieService();

        public BudgetingControllerService(IBudgetService budgetService, IOrganizationService organizationService, ICurrentMarket currentMarket)
        {
            _budgetService = budgetService ?? throw new ArgumentNullException(nameof(budgetService));
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _currentMarket = currentMarket ?? throw new ArgumentNullException(nameof(currentMarket));
        }

        public BudgetingPageViewModel GetViewModel(BudgetingPage currentPage)
        {
            var organizationBudgets = new List<BudgetViewModel>();
            var suborganizationsBudgets = new List<BudgetViewModel>();

            var currentOrganization = !string.IsNullOrEmpty(_cookieService.Get(Constant.Fields.SelectedSuborganization))
                ? _organizationService.GetSubFoundationOrganizationById(_cookieService.Get(Constant.Fields.SelectedSuborganization))
                : _organizationService.GetCurrentFoundationOrganization();

            var viewModel = new BudgetingPageViewModel
            {
                CurrentContent = currentPage,
                OrganizationBudgets = organizationBudgets,
                IsSubOrganization = !string.IsNullOrEmpty(_cookieService.Get(Constant.Fields.SelectedSuborganization))
            };

            if (currentOrganization != null)
            {
                var currentBudget = _budgetService.GetCurrentOrganizationBudget(currentOrganization.OrganizationId);
                if (currentBudget != null)
                {
                    viewModel.CurrentBudgetViewModel = new BudgetViewModel(currentBudget);
                }

                var budgets = _budgetService.GetOrganizationBudgets(currentOrganization.OrganizationId);
                if (budgets != null)
                {
                    organizationBudgets.AddRange(budgets.Select(budget => new BudgetViewModel(budget) { OrganizationName = currentOrganization.Name, IsCurrentBudget = (currentBudget?.BudgetId == budget.BudgetId) }));
                    viewModel.OrganizationBudgets = organizationBudgets;
                }

                var suborganizations = currentOrganization.SubOrganizations;
                if (suborganizations != null)
                {
                    foreach (var suborganization in suborganizations)
                    {
                        var suborgBudgets = _budgetService.GetCurrentOrganizationBudget(suborganization.OrganizationId);
                        if (suborgBudgets != null)
                        {
                            suborganizationsBudgets.Add(new BudgetViewModel(suborgBudgets) { OrganizationName = suborganization.Name });
                        }
                        viewModel.SubOrganizationsBudgets = suborganizationsBudgets;
                    }
                }

                var purchasersBudgetsViewModel = new List<BudgetViewModel>();
                var purchasersBudgets = _budgetService.GetOrganizationPurchasersBudgets(currentOrganization.OrganizationId);
                if (purchasersBudgets != null)
                {
                    purchasersBudgetsViewModel.AddRange(purchasersBudgets.Select(purchaserBudget => new BudgetViewModel(purchaserBudget)));
                }
                viewModel.PurchasersSpendingLimits = purchasersBudgetsViewModel;
            }
            viewModel.IsAdmin = CustomerContext.Current.CurrentContact.Properties[Constant.Fields.UserRole].Value.ToString() == Constant.UserRoles.Admin;

            return viewModel;
        }

        public BudgetingPageViewModel GetAddBudgetViewModel(BudgetingPage currentPage)
        {
            var viewModel = new BudgetingPageViewModel { CurrentContent = currentPage };

            try
            {
                if (!string.IsNullOrEmpty(_cookieService.Get(Constant.Fields.SelectedSuborganization)))
                {
                    var suborganization =
                        _organizationService.GetSubOrganizationById(
                            _cookieService.Get(Constant.Fields.SelectedSuborganization));
                    var organizationCurrentBudget =
                        _budgetService.GetCurrentOrganizationBudget(suborganization.ParentOrganization.OrganizationId);
                    viewModel.AvailableCurrencies = new List<string> {organizationCurrentBudget.Currency};
                    viewModel.IsSubOrganization = true;
                    viewModel.NewBudgetOption = new BudgetViewModel(organizationCurrentBudget);
                }
                else
                {
                    var availableCurrencies = _currentMarket.GetCurrentMarket().Currencies as List<Currency>;
                    if (availableCurrencies != null)
                    {
                        var currencies = new List<string>();
                        currencies.AddRange(availableCurrencies.Select(currency => currency.CurrencyCode));
                        viewModel.AvailableCurrencies = currencies;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType()).Error(ex.Message, ex.StackTrace);

                return null;
            }

            return viewModel;
        }

        public BudgetingPageViewModel GetEditBudgetViewModel(BudgetingPage currentPage, int budgetId)
        {
            var currentOrganization = !string.IsNullOrEmpty(_cookieService.Get(Constant.Fields.SelectedSuborganization))
                ? _organizationService.GetSubFoundationOrganizationById(_cookieService.Get(Constant.Fields.SelectedSuborganization))
                : _organizationService.GetCurrentFoundationOrganization();

            var currentBudget = _budgetService.GetCurrentOrganizationBudget(currentOrganization.OrganizationId);
            var viewModel = new BudgetingPageViewModel
            {
                CurrentContent = currentPage,
                NewBudgetOption = new BudgetViewModel(_budgetService.GetBudgetById(budgetId))
            };
            if (currentBudget != null && currentBudget.BudgetId == budgetId)
            {
                viewModel.NewBudgetOption.IsCurrentBudget = true;
            }

            return viewModel;
        }

        public BudgetingPageViewModel GetAddBudgetToUserViewModel(BudgetingPage currentPage)
        {
            var viewModel = new BudgetingPageViewModel { CurrentContent = currentPage };
            var currentOrganization = !string.IsNullOrEmpty(_cookieService.Get(Constant.Fields.SelectedSuborganization))
                ? _organizationService.GetSubFoundationOrganizationById(_cookieService.Get(Constant.Fields.SelectedSuborganization))
                : _organizationService.GetCurrentFoundationOrganization();
            var budget = _budgetService.GetCurrentOrganizationBudget(currentOrganization.OrganizationId);
            if (budget != null)
            {
                viewModel.NewBudgetOption = new BudgetViewModel(budget);
            }

            return viewModel;
        }
    }
}