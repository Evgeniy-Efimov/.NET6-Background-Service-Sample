using BackgroundService.Http.Interfaces;

namespace BackgroundService.Http
{
    public class ErrorResponse : IErrorResponse
    {
        public int? Code { get; set; }
        public string Message { get; set; }

        public string GetErrorMessage()
        {
            return Message;
        }

        public string GetStatusCode()
        {
            return Code?.ToString() ?? string.Empty;
        }
    }
}
