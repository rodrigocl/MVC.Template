﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using MvcTemplate.Components.Extensions.Html;
using MvcTemplate.Components.Security;
using MvcTemplate.Data.Core;
using MvcTemplate.Objects;
using MvcTemplate.Resources;
using MvcTemplate.Services;
using MvcTemplate.Tests.Data;
using MvcTemplate.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcTemplate.Tests.Unit.Services
{
    [TestFixture]
    public class RoleServiceTests
    {
        private TestingContext context;
        private RoleService service;
        private Role role;

        [SetUp]
        public void SetUp()
        {
            context = new TestingContext();
            service = new RoleService(new UnitOfWork(context));
            Authorization.Provider = Substitute.For<IAuthorizationProvider>();

            TearDownData();
            SetUpData();
        }

        [TearDown]
        public void TearDown()
        {
            Authorization.Provider = null;
            context.Dispose();
            service.Dispose();
        }

        #region Method: SeedPrivilegesTree(RoleView view)

        [Test]
        public void SeedPrivilegesTree_SeedsSelectedIds()
        {
            IEnumerable<String> expected = GetExpectedTree().SelectedIds;
            IEnumerable<String> actual = service.GetView(role.Id).PrivilegesTree.SelectedIds;

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void SeedPrivilegesTree_SeedsFirstLevelNodes()
        {
            RoleView roleView = service.GetView(role.Id);

            IEnumerator<JsTreeNode> expected = GetExpectedTree().Nodes.GetEnumerator();
            IEnumerator<JsTreeNode> actual = roleView.PrivilegesTree.Nodes.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.AreEqual(expected.Current.Id, actual.Current.Id);
                Assert.AreEqual(expected.Current.Name, actual.Current.Name);
                Assert.AreEqual(expected.Current.Nodes.Count, actual.Current.Nodes.Count);
            }
        }

        [Test]
        public void SeedPrivilegesTree_SeedsSecondLevelNodes()
        {
            RoleView roleView = service.GetView(role.Id);

            IEnumerator<JsTreeNode> expected = GetExpectedTree().Nodes.SelectMany(node => node.Nodes).GetEnumerator();
            IEnumerator<JsTreeNode> actual = roleView.PrivilegesTree.Nodes.SelectMany(node => node.Nodes).GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.AreEqual(expected.Current.Id, actual.Current.Id);
                Assert.AreEqual(expected.Current.Name, actual.Current.Name);
                Assert.AreEqual(expected.Current.Nodes.Count, actual.Current.Nodes.Count);
            }
        }

        [Test]
        public void SeedPrivilegesTree_SeedsThirdLevelNodes()
        {
            RoleView roleView = service.GetView(role.Id);

            IEnumerator<JsTreeNode> expected = GetExpectedTree().Nodes.SelectMany(node => node.Nodes).SelectMany(node => node.Nodes).GetEnumerator();
            IEnumerator<JsTreeNode> actual = roleView.PrivilegesTree.Nodes.SelectMany(node => node.Nodes).SelectMany(node => node.Nodes).GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.AreEqual(expected.Current.Id, actual.Current.Id);
                Assert.AreEqual(expected.Current.Name, actual.Current.Name);
                Assert.AreEqual(expected.Current.Nodes.Count, actual.Current.Nodes.Count);
            }
        }

        [Test]
        public void SeedPrivilegesTree_SeedsBranchesWithoutId()
        {
            JsTreeNode rootNode = service.GetView(role.Id).PrivilegesTree.Nodes.Single();
            IEnumerable<JsTreeNode> branches = GetAllBranchNodes(rootNode);

            Assert.IsFalse(branches.Any(branch => branch.Id != null));
        }

        [Test]
        public void SeedPrivilegesTree_SeedsLeafsWithId()
        {
            JsTreeNode rootNode = service.GetView(role.Id).PrivilegesTree.Nodes.Single();
            IEnumerable<JsTreeNode> leafs = GetAllLeafNodes(rootNode);

            Assert.IsFalse(leafs.Any(leaf => leaf.Id == null));
        }

        #endregion

        #region Method: GetViews()

        [Test]
        public void GetViews_GetsAccountViews()
        {
            IEnumerable<RoleView> actual = service.GetViews();
            IEnumerable<RoleView> expected = context
                .Set<Role>()
                .Project()
                .To<RoleView>()
                .OrderByDescending(account => account.CreationDate);

            TestHelper.EnumPropertyWiseEqual(expected, actual);
        }

        #endregion

        #region Method: GetView(String id)

        [Test]
        public void GetView_GetsViewById()
        {
            Role model = context.Set<Role>().SingleOrDefault();

            RoleView expected = Mapper.Map<RoleView>(model);
            RoleView actual = service.GetView(role.Id);

            TestHelper.PropertyWiseEqual(expected, actual);
        }

        [Test]
        public void GetView_SeedsPrivilegesTree()
        {
            service = Substitute.For<RoleService>(new UnitOfWork(context));

            RoleView view = service.GetView(role.Id);

            service.Received().SeedPrivilegesTree(view);
        }

        #endregion

        #region Method: Create(RoleView view)

        [Test]
        public void Create_CreatesRole()
        {
            RoleView expected = ObjectFactory.CreateRoleView(2);
            service.Create(expected);

            Role actual = context.Set<Role>().SingleOrDefault(r => r.Id == expected.Id);

            Assert.AreEqual(expected.CreationDate, actual.CreationDate);
            Assert.AreEqual(expected.Name, actual.Name);
        }

        [Test]
        public void Create_CreatesRolePrivileges()
        {
            RoleView roleView = ObjectFactory.CreateRoleView(2);
            roleView.PrivilegesTree.SelectedIds = GetExpectedTree().SelectedIds;
            service.Create(roleView);

            IEnumerable<String> expected = roleView.PrivilegesTree.SelectedIds;
            IEnumerable<String> actual = context
                .Set<Role>()
                .SingleOrDefault(model => model.Id == roleView.Id)
                .RolePrivileges
                .Select(r => r.PrivilegeId);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        #endregion

        #region Method: Edit(RoleView view)

        [Test]
        public void Edit_EditsRole()
        {
            RoleView expected = service.GetView(role.Id);
            expected.Name += "EditedName";
            service.Edit(expected);

            Role actual = context.Set<Role>().SingleOrDefault();

            Assert.AreEqual(expected.CreationDate, actual.CreationDate);
            Assert.AreEqual(expected.Name, actual.Name);
        }

        [Test]
        public void Edit_EditsRolePrivileges()
        {
            RoleView roleView = service.GetView(role.Id);
            roleView.PrivilegesTree.SelectedIds = roleView.PrivilegesTree.SelectedIds.Take(1).ToList();
            service.Edit(roleView);

            IEnumerable<String> expected = roleView.PrivilegesTree.SelectedIds;
            IEnumerable<String> actual = context.Set<Role>().SingleOrDefault().RolePrivileges.Select(rolePriv => rolePriv.PrivilegeId).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void Edit_RefreshesAuthorizationProvider()
        {
            service.Edit(service.GetView(role.Id));

            Authorization.Provider.Received().Refresh();
        }

        #endregion

        #region Method: Delete(String id)

        [Test]
        public void Delete_NullifiesDeletedRoleInAccounts()
        {
            Assert.IsTrue(context.Set<Account>().Any(account => account.RoleId == role.Id));

            service.Delete(role.Id);

            Assert.IsFalse(context.Set<Account>().Any(account => account.RoleId == role.Id));
        }

        [Test]
        public void Delete_DeletesRole()
        {
            Assert.IsTrue(context.Set<Role>().Any(model => model.Id == role.Id));

            service.Delete(role.Id);

            Assert.IsFalse(context.Set<Role>().Any());
        }

        [Test]
        public void Delete_RefreshesAuthorizationProvider()
        {
            service.Delete(role.Id);

            Authorization.Provider.Received().Refresh();
        }

        #endregion

        #region Test helpers

        private void SetUpData()
        {
            Account account = ObjectFactory.CreateAccount();
            role = ObjectFactory.CreateRole();
            account.RoleId = role.Id;

            context.Set<Account>().Add(account);

            role.RolePrivileges = new List<RolePrivilege>();

            Int32 privNumber = 1;
            IEnumerable<String> controllers = new[] { "Accounts", "Roles", "Users" };
            IEnumerable<String> actions = new[] { "Index", "Create", "Details", "Edit", "Delete" };

            foreach (String controller in controllers)
                foreach (String action in actions)
                {
                    RolePrivilege rolePrivilege = ObjectFactory.CreateRolePrivilege(privNumber++);
                    rolePrivilege.Privilege = new Privilege() { Controller = controller, Action = action };
                    rolePrivilege.Privilege.Area = controller != "Users" ? "Administration" : null;
                    rolePrivilege.Privilege.Id = rolePrivilege.Id;
                    rolePrivilege.PrivilegeId = rolePrivilege.Id;
                    rolePrivilege.RoleId = role.Id;
                    rolePrivilege.Role = role;

                    role.RolePrivileges.Add(rolePrivilege);
                }

            context.Set<Role>().Add(role);
            context.SaveChanges();
        }
        private void TearDownData()
        {
            context.Set<Privilege>().RemoveRange(context.Set<Privilege>());
            context.Set<Account>().RemoveRange(context.Set<Account>());
            context.Set<Role>().RemoveRange(context.Set<Role>());
            context.SaveChanges();
        }

        private JsTree GetExpectedTree()
        {
            JsTree expectedTree = new JsTree();
            JsTreeNode rootNode = new JsTreeNode();
            expectedTree.Nodes.Add(rootNode);
            rootNode.Name = MvcTemplate.Resources.Privilege.Titles.All;
            expectedTree.SelectedIds = role.RolePrivileges.Select(rolePrivilege => rolePrivilege.PrivilegeId).ToArray();

            IEnumerable<Privilege> allPrivileges = context
                .Set<Privilege>()
                .ToList()
                .Select(privilege => new Privilege
                    {
                        Id = privilege.Id,
                        Area = ResourceProvider.GetPrivilegeAreaTitle(privilege.Area),
                        Action = ResourceProvider.GetPrivilegeActionTitle(privilege.Action),
                        Controller = ResourceProvider.GetPrivilegeControllerTitle(privilege.Controller)
                    });

            foreach (IGrouping<String, Privilege> areaPrivilege in allPrivileges.GroupBy(privilege => privilege.Area).OrderBy(privilege => privilege.Key ?? privilege.FirstOrDefault().Controller))
            {
                JsTreeNode areaNode = new JsTreeNode(areaPrivilege.Key);
                foreach (IGrouping<String, Privilege> controllerPrivilege in areaPrivilege.GroupBy(privilege => privilege.Controller).OrderBy(privilege => privilege.Key))
                {
                    JsTreeNode controllerNode = new JsTreeNode(controllerPrivilege.Key);
                    foreach (IGrouping<String, Privilege> actionPrivilege in controllerPrivilege.GroupBy(privilege => privilege.Action).OrderBy(privilege => privilege.Key))
                        controllerNode.Nodes.Add(new JsTreeNode(actionPrivilege.First().Id, actionPrivilege.Key));

                    if (areaNode.Name == null)
                        rootNode.Nodes.Add(controllerNode);
                    else
                        areaNode.Nodes.Add(controllerNode);
                }

                if (areaNode.Name != null)
                    rootNode.Nodes.Add(areaNode);
            }

            return expectedTree;
        }
        private IEnumerable<JsTreeNode> GetAllBranchNodes(JsTreeNode root)
        {
            List<JsTreeNode> branches = root.Nodes.Where(node => node.Nodes.Count > 0).ToList();
            foreach (JsTreeNode branch in branches.ToList())
                branches.AddRange(GetAllBranchNodes(branch));

            if (root.Nodes.Count > 0)
                branches.Add(root);

            return branches;
        }
        private IEnumerable<JsTreeNode> GetAllLeafNodes(JsTreeNode root)
        {
            List<JsTreeNode> leafs = root.Nodes.Where(node => node.Nodes.Count == 0).ToList();
            IEnumerable<JsTreeNode> branches = root.Nodes.Where(node => node.Nodes.Count > 0);
            foreach (JsTreeNode branch in branches)
                leafs.AddRange(GetAllLeafNodes(branch));

            if (root.Nodes.Count == 0)
                leafs.Add(root);

            return leafs;
        }

        #endregion
    }
}
