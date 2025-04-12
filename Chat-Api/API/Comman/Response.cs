namespace API.Comman
{
    public class Response<T>
    {
        public bool IsSuccess { get; set; } = true;
        public string? Message { get; set; } = string.Empty;
        public T Data { get; }
        public string? Error { get; set; } = string.Empty;
        public Response(bool isSuccess, T data, string? error, string? message)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
            Message = message;
        }
        public static Response<T> Success(T data, string? message = null)
        {
            return new Response<T>(true, data, null, message);
        }
        public static Response<T> Failure(string? error, string? message = null)
        {
            return new Response<T>(false, default!, error, message);
        }
    }
}
