using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace LbaTool
{
    public interface ILocator : IXmlSerializable
    {
        Vector4 Translation { get; set; }
        Vector4 Rotation { get; set; }

        bool HasFooter { get; }

        void Read(BinaryReader reader, Dictionary<uint, string> hashLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);

        void ReadFooter(BinaryReader reader, Dictionary<uint, string> hashLookupTable, Dictionary<uint, string> datasetLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void WriteFooter(BinaryWriter writer);
    }
}
