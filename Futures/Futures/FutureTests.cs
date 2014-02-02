using System;
using System.Threading;
using NUnit.Framework;

namespace Future
{
    [TestFixture]
    public class FutureTests
    {
        [Test]
        public void TestFuture()
        {
            int count = 0;
         
            Func<int> action = () =>
            {
                Console.WriteLine("Running " + count);
                Thread.Sleep(TimeSpan.FromMilliseconds(count * 100));
                Console.WriteLine("Resolving " + count);

                count ++;
                return count;
            };

            var future = new Future<int>(action).Then(action).Then(action);

            Console.WriteLine("All setup, nonblock but now wait");

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("Requesting result");

            var result = future.Resolve();

            Assert.AreEqual(3, result);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestException()
        {
            Func<int> action = () =>
            {
                throw new Exception("Error");
            };

            var future = new Future<int>(action);

            future.Resolve();
        }
    }
}
