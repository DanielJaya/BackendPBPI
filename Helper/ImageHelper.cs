namespace BackendPBPI.Helper
{
    public static class ImageHelper
    {
        // Allowed image extensions
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        // Max file size: 5MB
        private static readonly long MaxFileSize = 5 * 1024 * 1024;

        /// <summary>
        /// Validate image file
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File tidak boleh kosong");
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                return (false, $"Ukuran file tidak boleh lebih dari {MaxFileSize / 1024 / 1024}MB");
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return (false, $"Format file tidak didukung. Hanya mendukung: {string.Join(", ", AllowedExtensions)}");
            }

            // Check content type
            if (!file.ContentType.StartsWith("image/"))
            {
                return (false, "File harus berupa gambar");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Convert IFormFile to byte array
        /// </summary>
        public static async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Convert byte array to Base64 string
        /// </summary>
        public static string ConvertToBase64(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Get full Base64 data URI
        /// </summary>
        public static string GetBase64DataUri(byte[] imageBytes, string contentType)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            var base64 = Convert.ToBase64String(imageBytes);
            return $"data:{contentType};base64,{base64}";
        }
    }
}