using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class Array : IDataType
    {
        public string Name { get; }
        public int SizeInBits { get; }
        public string CDataType
        {
            get
            {
                return NormalizeName();
            }
        }
        public IDataType ArrayType { get; }
        public int NumberOfElements { get; }

        public Array(string name, XElement element)
        {
            Name = name;
            NumberOfElements = int.Parse(element.Element("Size").Value);
            ArrayType = DataTypeFactory.Instance.CreateDataType((XElement)element.Element("Type").FirstNode);
        }

        public string NormalizeName()
        {
            return Name;
        }
    }
}
