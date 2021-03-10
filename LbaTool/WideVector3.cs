using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LbaTool
{
    public class WideVector3 : IXmlSerializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public ushort A { get; set; }
        public ushort B { get; set; }

        public virtual void Read(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            A = reader.ReadUInt16();
            B = reader.ReadUInt16();
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(A);
            writer.Write(B);
        }

        public virtual void ReadXml(XmlReader reader)
        {
            X = Extensions.ParseFloatRoundtrip(reader["x"]);
            Y = Extensions.ParseFloatRoundtrip(reader["y"]);
            Z = Extensions.ParseFloatRoundtrip(reader["z"]);
            A = ushort.Parse(reader["a"]);
            B = ushort.Parse(reader["b"]);
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("x", X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("z", Z.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("a", A.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("b", B.ToString(CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return string.Format("X: {1}, Y: {2}, Z: {3}, A: {4}, B: {5}", X, Y, Z, A, B);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
