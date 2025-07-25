﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Trivial.Globalization;

/// <summary>
/// Stream unit test.
/// </summary>
[TestClass]
public class DateTimeUnitTest
{
    /// <summary>
    /// Tests data time utilities.
    /// </summary>
    [TestMethod]
    public void TestDateTimeUtilites()
    {
        var time = DateTime.Now;
        var s = DateTimeUtilities.ToMessageTimeString(time);
        Assert.IsNotEmpty(s);
        time = DateTime.Now.AddDays(1);
        s = DateTimeUtilities.ToMessageTimeString(time);
        Assert.IsNotEmpty(s);
        time = DateTime.Now.AddMonths(1);
        s = DateTimeUtilities.ToMessageTimeString(time);
        Assert.IsNotEmpty(s);
        time = DateTime.Now.AddYears(1);
        s = DateTimeUtilities.ToMessageTimeString(time);
        Assert.IsNotEmpty(s);
        time = DateTime.Now.AddDays(-1);
        s = DateTimeUtilities.ToMessageTimeString(time);
        Assert.IsNotEmpty(s);
        time = DateTime.Now.AddMonths(1);
        s = DateTimeUtilities.ToMessageTimeString(time);
        Assert.IsNotEmpty(s);
        time = DateTime.Now.AddYears(-1);
        s = DateTimeUtilities.ToMessageTimeString(time);
        Assert.IsNotEmpty(s);
    }
}
