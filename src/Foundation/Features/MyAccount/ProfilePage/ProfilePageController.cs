using EPiServer.Cms.UI.AspNetIdentity;
using Foundation.Cms.Identity;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Customer.ViewModels;
using System;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount.ProfilePage
{
    [Authorize]
    public class ProfilePageController : IdentityControllerBase<Social.Models.Pages.ProfilePage>
    {
        private readonly ProfilePageControllerService _controllerService;

        public ProfilePageController(ProfilePageControllerService controllerService,
            ApplicationSignInManager<SiteUser> signinManager,
            ApplicationUserManager<SiteUser> userManager,
            ICustomerService customerService) : base(signinManager, userManager, customerService)
        {
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        public ActionResult Index(Social.Models.Pages.ProfilePage currentPage)
        {
            var viewModel = _controllerService.GetViewModel(currentPage, User.Identity.Name, CustomerService);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(Social.Models.Pages.ProfilePage currentPage, AccountInformationViewModel viewModel)
        {
            var user = CustomerService.GetSiteUser(User.Identity.Name);
            var contact = CustomerService.GetCurrentContact();
            user.FirstName = contact.FirstName = viewModel.FirstName;
            user.LastName = contact.LastName = viewModel.LastName;
            contact.Contact.BirthDate = viewModel.DateOfBirth;
            user.NewsLetter = viewModel.SubscribesToNewsletter;

            UserManager.UpdateAsync(user)
                .GetAwaiter()
                .GetResult();

            contact.SaveChanges();

            return Json(new { FirstName = contact.FirstName, LastName = contact.LastName });
        }
    }
}