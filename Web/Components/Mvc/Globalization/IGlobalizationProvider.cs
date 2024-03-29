﻿using System;
using System.Collections.Generic;

namespace MvcTemplate.Components.Mvc
{
    public interface IGlobalizationProvider
    {
        Language DefaultLanguage { get; }
        Language CurrentLanguage { get; set; }
        IEnumerable<Language> Languages { get; }

        Language this[String abbrevation] { get; }
    }
}
