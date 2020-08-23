using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class FXP : IDataType
    {
        public string Name { get; }
        public int SizeInBits { get; }
        public bool Signed { get; }
        public bool IncludeOverflowStatus { get; }
        public string CDataType => "uint32_t";

        public FXP(string name, XElement element)
        {
            Name = name;
            Signed = bool.Parse(element.Element("Signed").Value);
            SizeInBits = int.Parse(element.Element("WordLength").Value);
            IncludeOverflowStatus = bool.Parse(element.Element("IncludeOverflowStatus").Value);
        }
    }
}
