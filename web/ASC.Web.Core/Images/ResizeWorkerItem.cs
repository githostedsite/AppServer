using System;

using ASC.Common.Threading;
using ASC.Data.Storage;

using SixLabors.ImageSharp;

namespace ASC.Web.Core.Images
{
    public class ResizeWorkerItem : DistributedTask
    {
        public ResizeWorkerItem() { }

        public ResizeWorkerItem(byte[] data, long maxFileSize, Size size, IDataStore dataStore) : base()
        {
            Data = data;
            MaxFileSize = maxFileSize;
            Size = size;
            DataStore = dataStore;
        }

        public Size Size { get; }

        public IDataStore DataStore { get; }

        public long MaxFileSize { get; }

        public byte[] Data { get; }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ResizeWorkerItem)) return false;
            return Equals((ResizeWorkerItem)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MaxFileSize, Size);
        }
    }
}
