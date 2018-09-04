using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemplateWorkbench;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void TestSerialization()
        {
            {
                var model = new Model();
                model.DataModel = "Data model";
                model.TemplateCode = "Template code";
                model.projectPath = "test.proj";
                model.SaveProject();
                {
                    var fi = new FileInfo("test.proj");
                    Assert.IsTrue(fi.Exists);
                    Assert.AreEqual(205, fi.Length);
                }
                {
                    var fi = new FileInfo("datamodel.json");
                    Assert.IsTrue(fi.Exists);
                    Assert.AreEqual(10, fi.Length);
                }
                {
                    var fi = new FileInfo("templates.stg");
                    Assert.IsTrue(fi.Exists);
                    Assert.AreEqual(13, fi.Length);
                }
            }
            {
                var model = Model.LoadProject("test.proj");
                Assert.AreEqual("Data model", model.DataModel);
                Assert.AreEqual("Template code", model.TemplateCode);
            }
        }
    }
}
