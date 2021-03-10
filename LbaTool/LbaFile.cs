using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LbaTool
{
    public enum LocatorType
    {
        Type0 = 0,
        Type2 = 2,
        Type3 = 3
    }
    
    public class LbaFile : IXmlSerializable
    {
        public LocatorType Type;
        public List<ILocator> Locators = new List<ILocator>();

        /// <summary>
        /// Reads and populates data from a binary lba file.
        /// </summary>
        public void Read(BinaryReader reader, HashManager hashManager)
        {
            // Read header
            uint locatorCount = reader.ReadUInt32();
            Type = (LocatorType) reader.ReadUInt32();
            reader.ReadDouble();

            // Read locators
            bool hasFooter = false;
            for (int i = 0; i < locatorCount; i++)
            {
                switch (Type)
                {
                    case LocatorType.Type0:
                        ILocator locator0 = new LocatorType0();
                        locator0.Read(reader, hashManager.StrCode32LookupTable, hashManager.OnHashIdentified);
                        Locators.Add(locator0);
                        hasFooter = locator0.HasFooter;
                        break;
                    case LocatorType.Type2:
                        ILocator locator2 = new LocatorType2();
                        locator2.Read(reader, hashManager.StrCode32LookupTable, hashManager.OnHashIdentified); ;
                        Locators.Add(locator2);
                        hasFooter = locator2.HasFooter;
                        break;
                    case LocatorType.Type3:
                        ILocator locator3 = new LocatorType3();
                        locator3.Read(reader, hashManager.StrCode32LookupTable, hashManager.OnHashIdentified);
                        Locators.Add(locator3);
                        hasFooter = locator3.HasFooter;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Read footer
            if (hasFooter)
            {
                foreach (ILocator locator in Locators)
                {
                    locator.ReadFooter(reader, hashManager.StrCode32LookupTable, hashManager.PathCode32LookupTable, hashManager.OnHashIdentified);
                }
            }
            
        }

        /// <summary>
        /// Writes data to a binary lba file.
        /// </summary>
        public void Write(BinaryWriter writer)
        {
            // Write header
            writer.Write(Locators.Count);
            writer.Write((uint)Type);
            writer.Write(0);
            writer.Write(0);

            // Write locators
            foreach (var locator in Locators)
            {
                locator.Write(writer);
            }

            // Write footer
            if (Locators.Count > 0 && Locators[0].HasFooter)
            {
                foreach (var locator in Locators)
                {
                    locator.WriteFooter(writer);
                }
            }
        }

        /// <summary>
        /// Reads and populates data from an xml file.
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            reader.Read();
            Type = (LocatorType) int.Parse(reader["type"]);

            reader.ReadStartElement("lba");
            while (2 > 1)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        ILocator newLocator = CreateLocator();
                        newLocator.ReadXml(reader);
                        Locators.Add(newLocator);
                        reader.ReadEndElement();
                        continue;
                    case XmlNodeType.EndElement:
                        return;
                }
            }
        }

        /// <summary>
        /// Writes data to an xml file.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("lba");
            writer.WriteAttributeString("type", ((int)Type).ToString());

            foreach (ILocator locator in Locators)
            {
                writer.WriteStartElement("locator");
                locator.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();
        }
        
        ILocator CreateLocator()
        {
            switch (Type)
            {
                case LocatorType.Type0:
                    return new LocatorType0();
                case LocatorType.Type2:
                    return new LocatorType2();
                case LocatorType.Type3:
                    return new LocatorType3();
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new ArgumentOutOfRangeException();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
