using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiFpgaGen.FRC
{
    public class FRCClass
    {
        public string ClassName { get; }
        public IEnumerable<Register> GlobalRegisters => m_globalRegisters;
        public IEnumerable<FRCSystemRegister> SystemRegisters => m_systemRegisters.Select(x => x.Value);
        public IEnumerable<FRCInstanceRegister> InstanceRegisters => m_instanceRegisters.Select(x => x.Value);

        private readonly List<Register> m_globalRegisters = new List<Register>();
        private readonly Dictionary<string, FRCSystemRegister> m_systemRegisters = new Dictionary<string, FRCSystemRegister>();
        private readonly Dictionary<string, FRCInstanceRegister> m_instanceRegisters = new Dictionary<string, FRCInstanceRegister>();

        public int NumberOfSystems { get; set; }

        public FRCClass(string className)
        {
            ClassName = className;
        }

        public void AddGlobalRegister(Register register, string registerName)
        {
            m_globalRegisters.Add(register.CloneWithName(registerName));
        }

        public void AddSystemRegister(Register register, string registerName, int systemNumber)
        {
            if (!m_systemRegisters.TryGetValue(registerName, out var frcRegister))
            {
                frcRegister = new FRCSystemRegister(registerName, register, systemNumber);
                m_systemRegisters.Add(registerName, frcRegister);
                return;
            }
            frcRegister.AppendOffset(register, systemNumber);
        }

        public void AddInstanceRegister(Register register, string registerName, int instanceNumber)
        {
            if (!m_instanceRegisters.TryGetValue(registerName, out var frcRegister))
            {
                frcRegister = new FRCInstanceRegister(registerName, register, instanceNumber);
                m_instanceRegisters.Add(registerName, frcRegister);
                return;
            }
            frcRegister.AppendOffset(register, instanceNumber);
        }

        public static readonly string GlobalClassName = "Global";

        public static Dictionary<string, FRCClass> GetDefaultClassList()
        {
            return new Dictionary<string, FRCClass>()
            {
                [GlobalClassName] = new FRCClass(GlobalClassName)
            };
        }

        public static void AddRegisterToClassList(Dictionary<string, FRCClass> classList, Register register)
        {
            if (GetClassName(register, out var className, out var registerName, out var instanceNumber, out var isSystemRegister))
            {
                if (!classList.TryGetValue(className, out var frcClass))
                {
                    frcClass = new FRCClass(className);
                    classList.Add(className, frcClass);
                }


                if (instanceNumber.HasValue)
                {
                    if (isSystemRegister)
                    {
                        frcClass.AddSystemRegister(register, registerName, instanceNumber.Value);
                    }
                    {
                        frcClass.AddInstanceRegister(register, registerName, instanceNumber.Value);
                    }
                }
                else
                {
                    frcClass.AddGlobalRegister(register, registerName);
                }
            }
            else
            {
                classList[GlobalClassName].AddGlobalRegister(register, register.Name);
            }
        }

        public static void ValidateClasses(Dictionary<string, FRCClass> classList)
        {
            foreach (var frcClass in classList)
            {
                bool first = true;
                foreach (var systemReg in frcClass.Value.m_systemRegisters)
                {
                    if (first)
                    {
                        frcClass.Value.NumberOfSystems = systemReg.Value.OffsetCount;
                        first = false;
                    }
                    else
                    {
                        if (frcClass.Value.NumberOfSystems != systemReg.Value.OffsetCount)
                        {
                            throw new InvalidOperationException("Number of systems cannot be different between registers in a class");
                        }
                    }
                }
            }
        }

        public static bool GetClassName(Register register, [NotNullWhen(true)] out string? className, [NotNullWhen(true)] out string? registerName, out int? instanceNumber, out bool isSystemRegister)
        {
            if (register.Name.Contains("."))
            {
                var split = register.Name.Split(".", 2);
                bool instanceNumberOnSystem = char.IsDigit(split[0][^1]);
                bool instanceNumberOnRegister = char.IsDigit(split[1][^1]);
                isSystemRegister = false;
                if (instanceNumberOnRegister && instanceNumberOnSystem)
                {
                    throw new InvalidOperationException("Can't have instance number on both register and system");
                }
                if (instanceNumberOnSystem)
                {
                    instanceNumber = split[0][^1] - '0';
                    split[0] = split[0][0..^1];
                    isSystemRegister = true;
                }
                else if (instanceNumberOnRegister)
                {
                    instanceNumber = split[1][^1] - '0';
                    split[1] = split[1][0..^1];
                }
                else
                {
                    instanceNumber = null;
                }
                className = split[0];
                registerName = split[1];
                return true;
            }
            className = null;
            registerName = null;
            instanceNumber = null;
            isSystemRegister = false;
            return false;
        }
    }
}
