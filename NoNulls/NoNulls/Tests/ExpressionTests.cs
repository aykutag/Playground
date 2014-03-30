using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace NoNulls
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        [ExpectedException(typeof(NoValueException))]
        public void ShouldThrow()
        {
            var user = new User();

            var name = Option.Safe(() => user.School.District.Street.Number);

            var v = name.Value;
        }

        [TestMethod]
        public void TestBasicNullWithValueTypeTarget()
        {
            var user = new User();

            var name = Option.Safe(() => user.School.District.Street.Number);

            Assert.IsFalse(name.HasValue());
        }


        [TestMethod]
        public void TestGet()
        {
            var user = new User();

            var name = Option.Safe(() => user.GetSchool().District.Street.Name);

            Assert.IsFalse(name.HasValue()); 
        }


        [TestMethod]
        public void TestNullWithReferenceTypeTarget()
        {
            var user = new User
            {
                School = new School()
            };

            var name = Option.Safe(() => user.School.District);

            Assert.IsFalse(name.HasValue());
        }

        [TestMethod]
        public void TestNonNullWithMethods()
        {
            var user = new User
            {
                School = new School
                         {
                             District = new District
                                        {
                                            Street = new Street
                                                     {
                                                         Name = "foo"
                                                     }
                                        }
                         }
            };

            var name = Option.Safe(() => user.GetSchool().GetDistrict().GetStreet().Name);

            Assert.AreEqual(name.Value, "foo");
        }

        [TestMethod]
        public void TestNonNullsWithMethodCalls()
        {
            var user = new User
            {
                School = new School
                {
                    District = new District
                    {
                        Street = new Street
                                 {
                                     Name = "foo"
                                 }
                    }
                }
            };

            var name = Option.Safe(() => user.GetSchool().GetDistrict().GetStreet().GetName(1));

            Assert.AreEqual(name.Value, "foo1");
        }
    }
}
