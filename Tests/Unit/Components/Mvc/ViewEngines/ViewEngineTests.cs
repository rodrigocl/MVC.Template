﻿using MvcTemplate.Components.Mvc;
using NUnit.Framework;
using System;

namespace MvcTemplate.Tests.Unit.Components.Mvc
{
    [TestFixture]
    public class ViewEngineTests
    {
        private ViewEngine viewEngine;

        [SetUp]
        public void TestInit()
        {
            viewEngine = new ViewEngine();
        }

        #region Constructor: ViewEngine()

        [Test]
        public void ViewEngine_SetsAreaViewLocationFormats()
        {
            String[] expected = { "~/Views/{2}/{1}/{0}.cshtml" };
            String[] actual = viewEngine.AreaViewLocationFormats;

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ViewEngine_SetsAreaMasterLocationFormats()
        {
            String[] expected = { "~/Views/{2}/{1}/{0}.cshtml" };
            String[] actual = viewEngine.AreaMasterLocationFormats;

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ViewEngine_SetsAreaPartialViewLocationFormats()
        {
            String[] expected = { "~/Views/{2}/{1}/{0}.cshtml" };
            String[] actual = viewEngine.AreaPartialViewLocationFormats;

            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion
    }
}
