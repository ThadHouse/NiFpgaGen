using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class U64 : IDataType
    {
        public string Name { get; }
        public int SizeInBits => 64;
        public string CDataType => "uint64_t";

        public U64(string name, XElement element)
        {
            Name = name;
        }
    }
}
