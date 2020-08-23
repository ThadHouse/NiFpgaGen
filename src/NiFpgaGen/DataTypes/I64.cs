using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class I64 : IDataType
    {
        public string Name { get; }
        public int SizeInBits => 64;
        public string CDataType => "int64_t";

        public I64(string name, XElement element)
        {
            Name = name;
        }
    }
}
