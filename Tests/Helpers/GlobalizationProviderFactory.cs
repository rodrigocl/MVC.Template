﻿using MvcTemplate.Components.Mvc;
using NSubstitute;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace MvcTemplate.Tests.Helpers
{
    public class GlobalizationProviderFactory
    {
        public static IGlobalizationProvider CreateProvider()
        {
            IGlobalizationProvider provider = Substitute.For<IGlobalizationProvider>();
            SetUpSubstitute(provider);

            return provider;
        }

        private static void SetUpSubstitute(IGlobalizationProvider provider)
        {
            List<Language> languages = new List<Language>()
            {
                new Language()
                {
                    Culture = new CultureInfo("en-GB"),
                    Abbrevation = "en",
                    IsDefault = true,
                    Name = "English"
                },
                new Language()
                {
                    Culture = new CultureInfo("lt-LT"),
                    Abbrevation = "lt",
                    IsDefault = false,
                    Name = "Lietuvių"
                }
            };

            provider.When(language => language.CurrentLanguage = Arg.Any<Language>()).Do(value =>
            {
                Thread.CurrentThread.CurrentUICulture = value.Arg<Language>().Culture;
                Thread.CurrentThread.CurrentCulture = value.Arg<Language>().Culture;
            });
            provider.CurrentLanguage.Returns(languages[0]);
            provider.DefaultLanguage.Returns(languages[0]);
            provider.Languages.Returns(languages);
            provider["en"].Returns(languages[0]);
            provider["lt"].Returns(languages[1]);
        }
    }
}
