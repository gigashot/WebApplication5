namespace WebApplication5.Models.DTOs
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static ApiResponse Ok(string message = null)
        {
            return new ApiResponse { Success = true, Message = message };
        }

        public static ApiResponse Error(string message)
        {
            return new ApiResponse { Success = false, Message = message };
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = null)
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }
    }
}
