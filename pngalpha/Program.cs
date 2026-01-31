using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace pngalpha;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: pngalpha <image_on_white> <image_on_black> <output_file>");
            Console.WriteLine();
            Console.WriteLine("Creates a transparent PNG file from two images:");
            Console.WriteLine("one on a white background and one on a black background.");
            Console.WriteLine();
            Console.WriteLine("Example: pngalpha image_white.png image_black.png output.png");
            return 1;
        }

        string imgOnWhitePath = args[0];
        string imgOnBlackPath = args[1];
        string outputPath = args[2];

        try
        {
            await ExtractAlphaTwoPass(imgOnWhitePath, imgOnBlackPath, outputPath);
            Console.WriteLine($"Transparent PNG file created: {outputPath}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Two-pass alpha extraction algorithm.
    /// Takes two images of the same subject - one on white background, one on black background.
    /// Calculates the alpha channel and recovers the original foreground color.
    /// </summary>
    static async Task ExtractAlphaTwoPass(string imgOnWhitePath, string imgOnBlackPath, string outputPath)
    {
        using var imgWhite = await Image.LoadAsync<Rgba32>(imgOnWhitePath);
        using var imgBlack = await Image.LoadAsync<Rgba32>(imgOnBlackPath);

        if (imgWhite.Width != imgBlack.Width || imgWhite.Height != imgBlack.Height)
        {
            throw new InvalidOperationException("Dimension mismatch: Images must be identical size.");
        }

        int width = imgWhite.Width;
        int height = imgWhite.Height;

        using var output = new Image<Rgba32>(width, height);

        // Distance between White (255,255,255) and Black (0,0,0)
        // sqrt(255^2 + 255^2 + 255^2) ≈ 441.67
        double bgDist = Math.Sqrt(3.0 * 255 * 255);

        imgWhite.ProcessPixelRows(imgBlack, output, (whiteAccessor, blackAccessor, outputAccessor) =>
        {
            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> whiteRow = whiteAccessor.GetRowSpan(y);
                Span<Rgba32> blackRow = blackAccessor.GetRowSpan(y);
                Span<Rgba32> outputRow = outputAccessor.GetRowSpan(y);

                for (int x = 0; x < width; x++)
                {
                    Rgba32 pixelWhite = whiteRow[x];
                    Rgba32 pixelBlack = blackRow[x];

                    // Calculate the distance between the two observed pixels
                    double pixelDist = Math.Sqrt(
                        Math.Pow(pixelWhite.R - pixelBlack.R, 2) +
                        Math.Pow(pixelWhite.G - pixelBlack.G, 2) +
                        Math.Pow(pixelWhite.B - pixelBlack.B, 2)
                    );

                    // THE FORMULA:
                    // If the pixel is 100% opaque, it looks the same on Black and White (pixelDist = 0).
                    // If the pixel is 100% transparent, it looks exactly like the backgrounds (pixelDist = bgDist).
                    double alpha = 1.0 - (pixelDist / bgDist);

                    // Clamp results to 0-1 range
                    alpha = Math.Clamp(alpha, 0.0, 1.0);

                    // Color Recovery:
                    // We use the image on black to recover the color, dividing by alpha 
                    // to un-premultiply it (brighten the semi-transparent pixels)
                    double rOut = 0, gOut = 0, bOut = 0;

                    if (alpha > 0.01)
                    {
                        // Recover foreground color from the version on black
                        // (C - (1-alpha) * BG) / alpha
                        // Since BG is black (0,0,0), this simplifies to C / alpha
                        rOut = pixelBlack.R / alpha;
                        gOut = pixelBlack.G / alpha;
                        bOut = pixelBlack.B / alpha;
                    }

                    outputRow[x] = new Rgba32(
                        (byte)Math.Min(255, Math.Round(rOut)),
                        (byte)Math.Min(255, Math.Round(gOut)),
                        (byte)Math.Min(255, Math.Round(bOut)),
                        (byte)Math.Round(alpha * 255)
                    );
                }
            }
        });

        await output.SaveAsPngAsync(outputPath);
    }
}
