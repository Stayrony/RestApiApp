using System;
using System.Net;

namespace RestApiApp.Models.Rest
{
    public class RestResult<T>
    {
        public T Result { get; private set; }
        public bool IsSuccess { get; private set; }
        public HttpStatusCode Code { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Exception { get; private set; }

        public static RestResult<T> Ok(T result) => new RestResult<T>
        {
            IsSuccess = true,
            Code = HttpStatusCode.OK,
            Result = result,
            ErrorMessage = null
        };

        public static RestResult<T> Fail
            (HttpStatusCode resultCode) =>
            new RestResult<T>
            {
                IsSuccess = false,
                Code = resultCode,
                Result = default(T),
                Exception = null,
                ErrorMessage = null
            };

        public static RestResult<T> Fail
            (HttpStatusCode resultCode, string errorMessage) =>
            new RestResult<T>
            {
                IsSuccess = false,
                Code =resultCode,
                Result = default(T),
                ErrorMessage = errorMessage
            };

        public static RestResult<T> Fatal
            (Exception ex) =>
            new RestResult<T>
            {
                IsSuccess = false,
                //Code = HttpStatusCode.BadGateway,
                Result = default(T),
                Exception = ex,
                ErrorMessage = null
            };

        public static RestResult<T> Ok() => Ok(default(T));
    }
}
