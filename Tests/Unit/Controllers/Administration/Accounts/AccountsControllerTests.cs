using MvcTemplate.Controllers.Administration;
using MvcTemplate.Objects;
using MvcTemplate.Services;
using MvcTemplate.Validators;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcTemplate.Tests.Unit.Controllers.Administration
{
    [TestFixture]
    public class AccountsControllerTests
    {
        private AccountsController controller;
        private AccountEditView accountEdit;
        private IAccountValidator validator;
        private IAccountService service;
        private AccountView account;

        [SetUp]
        public void SetUp()
        {
            validator = Substitute.For<IAccountValidator>();
            service = Substitute.For<IAccountService>();
            accountEdit = new AccountEditView();
            account = new AccountView();

            controller = Substitute.ForPartsOf<AccountsController>(service, validator);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.RouteData = new RouteData();
        }

        #region Method: Index()

        [Test]
        public void Index_ReturnsModelsView()
        {
            service.GetViews().Returns(new[] { account }.AsQueryable());

            Object actual = controller.Index().Model;
            Object expected = service.GetViews();

            Assert.AreSame(expected, actual);
        }

        #endregion

        #region Method: Details(String id)

        [Test]
        public void Details_OnNotFoundModelRedirectsToNotFound()
        {
            controller.When(sub => sub.RedirectToNotFound()).DoNotCallBase();
            service.GetView<AccountView>(String.Empty).Returns((AccountView)null);
            controller.RedirectToNotFound().Returns(new RedirectToRouteResult(new RouteValueDictionary()));

            Object expected = controller.RedirectToNotFound();
            Object actual = controller.Details(String.Empty);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Details_ReturnsAccountView()
        {
            service.GetView<AccountView>(account.Id).Returns(account);

            Object actual = (controller.Details(account.Id) as ViewResult).Model;
            Object expected = account;

            Assert.AreSame(expected, actual);
        }

        #endregion

        #region Method: Edit(String id)

        [Test]
        public void Edit_OnNotFoundModelRedirectsToNotFound()
        {
            controller.When(sub => sub.RedirectToNotFound()).DoNotCallBase();
            service.GetView<AccountView>(String.Empty).Returns((AccountView)null);
            controller.RedirectToNotFound().Returns(new RedirectToRouteResult(new RouteValueDictionary()));

            Object expected = controller.RedirectToNotFound();
            Object actual = controller.Edit(String.Empty);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Edit_ReturnsAccountView()
        {
            service.GetView<AccountEditView>(account.Id).Returns(accountEdit);

            Object actual = (controller.Edit(account.Id) as ViewResult).Model;
            Object expected = accountEdit;

            Assert.AreSame(expected, actual);
        }

        #endregion

        #region Method: Edit(AccountEditView account)

        [Test]
        public void Edit_ReturnsSameModelIfCanNotEdit()
        {
            validator.CanEdit(accountEdit).Returns(false);

            Object actual = (controller.Edit(accountEdit) as ViewResult).Model;
            Object expected = accountEdit;

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void Edit_EditsAccountView()
        {
            validator.CanEdit(accountEdit).Returns(true);

            controller.Edit(accountEdit);

            service.Received().Edit(accountEdit);
        }

        [Test]
        public void Edit_AfterSuccessfulEditRedirectsToIndexIfAuthorized()
        {
            controller.RedirectIfAuthorized("Index").Returns(new RedirectToRouteResult(new RouteValueDictionary()));
            validator.CanEdit(accountEdit).Returns(true);

            ActionResult expected = controller.RedirectIfAuthorized("Index");
            ActionResult actual = controller.Edit(accountEdit);

            Assert.AreSame(expected, actual);
        }

        #endregion
    }
}
