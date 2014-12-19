using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangent.Parsing.Errors
{
    public class ResultOrError<R, E>
    {
        public readonly bool Success;
        public readonly R Result;
        public readonly E Error;

        public ResultOrError(R result)
        {
            Result = result;
            Success = true;
        }

        public ResultOrError(E error)
        {
            Error = error;
            Success = false;
        }

        public void Dispatch(Action<R> onSuccess, Action<E> onError)
        {
            if (Success) {
                onSuccess(Result);
            } else {
                onError(Error);
            }
        }

        public T Dispatch<T>(Func<R, T> onSuccess, Func<E, T> onError)
        {
            if (Success) {
                return onSuccess(Result);
            } else {
                return onError(Error);
            }
        }
    }
}
