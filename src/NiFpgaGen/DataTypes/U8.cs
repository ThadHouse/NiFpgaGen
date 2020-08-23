using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class U8 : IDataType
    {
        public string Name { get; }
        public int SizeInBits => 8;
        public string CDataType => "uint8_t";

        public U8(string name, XElement element)
        {
            Name = name;
        }
    }
}
