namespace Galaxy.Lol.Application.Results
{

    public class Result
    {
        public bool IsSuccess { get; }
        public int ErrorCode { get; set; }
        public string? Message { get; }
        public IReadOnlyCollection<string> Errors { get; }

        public Result()
        {
            Errors = [];
        }

        protected internal Result(bool isSuccess, string message, int errorCode = 0, IEnumerable<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            ErrorCode = errorCode;
            Errors = errors?.ToList().AsReadOnly() ?? (IReadOnlyCollection<string>)[];
        }

        public static Result Success() => new(true, "Proceso ejecutado correctamente");
        public static Result Success(string message) => new(true, message);
        public static Result Failure(string error) => new(false, error);
        public static Result Failure(string error, int errorCode) => new(false, error, errorCode);
        public static Result Failure(IEnumerable<string> errors) => new(false, "Proceso fallido", 0, errors);
    }

    public class Result<T> : Result
    {
        public T? Data { get; }

        public Result() { }

        protected internal Result(bool isSuccess, string message, T? data = default, int errorCode = 0,
                                  IEnumerable<string>? errors = null)
            : base(isSuccess, message, errorCode, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new(true, "Proceso ejecutado correctamente", data);
        public static Result<T> Success(T data, string message) => new(true, message, data);
        public static new Result<T> Failure(string error) => new(false, error);
        public static new Result<T> Failure(string error, int errorCode) => new(false, error, default, errorCode);
        public static new Result<T> Failure(IEnumerable<string> errors) => new(false, "Proceso fallido", default, 0, errors);
    }
}
