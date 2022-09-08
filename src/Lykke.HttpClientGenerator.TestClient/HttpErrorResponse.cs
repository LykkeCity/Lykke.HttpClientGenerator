namespace Lykke.HttpClientGenerator.TestClient
{
    public class HttpErrorResponse
    {
        public string ErrorMessage { get; set; } = string.Empty;

        public override string ToString() => ErrorMessage;
    }
}