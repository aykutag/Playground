using System;
using System.Threading;

namespace Future
{
    public class Future<T>
    {
        private bool _isComplete;

        private Thread _runner;

        private Exception _ex;

        private readonly object _lock = new object();

        private T _result;

        public Future(Func<T> function)
        {
            Execute(Wrapped(function));
        }

        private Action Wrapped(Func<T> function)
        {
            return () =>
            {              
                try
                {
                    _result = function();

                    lock (_lock)
                    {
                        _isComplete = true;
                    }
                }
                catch (Exception ex)
                {
                    _ex = ex;
                }                
            };
        }

        private void Execute(Action wrapped)
        {
            _runner = new Thread(new ThreadStart(wrapped));
    
            _runner.Start();
        }

        public T Resolve()
        {
            lock (_lock)
            {
                if (_isComplete)
                {
                    return _result;
                }
            }

            _runner.Join();
                
            if (_ex != null)
            {
                throw _ex;
            }

            return _result;        
        }

        public Future<T> Then(Func<T> next)
        {
            return new Future<T>(() =>
            {
                Resolve();

                return next();
            });
        }  

        public Future<Y> Then<Y>(Func<T, Y> next)
        {
            return new Future<Y>(() =>
            {
                var previousResult = Resolve();

                return next(previousResult);
            });
        }  
    }
}
