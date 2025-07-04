using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MV.ApplicationLayer.ServiceInterfaces;
using QRCoder;

namespace MV.ApplicationLayer.Services
{
    public class QrCodeService : IQrCodeService
    {
        public async Task<string> GenerateQrCodeAsync(int bookingId)
        {
            var qrContent = $"BOOKING-{bookingId}-{DateTime.Now:yyyyMMdd}";
            return await GenerateQrCodeFromContentAsync(qrContent);
        }

        public async Task<string> GenerateQrCodeFromContentAsync(string content)
        {
            return await Task.Run(() =>
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                // Sử dụng BitmapByteQRCode để tạo QR code nếu không có class QRCode
                var qrCode = new BitmapByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                return Convert.ToBase64String(qrCodeBytes);
            });
        }

        public int? DecodeBookingIdFromQrCode(string qrContent)
        {
            try
            {
                if (string.IsNullOrEmpty(qrContent))
                    return null;

                // Format: BOOKING-{bookingId}-{date}
                var parts = qrContent.Split('-');
                if (parts.Length >= 2 && parts[0] == "BOOKING")
                {
                    if (int.TryParse(parts[1], out var bookingId))
                    {
                        return bookingId;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
} 