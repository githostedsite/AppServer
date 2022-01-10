using System;

using ASC.Core.Common.Settings;

using SixLabors.ImageSharp;

namespace ASC.Web.Core.Files
{
    [Serializable]
    public class RoomLogoSettings : IImageSettings
    {
        public Guid ID => new Guid("141e3483-30d6-4d7f-81b6-c45e8983e7ab");
        public Point Point { get; set; }
        public Size Size { get; set; }
        public bool IsDefault { get; set; }

        public RoomLogoSettings() { }

        public RoomLogoSettings(Size size) => Size = size;

        public RoomLogoSettings(Point point, Size size)
        {
            Point = point;
            Size = size;
        }

        public RoomLogoSettings(int x, int y, int width, int height)
        {
            Point = new Point(x, y);
            Size = new Size(width, height);
        }

        public ISettings GetDefault(IServiceProvider serviceProvider)
        {
            return new RoomLogoSettings()
            {
                Point = new Point(0, 0),
                Size = new Size(0, 0),
                IsDefault = true
            };
        }
    }
}
