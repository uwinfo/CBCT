using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.AspNetCore.Http;

namespace Su
{
    public class ImageHelper
    {
        public class WidthAndHeight
        {
            public int Width { get; set; }

            public int Height { get; set; }
        }

        public static WidthAndHeight GetWidthAndHeight(string imagePath)
        {
            // 載入圖片
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                return new WidthAndHeight()
                {
                    Width = image.Width,
                    Height = image.Height
                };
            }
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="width">二维码宽，默认500</param>
        /// <param name="height">二维码高，默认500</param>
        public static MemoryStream GetQrCodeMemoryStream(string value, int width = 500, int height = 500)
        {
            var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    DisableECI = true,
                    CharacterSet = "UTF-8",
                    Width = width,
                    Height = height,
                    Margin = 1
                }
            };
            var image = writer.WriteAsImageSharp<Rgba32>(value);
            var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return ms;
        }

        public static void ResizeAndSaveOriginalImage(Stream inputStream, string outputPath,
            int newWidth = 0, int newHeight = 0,
            int jpegQuality = 85,
            PngCompressionLevel compressionLevel = PngCompressionLevel.Level7)
        {
            // Load the image from the input stream
            using (Image image = Image.Load(inputStream))
            {
                // Resize the image
                if (newWidth > 0 && newHeight > 0)
                {
                    image.Mutate(x => x.Resize(newWidth, newHeight));
                }

                ImageEncoder encoder = null;
                if (outputPath.ToLower().EndsWith(".png"))
                {
                    encoder = new PngEncoder
                    {
                        CompressionLevel = compressionLevel // Set the compression level (0-9)
                    };
                }

                if (outputPath.ToLower().EndsWith(".jpg") || outputPath.ToLower().EndsWith(".jpeg"))
                {
                    encoder = new JpegEncoder
                    {
                        Quality = jpegQuality
                    };
                }

                if (encoder == null)
                {
                    image.Save(outputPath);
                }
                else
                {
                    image.Save(outputPath, encoder);
                }
            }
        }

        public static void ResizeAndSaveWebpImage(IFormFile file, string outputFilename,
          int newWidth = 0, int newHeight = 0,
          int webpQuality = 80,
          int maxWidth = 0, int maxHeight = 0)
        {
            // Load the image from the input stream
            using var stream = file.OpenReadStream();
            using Image image = Image.Load(stream);
            ResizeAndSaveWebpImage(image, outputFilename, newWidth, newHeight, webpQuality: webpQuality, maxWidth: maxWidth, maxHeight: maxHeight);
        }

        public static void ResizeAndSaveWebpImage(Image image, string outputFilename,
        int newWidth = 0, int newHeight = 0,
        int webpQuality = 80,
         int maxWidth = 0, int maxHeight = 0
          )// 新增 WebP 壓縮品質
        {
            // Resize the image
            if (newWidth > 0 && newHeight > 0)
            {
                image.Mutate(x => x.Resize(newWidth, newHeight));
            }

            if (maxHeight > 0 && maxWidth > 0)
            {
                double ratioX = 1;
                double ratioY = 1;
                if (image.Width > maxWidth)
                {
                    ratioX = Convert.ToDouble(maxWidth) / image.Width;
                }

                if (image.Height > maxHeight)
                {
                    ratioY = Convert.ToDouble(maxHeight) / image.Height;
                }

                if (ratioX < 1 || ratioY < 1)
                {
                    if (ratioX < ratioY)
                    {
                        image.Mutate(x => x.Resize(maxWidth, Convert.ToInt32(image.Height * ratioX)));
                    }
                    else
                    {
                        image.Mutate(x => x.Resize(Convert.ToInt32(image.Width * ratioY), maxHeight));
                    }
                }
            }

            outputFilename = Path.ChangeExtension(outputFilename, ".webp");

            var encoder = new WebpEncoder
            {
                Quality = webpQuality
            };

            image.Save(outputFilename, encoder);
        }


        //public static Bitmap ResizeImage(Image image, int destWidth, int destHeight)
        //{
        //    int originalW = image.Width;
        //    int originalH = image.Height;

        //    float ratio = Math.Min(Convert.ToSingle(destWidth) / originalW, Convert.ToSingle(destHeight) / originalH);

        //    int newW = Convert.ToInt32(originalW * ratio);
        //    int newH = Convert.ToInt32(originalH * ratio);

        //    var destRect = new Rectangle(0, 0, newW, newH);
        //    var destImage = new Bitmap(newW, newH);

        //    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        //    using (var graphics = Graphics.FromImage(destImage))
        //    {
        //        graphics.CompositingMode = CompositingMode.SourceCopy;
        //        graphics.CompositingQuality = CompositingQuality.HighQuality;
        //        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        graphics.SmoothingMode = SmoothingMode.HighQuality;
        //        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //        using (var wrapMode = new ImageAttributes())
        //        {
        //            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        //            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        //        }
        //    }

        //    return destImage;
        //}

        //public static Image AddWaterMark(Image img, string waterMark, float fontSize)
        //{
        //    int width = img.Width;
        //    int height = img.Height;

        //    Brush brush = new SolidBrush(Color.FromArgb(255, 149, 149, 128));

        //    Font font = new("微軟正黑體", fontSize, GraphicsUnit.Pixel);
        //    using (Graphics graphics = Graphics.FromImage(img))
        //    {
        //        PointF firstLocation = new PointF(0f, 0f);

        //        // Set maximum layout size.
        //        SizeF layoutSize = new SizeF(width, 500);

        //        // Measure string.
        //        SizeF stringSize = new SizeF();
        //        stringSize = graphics.MeasureString(waterMark, font, layoutSize);
        //        float x = (Convert.ToSingle(width) - stringSize.Width) / 2;
        //        RectangleF rectangle = new RectangleF(x, height / 4, width, stringSize.Height);

        //        graphics.DrawString(waterMark, font, brush, rectangle);
        //    }

        //    return img;
        //}

        ///// <summary>
        ///// 移除圖片的旋較屬性
        ///// </summary>
        //public static Image RemoveOrientation(Image img)
        //{
        //    int exifOrientationID = 0x112;
        //    if (img.PropertyIdList.Contains(exifOrientationID))
        //    {
        //        var prop = img.GetPropertyItem(exifOrientationID);

        //        int val = BitConverter.ToUInt16(prop.Value, 0);
        //        var rot = RotateFlipType.RotateNoneFlipNone;

        //        if (val == 3 || val == 4)
        //            rot = RotateFlipType.Rotate180FlipNone;
        //        else if (val == 5 || val == 6)
        //            rot = RotateFlipType.Rotate90FlipNone;
        //        else if (val == 7 || val == 8)
        //            rot = RotateFlipType.Rotate270FlipNone;

        //        if (val == 2 || val == 4 || val == 5 || val == 7)
        //            rot |= RotateFlipType.RotateNoneFlipX;

        //        if (rot != RotateFlipType.RotateNoneFlipNone)
        //        {
        //            img.RotateFlip(rot);
        //            img.RemovePropertyItem(exifOrientationID);
        //        }
        //    }

        //    return img;
        //}
    }
}
