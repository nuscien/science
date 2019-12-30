﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trivial.Data;
using Trivial.Text;

namespace Trivial.UnitTest.Text
{
    /// <summary>
    /// CSV unit test.
    /// </summary>
    [TestClass]
    public class CsvUnitTest
    {
        /// <summary>
        /// Tests CSV parser.
        /// </summary>
        public void TestCsvParser()
        {
            var text = "ab,cd,\"efg\",56789,!!!\nhijk,l,mn,43210";
            var parser = new CsvParser(text);
            var col = parser.ToList();

            Assert.AreEqual(2, col.Count);
            Assert.AreEqual(4, col[0].Count);
            Assert.AreEqual("ab", col[0][0]);
            Assert.AreEqual("cd", col[0][1]);
            Assert.AreEqual("efg", col[0][2]);
            Assert.AreEqual("56789", col[0][3]);
            Assert.AreEqual(4, col[1].Count);
            Assert.AreEqual("!!!\nhijk", col[1][0]);
            Assert.AreEqual("l", col[1][1]);
            Assert.AreEqual("mn", col[1][2]);
            Assert.AreEqual("43210", col[1][3]);

            var models = parser.ConvertTo<JsonModel>(new[] { "A", "B", "C", "Num" }).ToList();
            Assert.AreEqual(2, models.Count);
            Assert.AreEqual("ab", models[0].A);
            Assert.AreEqual("cd", models[0].B);
            Assert.AreEqual("efg", models[0].C);
            Assert.AreEqual("56789", models[0].Num);
            Assert.AreEqual("!!!\nhijk", models[1].A);
            Assert.AreEqual("l", models[1].B);
            Assert.AreEqual("mn", models[1].C);
            Assert.AreEqual("43210", models[1].Num);
        }
    }
}
