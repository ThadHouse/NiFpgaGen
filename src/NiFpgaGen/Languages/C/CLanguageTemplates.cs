using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NiFpgaGen.Languages.C
{
    public class CLanguageTemplates : ILanguageBase
    {
        public string HeaderTemplate { get; }

        public string StructDeclarationTemplate { get; }

        public string GlobalGetFunctionDeclarationTemplate { get; }

        public string GlobalSetFunctionDeclarationTemplate { get; }

        public string InstanceGetFunctionDeclarationTemplate { get; }

        public string InstanceSetFunctionDeclarationTemplate { get; }

        public string SystemGetFunctionDeclarationTemplate { get; }

        public string SystemSetFunctionDeclarationTemplate { get; }

        public CLanguageTemplates()
        {
            Assembly assembly = typeof(CLanguageTemplates).Assembly;
            var resources = typeof(CLanguageTemplates).Assembly.GetManifestResourceNames()
                .Where(x => x.Contains("Languages.C."))
                .ToArray();

            string LoadFromAssembly(string resourceName)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd().Replace("\r\n", "\n");
            }

            HeaderTemplate = resources.Where(x => x.EndsWith("Header.txt")).Select(x => LoadFromAssembly(x)).First();
            StructDeclarationTemplate = resources.Where(x => x.EndsWith("StructDeclaration.txt")).First();
            GlobalGetFunctionDeclarationTemplate = resources.Where(x => x.EndsWith("GlobalGetFunctionDeclaration.txt")).Select(x => LoadFromAssembly(x)).First();
            GlobalSetFunctionDeclarationTemplate = resources.Where(x => x.EndsWith("GlobalSetFunctionDeclaration.txt")).Select(x => LoadFromAssembly(x)).First();
            InstanceGetFunctionDeclarationTemplate = resources.Where(x => x.EndsWith("InstanceGetFunctionDeclaration.txt")).Select(x => LoadFromAssembly(x)).First();
            InstanceSetFunctionDeclarationTemplate = resources.Where(x => x.EndsWith("InstanceSetFunctionDeclaration.txt")).Select(x => LoadFromAssembly(x)).First();
            SystemGetFunctionDeclarationTemplate = resources.Where(x => x.EndsWith("SystemGetFunctionDeclaration.txt")).Select(x => LoadFromAssembly(x)).First();
            SystemSetFunctionDeclarationTemplate = resources.Where(x => x.EndsWith("SystemSetFunctionDeclaration.txt")).Select(x => LoadFromAssembly(x)).First();

            ;
        }
    }
}
