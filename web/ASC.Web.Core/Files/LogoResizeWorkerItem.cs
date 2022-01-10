using System;

using ASC.Common;
using ASC.Data.Storage;
using ASC.Web.Core.Images;

using SixLabors.ImageSharp;

namespace ASC.Web.Core.Files
{
    [Transient]
    public class LogoResizeWorkerItem<T> : ResizeWorkerItem
    {
        public T FolderId { get; }
        public RoomLogoSettings Settings { get; }

        public LogoResizeWorkerItem() { }

        public LogoResizeWorkerItem(T folderId, byte[] data, long maxFileSize, Size size, IDataStore dataStore, RoomLogoSettings settings)
            : base(data, maxFileSize, size, dataStore)
        {
            FolderId = folderId;
            Settings = settings;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(LogoResizeWorkerItem<T> other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.FolderId.Equals(FolderId) && other.MaxFileSize == MaxFileSize && other.Size.Equals(Size);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FolderId, MaxFileSize, Size);
        }
    }
}
