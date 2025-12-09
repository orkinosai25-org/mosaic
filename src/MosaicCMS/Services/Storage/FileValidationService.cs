namespace MosaicCMS.Services.Storage;

/// <summary>
/// Service for validating file uploads using magic number (file signature) detection
/// Prevents malicious files disguised with valid MIME types
/// </summary>
public static class FileValidationService
{
    // Content type constants
    private const string SvgContentType = "image/svg+xml";
    private const string PlainTextContentType = "text/plain";
    private const string CsvContentType = "text/csv";
    private const string WebPContentType = "image/webp";

    // File signatures (magic numbers) for allowed file types
    private static readonly Dictionary<string, List<byte[]>> FileSignatures = new()
    {
        // Images
        { "image/jpeg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // JPEG with EXIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // JPEG
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // JPEG
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }  // JPEG
            }
        },
        { "image/png", new List<byte[]>
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        },
        { "image/gif", new List<byte[]>
            {
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, // GIF87a
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }  // GIF89a
            }
        },
        { "image/webp", new List<byte[]>
            {
                new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF (check additional bytes for WEBP)
            }
        },
        // Documents
        { "application/pdf", new List<byte[]>
            {
                new byte[] { 0x25, 0x50, 0x44, 0x46 } // %PDF
            }
        },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new List<byte[]>
            {
                new byte[] { 0x50, 0x4B, 0x03, 0x04 } // ZIP (DOCX is ZIP-based)
            }
        },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new List<byte[]>
            {
                new byte[] { 0x50, 0x4B, 0x03, 0x04 } // ZIP (XLSX is ZIP-based)
            }
        },
        { "application/msword", new List<byte[]>
            {
                new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } // DOC
            }
        },
        { "application/vnd.ms-excel", new List<byte[]>
            {
                new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } // XLS
            }
        }
    };

    /// <summary>
    /// Validates that a file's magic number matches its declared MIME type
    /// </summary>
    /// <param name="stream">File stream to validate</param>
    /// <param name="contentType">Declared MIME type</param>
    /// <returns>True if file signature matches content type</returns>
    public static async Task<bool> ValidateFileSignatureAsync(Stream stream, string contentType)
    {
        // SVG and text files don't have binary signatures
        if (contentType == SvgContentType || contentType == PlainTextContentType || contentType == CsvContentType)
        {
            return await ValidateTextFileAsync(stream, contentType);
        }

        if (!FileSignatures.TryGetValue(contentType, out var signatures))
        {
            // Unknown type - reject
            return false;
        }

        // Read the first 16 bytes (enough for most signatures)
        var headerBytes = new byte[16];
        var position = stream.Position;
        
        try
        {
            var bytesRead = await stream.ReadAsync(headerBytes.AsMemory(0, 16));
            if (bytesRead == 0)
            {
                return false;
            }

            // Check if any signature matches
            foreach (var signature in signatures)
            {
                if (headerBytes.Take(signature.Length).SequenceEqual(signature))
                {
                    return true;
                }
            }

            // Special case for WebP - need to check RIFF + WEBP
            if (contentType == WebPContentType && bytesRead >= 12)
            {
                var isRiff = headerBytes.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 });
                var isWebP = headerBytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 });
                if (isRiff && isWebP)
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            // Reset stream position
            stream.Position = position;
        }
    }

    /// <summary>
    /// Validates text-based files (SVG, TXT, CSV) by checking for valid text content
    /// </summary>
    private static async Task<bool> ValidateTextFileAsync(Stream stream, string contentType)
    {
        var position = stream.Position;
        
        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            var content = await reader.ReadToEndAsync();

            // Check for null bytes (indicates binary file masquerading as text)
            if (content.Contains('\0'))
            {
                return false;
            }

            // For SVG, verify it starts with XML declaration or svg tag
            if (contentType == SvgContentType)
            {
                var trimmed = content.TrimStart();
                return trimmed.StartsWith("<?xml") || trimmed.StartsWith("<svg");
            }

            // For text files, just verify no null bytes (already checked above)
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            stream.Position = position;
        }
    }

    /// <summary>
    /// Gets maximum allowed file size for a given content type
    /// </summary>
    public static long GetMaxFileSize(string contentType)
    {
        // Different size limits based on content type
        return contentType switch
        {
            SvgContentType => 1 * 1024 * 1024,       // 1 MB for SVG
            PlainTextContentType => 5 * 1024 * 1024,  // 5 MB for text
            CsvContentType => 5 * 1024 * 1024,        // 5 MB for CSV
            _ when contentType.StartsWith("image/") => 10 * 1024 * 1024,  // 10 MB for images
            _ => 10 * 1024 * 1024  // 10 MB default
        };
    }
}
