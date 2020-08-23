using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class I32 : IDataType
    {
        public string Name { get; }
        public int SizeInBits => 32;
        public string CDataType => "int32_t";

        public I32(string name, XElement element)
        {
            Name = name;
        }
    }
}
