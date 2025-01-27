using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GwentCardDownloader
{
    public class ImageProcessor
    {
        public async Task ProcessImageAsync(string imagePath, ImageQuality quality)
        {
            switch (quality)
            {
                case ImageQuality.Low:
                    await OptimizeForLowQuality(imagePath);
                    break;
                case ImageQuality.Medium:
                    await OptimizeForMediumQuality(imagePath);
                    break;
                case ImageQuality.High:
                    await OptimizeForHighQuality(imagePath);
                    break;
            }
        }

        private async Task OptimizeForLowQuality(string imagePath)
        {
            using (var image = Image.FromFile(imagePath))
            {
                var resizedImage = ResizeImage(image, 100, 100);
                var grayscaleImage = ConvertToGrayscale(resizedImage);
                grayscaleImage.Save(imagePath, ImageFormat.Jpeg);
            }
            await Task.CompletedTask;
        }

        private async Task OptimizeForMediumQuality(string imagePath)
        {
            using (var image = Image.FromFile(imagePath))
            {
                var resizedImage = ResizeImage(image, 200, 200);
                var brightenedImage = AdjustBrightness(resizedImage, 1.2f);
                brightenedImage.Save(imagePath, ImageFormat.Jpeg);
            }
            await Task.CompletedTask;
        }

        private async Task OptimizeForHighQuality(string imagePath)
        {
            using (var image = Image.FromFile(imagePath))
            {
                var resizedImage = ResizeImage(image, 300, 300);
                var enhancedImage = EnhanceColors(resizedImage);
                enhancedImage.Save(imagePath, ImageFormat.Jpeg);
            }
            await Task.CompletedTask;
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            var resized = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resized;
        }

        private Image ConvertToGrayscale(Image image)
        {
            var grayscale = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(grayscale))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                    new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                    new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return grayscale;
        }

        private Image AdjustBrightness(Image image, float brightness)
        {
            var adjusted = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(adjusted))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { brightness, 0, 0, 0, 0 },
                    new float[] { 0, brightness, 0, 0, 0 },
                    new float[] { 0, 0, brightness, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return adjusted;
        }

        private Image EnhanceColors(Image image)
        {
            var enhanced = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(enhanced))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { 1.2f, 0, 0, 0, 0 },
                    new float[] { 0, 1.2f, 0, 0, 0 },
                    new float[] { 0, 0, 1.2f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return enhanced;
        }
    }
}
