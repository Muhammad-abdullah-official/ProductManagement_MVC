namespace ProductManagement.RESPONSES
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        // ── Factory helpers ──────────────────────
        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data, StatusCode = 200 };

        public static ApiResponse<T> Created(T data, string message = "Created") =>
            new() { Success = true, Message = message, Data = data, StatusCode = 201 };

        public static ApiResponse<T> Fail(string error, int statusCode = 400) =>
            new() { Success = false, Message = error, Errors = [error], StatusCode = statusCode };

        public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400) =>
            new() { Success = false, Message = "Validation failed", Errors = errors, StatusCode = statusCode };

        public static ApiResponse<T> NotFound(string message = "Resource not found") =>
            Fail(message, 404);

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized") =>
            Fail(message, 401);

        public static ApiResponse<T> Forbidden(string message = "Forbidden") =>
            Fail(message, 403);
    }
}
