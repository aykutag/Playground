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
            TestFutureImpl(action => new NewThreadFuture<int>(action));
            TestFutureImpl(action => new NewThreadPoolFuture<int>(action));
        }

        private void TestFutureImpl(Func<Func<int>, Future<int>> generator)
        {
            int count = 0;

            Func<int> action = () =>
            {
                Console.WriteLine("Running " + count);
                Thread.Sleep(TimeSpan.FromMilliseconds(count * 100));
                Console.WriteLine("Resolving " + count);

                count++;
                return count;
            };

            var future = generator(action).Then(action).Then(action);

            Console.WriteLine("All setup, nonblock but now wait");

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("Requesting result");

            var result = future.Resolve();

            Assert.AreEqual(3, result);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestThreadException()
        {
            Func<int> action = () =>
            {
                throw new Exception("Error");
            };

            var future = new NewThreadFuture<int>(action);

            future.Resolve();
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestThreadPoolException()
        {
            Func<int> action = () =>
            {
                throw new Exception("Error");
            };

            var future = new NewThreadPoolFuture<int>(action);

            future.Resolve();
        }
    }
}
