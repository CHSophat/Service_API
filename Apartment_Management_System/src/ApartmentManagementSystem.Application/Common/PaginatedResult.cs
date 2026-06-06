namespace ApartmentManagementSystem.Application.Common
{
	public record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
	{
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool IsSuccess { get; set; }
        public object? Value { get; set; }
        public object Error { get; set; }
    }
}
