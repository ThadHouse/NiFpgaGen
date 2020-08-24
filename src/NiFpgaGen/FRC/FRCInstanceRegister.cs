using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiFpgaGen.FRC
{
    public class FRCInstanceRegister
    {
        public string Name { get; }
        public Register BaseRegister { get; }
        public IEnumerable<(int index, uint offset)> Offsets => m_offsets.Select(x => (x.Key, x.Value));

        public int OffsetCount => m_offsets.Count;

        private Dictionary<int, uint> m_offsets = new Dictionary<int, uint>();

        public FRCInstanceRegister(string name, Register baseRegister, int index)
        {
            Name = name;
            BaseRegister = baseRegister;
            m_offsets.Add(index, baseRegister.Offset);
        }

        public void AppendOffset(Register register, int index)
        {
            m_offsets.Add(index, register.Offset);
        }
    }
}
