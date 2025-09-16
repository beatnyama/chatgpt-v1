using System;
using QRCoder;

namespace WeatherApi.Services;

public class QrCodeGenerator : IQrCodeGenerator
{
    public string GeneratePngBase64(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("QR code content cannot be null or empty.", nameof(content));
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = pngQrCode.GetGraphic(pixelsPerModule: 20);
        return Convert.ToBase64String(qrCodeBytes);
    }
}
