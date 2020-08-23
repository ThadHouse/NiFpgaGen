using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiFpgaGen.DataTypes
{
    public interface IDataType
    {
        string Name { get; }
        int SizeInBits { get; }
        string CDataType { get; }
    }
}
