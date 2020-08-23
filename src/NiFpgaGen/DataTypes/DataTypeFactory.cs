using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen.DataTypes
{
    public class DataTypeFactory
    {
        public static DataTypeFactory Instance => new DataTypeFactory();

        private ImmutableDictionary<string, Func<string, XElement, IDataType>> dataTypeCreators;

        private DataTypeFactory()
        {
            dataTypeCreators = typeof(IDataType).Assembly.GetTypes()
                .Where(x => x.GetInterface(typeof(IDataType).FullName!) != null)
                .ToImmutableDictionary(x => x.Name, x =>
                {
                    var constructor = x.GetConstructor(new Type[] { typeof(string), typeof(XElement) });
                    if (constructor == null)
                    {
                        throw new InvalidOperationException("Must have a constructor taking an XElement");
                    }
                    Func<string, XElement, IDataType> constructionFunc = (name, elem) =>
                    {
                        return (IDataType)constructor.Invoke(new object[] { name, elem });
                    };
                    return constructionFunc;
                });
        }

        public IDataType CreateFromRegister(XElement element)
        {
            return CreateDataType((XElement)element.Element("Datatype").FirstNode);
        }

        public IDataType CreateDataType(XElement element)
        {
            string type = element.Name.LocalName;
            string name = element.Element("Name").Value;

            return dataTypeCreators[type](name, element);

        }
    }
}
