using SixLabors.ImageSharp;

namespace ASC.Core.Common.Settings
{
    public interface IImageSettings : ISettings
    {
        public Point Point { get; set; }
        public Size Size { get; set; }
        public bool IsDefault { get; set; }
    }
}
