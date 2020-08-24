using NiFpgaGen.DataTypes;
using NiFpgaGen.FRC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen
{
    //public enum DataType
    //{
    //    Array,
    //    Boolean,
    //    Cluster,
    //    FXP,
    //    EnumU16,
    //    U8,
    //    I8,
    //    I16,
    //    U16,
    //    I32,
    //    U32,
    //    U64,
    //    I64
    //}

    //public class Register
    //{
    //    public string Name { get; set; }
    //    public bool Hidden { get; }
    //    public bool Indicator { get; }
    //    public DataType DataType { get; }
    //    public uint Offset { get; }
    //    public XElement ExtraData { get; }

    //    public List<(int, uint)> OffsetList = new List<(int, uint)>();

    //    public Register(XElement element)
    //    {
    //        Name = element.Element("Name").Value;
    //        Hidden = bool.Parse(element.Element("Hidden").Value);
    //        Indicator = bool.Parse(element.Element("Indicator").Value);
    //        DataType = Enum.Parse<DataType>(((XElement)element.Element("Datatype").FirstNode).Name.LocalName);
    //        Offset = uint.Parse(element.Element("Offset").Value);
    //        ExtraData = element;
    //        ;
    //    }
    //}

    class Program
    {
        static async Task Main(string[] args)
        {
            string fileName = @"C:\Users\thadh\Documents\GitHub\thadhouse\NiFpgaGen\roboRIO_FPGA_2020_20.1.2.lvbitx";

            using var file = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            XElement bitfile = await XElement.LoadAsync(file, LoadOptions.None, CancellationToken.None);

            //var registerList = bitfile.Descendants("VI").Descendants("RegisterList").Descendants("Register").Select(x => new Register(x));

            var registerList2 = bitfile.Element("VI").Element("RegisterList").Elements("Register");

            DataTypeFactory factory = DataTypeFactory.Instance;

            List<Register> registers = new List<Register>();

            foreach (var register in registerList2)
            {
                registers.Add(new Register(register));
            }

            var frcRegisters = FRCClass.GetDefaultClassList();
            foreach (var register in registers)
            {
                FRCClass.AddRegisterToClassList(frcRegisters, register);
            }

            FRCClass.ValidateClasses(frcRegisters);

            //var frcMapping = new FRCMapping(registerList);

            //frcMapping.GenerateC(@"C:\Users\thadh\Documents\GitHub\thadhouse\NiFpgaGen\Generated", "FRC");

            ;
        }
    }
}
