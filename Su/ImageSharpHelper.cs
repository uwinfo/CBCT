using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Su
{
    public class ImageSharpHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="text"></param>
        /// <param name="saveFullFilename"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontName"></param>
        /// <param name="angle">文字旋轉角度</param>
        public static void AddRotatedText(Stream stream, string text, string saveFullFilename, float fontSize, string fontName = "mingliu", float angle = 45f)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(stream))
            {
                // 設置字體
                Font font = SystemFonts.CreateFont(fontName, fontSize);

                // 設置文字顏色和透明度
                Color color = Color.FromRgba(232, 23, 123, 128);

               
                Point point = new Point(image.Width / 2, image.Height / 2);
                var option = new DrawingOptions();
                option.Transform = Matrix3x2Extensions.CreateRotationDegrees(angle, new Point(10, 100));

                // 在圖片上繪製斜向文字
                image.Mutate(x =>
                {
                    x.DrawText(option, text, font, color, point);
                });

                // 將圖片保存到指定路徑
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(saveFullFilename));
                image.Save(saveFullFilename);
            }
        }

        public static void AddRotatedText(Image<Rgba32> image, string text, string saveFullFilename)
        {
            // 設置字體
            Font font = new Font(SystemFonts.CreateFont("Arial", 16), 16);

            // 設置文字顏色和透明度
            Color color = Color.FromRgba(232, 23, 123, 128);

            // 設置文字旋轉角度和位置
            float angle = 45f;
            Point point = new Point(image.Width / 2, image.Height / 2);
            var option = new DrawingOptions();
            option.Transform = Matrix3x2Extensions.CreateRotationDegrees(angle, new PointF(150, 150));

            // 在圖片上繪製斜向文字
            image.Mutate(x =>
            {
                x.DrawText(option, text, font, color, point);
            });

            // 將圖片保存到指定路徑
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(saveFullFilename));
            image.Save(saveFullFilename);
        }

        public static void AddRotatedText(string imagePath, string text, string saveFullFilename)
        {
            // 載入圖片
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                AddRotatedText(image, text, saveFullFilename);
            }
        }

    }
}
