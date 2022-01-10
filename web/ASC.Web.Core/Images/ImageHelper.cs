using System;

using ASC.Core.Common.Settings;
using ASC.Web.Core.Users;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ASC.Web.Core.Utility
{
    public static class ImageHelper
    {
        public static byte[] ParseImage(byte[] imageData, long maxFileSize, Size maxImageSize, out IImageFormat imgFormat, out int width, out int height)
        {
            if (imageData == null || imageData.Length <= 0) throw new Exception(); // TODO: add Exceptions
            if (maxFileSize != -1 && imageData.Length > maxFileSize) throw new Exception();

            try
            {
                using var img = Image.Load(imageData, out var format);
                imgFormat = format;
                width = img.Width;
                height = img.Height;
                var maxWidth = maxImageSize.Width;
                var maxHeight = maxImageSize.Height;

                if ((maxHeight != -1 && img.Height > maxHeight) || (maxWidth != -1 && img.Width > maxWidth))
                {
                    #region calulate height and width

                    if (width > maxWidth && height > maxHeight)
                    {

                        if (width > height)
                        {
                            height = (int)(height * (double)maxWidth / width + 0.5);
                            width = maxWidth;
                        }
                        else
                        {
                            width = (int)(width * (double)maxHeight / height + 0.5);
                            height = maxHeight;
                        }
                    }

                    if (width > maxWidth && height <= maxHeight)
                    {
                        height = (int)(height * (double)maxWidth / width + 0.5);
                        width = maxWidth;
                    }

                    if (width <= maxWidth && height > maxHeight)
                    {
                        width = (int)(width * (double)maxHeight / height + 0.5);
                        height = maxHeight;
                    }

                    var tmpW = width;
                    var tmpH = height;
                    #endregion

                    using Image destRound = img.Clone(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(tmpW, tmpH),
                        Mode = ResizeMode.Stretch
                    }));

                    imageData = CommonPhotoManager.SaveToBytes(destRound);
                }
                return imageData;
            }
            catch (OutOfMemoryException)
            {
                throw new ImageSizeLimitException();
            }
            catch (ArgumentException error)
            {
                throw new ASC.Web.Core.Users.UnknownImageFormatException(error);
            }
        }

        public static Image GetImage(Image mainImg, Size size, IImageSettings imageSettings)
        {

            var x = imageSettings.Point.X > 0 ? imageSettings.Point.X : 0;
            var y = imageSettings.Point.Y > 0 ? imageSettings.Point.Y : 0;
            var rect = new Rectangle(x,
                                     y,
                                     imageSettings.Size.Width,
                                     imageSettings.Size.Height);

            Image destRound = mainImg.Clone(x => x.Crop(rect).Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Stretch
            }));
            return destRound;
        }

        public static void CheckImgFormat(byte[] data)
        {
            IImageFormat imgFormat;
            try
            {
                using var img = Image.Load(data, out var format);
                imgFormat = format;
            }
            catch (OutOfMemoryException)
            {
                throw new ImageSizeLimitException();
            }
            catch (ArgumentException error)
            {
                throw new Web.Core.Users.UnknownImageFormatException(error);
            }

            if (imgFormat.Name != "PNG" && imgFormat.Name != "JPEG")
            {
                throw new Web.Core.Users.UnknownImageFormatException();
            }
        }
    }
}
