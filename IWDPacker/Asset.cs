using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IWDPacker
{
    class Asset
    {
        public string ShortPath { get; private set; }
        public string FullPath { get; private set; }

        public Asset(string shortPath, string fullPath)
        {
            ShortPath = shortPath;
            FullPath = fullPath;
        }

        public Asset(string shortPath)
        {
            ShortPath = shortPath;
        }

        public override bool Equals(object obj)
        {
            Asset a = obj as Asset;
            if (a == null)
                return false;

            if (a.ShortPath == ShortPath && a.FullPath == FullPath)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Asset a, Asset b)
        {
            if (a == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Asset a, Asset b)
        {
            return a != b;
        }
    }

    class ImageAsset : Asset
    {
        public ImageAsset(string shortPath, string fullPath)
            : base(shortPath, fullPath)
        { 
        }

        public ImageAsset(string shortPath)
            : base(shortPath)
        {
        }
    }
}
