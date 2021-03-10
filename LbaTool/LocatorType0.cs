using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace LbaTool
{
    public class LocatorType0 : ILocator
    {
        public Vector4 Translation { get; set; }
        public Vector4 Rotation { get; set; }

        public bool HasFooter => false;

        public void Read(BinaryReader reader, Dictionary<uint, string> hashLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Translation = new Vector4();
            Translation.Read(reader);

            Rotation = new Vector4();
            Rotation.Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            Translation.Write(writer);
            Rotation.Write(writer);
        }

        public void ReadFooter(BinaryReader reader, Dictionary<uint, string> hashLookupTable, Dictionary<uint, string> datasetLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            throw new InvalidOperationException("This locator format does not have a footer.");
        }

        public void WriteFooter(BinaryWriter writer)
        {
            throw new InvalidOperationException("This locator format does not have a footer.");
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("locator");

            Translation = new Vector4();
            Translation.ReadXml(reader);

            reader.Read();
            
            Rotation = new Vector4();
            Rotation.ReadXml(reader);

            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("translation");
            Translation.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("rotation");
            Rotation.WriteXml(writer);
            writer.WriteEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
