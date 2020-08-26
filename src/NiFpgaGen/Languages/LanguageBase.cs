using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiFpgaGen.Languages
{
    public interface ILanguageBase
    {
        string? HeaderTemplate { get; }
        string? StructDeclarationTemplate { get; }
        string? GlobalGetFunctionDeclarationTemplate { get; }
        string? GlobalSetFunctionDeclarationTemplate { get; }
        string? InstanceGetFunctionDeclarationTemplate { get; }
        string? InstanceSetFunctionDeclarationTemplate { get; }
        string? SystemGetFunctionDeclarationTemplate { get; }
        string? SystemSetFunctionDeclarationTemplate { get; }
    }
}
