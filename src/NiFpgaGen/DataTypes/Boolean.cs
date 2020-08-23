using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class Boolean : IDataType
    {
        public string Name { get; }
        public static string CBoolType { get; set; } = "bool";
        public int SizeInBits => 1;
        public string CDataType => CBoolType;

        public Boolean(string name, XElement element)
        {
            Name = name;
        }
    }
}
