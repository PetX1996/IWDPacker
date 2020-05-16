using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IWDPacker
{
    class MaterialAsset
    {
        public string FilePath { get; private set; }

        public string ColorMap { get; private set; }
        public string NormalMap { get; private set; }
        public string SpecularMap { get; private set; }

        public MaterialAsset(string filePath)
        { 
            FilePath = filePath;
            ReadFile();
        }

        private void ReadFile()
        {
            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Delete))
            {
                using (BinaryReader bin = new BinaryReader(fs, ASCIIEncoding.ASCII))
                {
                    bin.ReadByte(); // ???
                    bin.ReadByte(); // ???
                    bin.ReadByte(); // ???
                    bin.ReadByte(); // ???
                    int imagesOffset = bin.ReadByte();

                    int offset = 5;
                    while (offset < imagesOffset)
                    {
                        if (bin.Read() == -1)
                            throw new InvalidOperationException("Error during reading material '" + FilePath + "', unexpected end of file");

                        offset++;
                    }

                    string type;
                    string image;
                    image = ReadString(bin);
                    if (bin.PeekChar() != -1)
                    {
                        type = ReadString(bin);
                        TryParseImage(type, image);
                    }
                    else
                    {
                        Console.WriteLine("Could not read material '" + FilePath + "'");
                        return;
                    }

                    while (bin.PeekChar() != -1)
                    {
                        type = ReadString(bin);
                        if (bin.PeekChar() != -1)
                        {
                            image = ReadString(bin);
                            TryParseImage(type, image);
                        }
                    }
                }
            }            
        }

        private void TryParseImage(string type, string image)
        {
            if (type == "colorMap")
                ColorMap = image;
            else if (type == "normalMap")
                NormalMap = image;
            else if (type == "specularMap")
                SpecularMap = image;
        }

        private string ReadString(BinaryReader bin)
        {
            StringBuilder sb = new StringBuilder();
            int b = bin.Read();
            while (b != -1)
            {
                if (b == 0) // end of string
                    return sb.ToString();

                sb.Append((char)b);

                b = bin.Read();
            }
            throw new InvalidOperationException("Error during reading material '" + FilePath + "', found non-terminated string");
        }
    }
}
