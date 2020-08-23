using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class U16 : IDataType
    {
        public string Name { get; }
        public int SizeInBits => 16;
        public string CDataType => "uint16_t";

        public U16(string name, XElement element)
        {
            Name = name;
        }
    }
}
