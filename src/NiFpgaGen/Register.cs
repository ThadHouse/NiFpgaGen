using NiFpgaGen.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen
{
    public class Register
    {
        public string Name { get; }
        public IDataType DataType { get; }
        public bool Hidden { get; }
        public bool Indicator { get; }
        public uint Offset { get; }

        public Register(XElement element)
        {
            Name = element.Element("Name").Value;
            DataType = DataTypeFactory.Instance.CreateFromRegister(element);
            Hidden = bool.Parse(element.Element("Hidden").Value);
            Indicator = bool.Parse(element.Element("Indicator").Value);
            Offset = uint.Parse(element.Element("Offset").Value);
        }

        private Register(Register oldRegister, string newName)
        {
            Name = newName;
            DataType = oldRegister.DataType;
            Hidden = oldRegister.Hidden;
            Indicator = oldRegister.Indicator;
            Offset = oldRegister.Offset;
        }

        public Register CloneWithName(string name)
        {
            return new Register(this, name);
        }
    }
}
