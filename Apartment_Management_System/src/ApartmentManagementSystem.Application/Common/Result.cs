namespace ApartmentManagementSystem.Application.Common
{
	public class Result<T>
	{
        public object Error;

        public bool IsSuccess { get; private set; }
		public string? Message { get; private set; }
		public T? Data { get; private set; }
		public List<string>? Errors { get; private set; }
        public object? Value { get; set; }



        public Result(bool isSuccess, T? data, string? message, List<string>? errors)
		{
			IsSuccess = isSuccess;
			Data = data;
			Message = message;
			Errors = errors;
		}

		public static Result<T> Success(T data, string? message = null)
		{
			return new Result<T>(true, data, message, null);
		}

		public static Result<T> Failure(string message, List<string>? errors = null)
		{
			return new Result<T>(false, default, message, errors);
		}


	}

}
