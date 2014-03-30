using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace NoNulls
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void Test1()
        {
            var user = new User();

            var name = Option.Safe(() => user.School.District.Street.Name);

            Assert.IsNull(name);
        }


        [TestMethod]
        public void Test2()
        {
            var user = new User
                       {
                           School = new School()
                       };

            var name = Option.Safe(() => user.School.District);

            Assert.IsNull(name);
        }

        [TestMethod]
        public void Test3()
        {
            var user = new User
            {
                School = new School
                         {
                             District = new District()
                         }
            };

            var name = Option.Safe(() => user.School.District);

            Assert.IsNotNull(name);
        }
    }
}
