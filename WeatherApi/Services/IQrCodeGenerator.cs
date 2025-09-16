namespace WeatherApi.Services;

public interface IQrCodeGenerator
{
    string GeneratePngBase64(string content);
}
