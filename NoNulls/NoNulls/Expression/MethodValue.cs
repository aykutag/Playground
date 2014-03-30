using System;

namespace NoNulls
{
    public class MethodValue<T> 
    {
        private readonly bool _hasValue;
        private T _value;

        public T Value
        {
            get
            {
                if (HasValue())
                {
                    return _value;   
                }
                throw new NoValueException("The property does not exist. Failure was at {0}", Failure);
            }
            private set
            {
                _value = value;
            }
        }

        public String Failure { get; set; }

        public MethodValue(T value, String failure, bool hasValue)
        {
            _hasValue = hasValue;
            Value = value;
            Failure = failure;
        }


        public bool HasValue()
        {
            return _hasValue;
        }
    }
}