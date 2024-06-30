namespace BackgroundService.Http.Interfaces
{
    public interface IErrorResponse
    {
        public string GetStatusCode();

        public string GetErrorMessage();
    }
}
