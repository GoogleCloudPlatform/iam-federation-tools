using System;
using System.Reflection;

namespace Google.Solutions.AAAuth
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Remove all enclosing <c>AggregateException</c> and 
        /// <c>TargetInvocationException</c> exceptions.
        /// </summary>
        public static Exception Unwrap(this Exception e)
        {
            if (e is AggregateException aggregate &&
                aggregate.InnerException != null)
            {
                return aggregate.InnerException.Unwrap();
            }
            else if (e is TargetInvocationException target &&
                target.InnerException != null)
            {
                return target.InnerException.Unwrap();
            }
            else
            {
                return e;
            }
        }
    }
}
