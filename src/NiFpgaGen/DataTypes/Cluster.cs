using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class Cluster : IDataType
    {
        public string Name { get; }
        public int SizeInBits { get; }
        public string CDataType { get; }
        public IReadOnlyList<IDataType> Types { get; }

        public Cluster(string name, XElement element)
        {
            Name = name;
            var typeList = element.Element("TypeList").Elements();
            var localTypes = new List<IDataType>();
            Types = localTypes;
            foreach (XElement type in typeList)
            {
                localTypes.Add(DataTypeFactory.Instance.CreateDataType(type));
            }
        }
    }
}
