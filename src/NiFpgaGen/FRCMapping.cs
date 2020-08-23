using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiFpgaGen
{
    public class FRCRegister
    {
        public Dictionary<string, Register> RegisterMap { get; } = new Dictionary<string, Register>();
        public List<Register> GlobalsOfRegister { get; } = new List<Register>();

        public FRCRegister()
        {
        }

        public void Add(Register reg, int value)
        {
            if (RegisterMap.TryGetValue(reg.Name, out var rList))
            {
                rList.OffsetList.Add((value, reg.Offset));
            }
            else
            {
                reg.OffsetList.Add((value, reg.Offset));
                RegisterMap.Add(reg.Name, reg);
            }
        }

        public void AddGlobal(Register reg)
        {
            GlobalsOfRegister.Add(reg);
        }
    }

    public class FRCMapping
    {
        public Dictionary<string, FRCRegister> ClassRegisters { get; } = new Dictionary<string, FRCRegister>();

        public FRCMapping(IEnumerable<Register> Registers)
        {
            ClassRegisters.Add("Global", new FRCRegister());
            foreach (var register in Registers)
            {
                if (!register.Name.Contains("."))
                {
                    ClassRegisters["Global"].AddGlobal(register);
                }
                else
                {
                    var name = register.Name;
                    var split = name.Split('.', 2);
                    register.Name = split[1];
                    if (char.IsDigit(split[0][^1]))
                    {
                        string className = split[0][0..^1];
                        int idx = split[0][^1] - '0';
                        if (ClassRegisters.TryGetValue(className, out var value))
                        {
                            value.Add(register, idx);
                        }
                        else
                        {
                            var frcReg = new FRCRegister();
                            frcReg.Add(register, idx);
                            ClassRegisters.Add(className, frcReg);
                        }
                    }
                    else if (char.IsDigit(split[1][^1]))
                    {
                        string className = split[0];
                        int idx = split[1][^1] - '0';
                        register.Name = split[1][0..^1];
                        if (ClassRegisters.TryGetValue(className, out var value))
                        {
                            value.Add(register, idx);
                        }
                        else
                        {
                            var frcReg = new FRCRegister();
                            frcReg.Add(register, idx);
                            ClassRegisters.Add(className, frcReg);
                        }
                    }
                    else
                    {
                        string className = split[0];
                        if (ClassRegisters.TryGetValue(className, out var value))
                        {
                            value.AddGlobal(register);
                        }
                        else
                        {
                            var frcReg = new FRCRegister();
                            frcReg.AddGlobal(register);
                            ClassRegisters.Add(className, frcReg);
                        }
                    }
                }
            }
            ;
        }

        public void GenerateC(string directory, string prefix)
        {
            Directory.CreateDirectory(directory);

            FRCCGenerator generator = new FRCCGenerator(this, directory, prefix, "FRC_FPGATypes.h", "FRC_FPGA_STATUS");
            foreach (var clazz in ClassRegisters)
            {
                generator.GenerateClass(clazz.Key, clazz.Value);
            }
        }
    }
}
