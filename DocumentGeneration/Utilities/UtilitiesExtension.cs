using Razor.Templating.Core;
using SkiaSharp;
using ZXing;
using ZXing.QrCode.Internal;
using ZXing.QrCode;

namespace DocumentGeneration.Utilities
{
    public static class UtilitiesExtension
    {
        public static async Task<string> GenerateHtmlContent<T>(T invoice, string viewName)
        {
            return await RazorTemplateEngine.RenderAsync($"Views/{viewName}.cshtml", invoice);
        }

        public static string GenerateQRCodeDataUrl(string content)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    ErrorCorrection = ErrorCorrectionLevel.H,
                    Height = 200,
                    Width = 200,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(content);
            using var bitmap = new SKBitmap(pixelData.Width, pixelData.Height);

            // Copy pixel data to bitmap
            var info = new SKImageInfo(pixelData.Width, pixelData.Height);
            using var surface = SKSurface.Create(info);

            unsafe
            {
                fixed (byte* p = pixelData.Pixels)
                {
                    using var skData = SKData.Create((IntPtr)p, pixelData.Pixels.Length);
                    var skImage = SKImage.FromPixels(info, skData, pixelData.Width * 4);
                    surface.Canvas.DrawImage(skImage, 0, 0);
                }
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            // Convert to base64
            var base64String = Convert.ToBase64String(data.ToArray());
            return $"data:image/png;base64,{base64String}";
        }

        public static string GenerateBarcodeDataUrl(string content)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 80,
                    Width = 220,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(content);
            using var bitmap = new SKBitmap(pixelData.Width, pixelData.Height);

            // Copy pixel data to bitmap
            var info = new SKImageInfo(pixelData.Width, pixelData.Height);
            using var surface = SKSurface.Create(info);

            unsafe
            {
                fixed (byte* p = pixelData.Pixels)
                {
                    using var skData = SKData.Create((IntPtr)p, pixelData.Pixels.Length);
                    var skImage = SKImage.FromPixels(info, skData, pixelData.Width * 4);
                    surface.Canvas.DrawImage(skImage, 0, 0);
                }
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            // Convert to base64
            var base64String = Convert.ToBase64String(data.ToArray());
            return $"data:image/png;base64,{base64String}";
        }
    }
}
