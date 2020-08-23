using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NiFpgaGen
{
    public class FRCCGenerator
    {
        public FRCMapping Mapping { get; }
        public string Directory { get; }
        public string Prefix { get; }
        public string TypeHeaderName { get; }
        public string StatusTypeName { get; }

        private SourceCodeStringBuilder headerBuilder = new SourceCodeStringBuilder(2);
        private SourceCodeStringBuilder srcBuilder = new SourceCodeStringBuilder(2);

        public Dictionary<DataType, string> TypeMap = new Dictionary<DataType, string>
        {
            [DataType.U8] = "uint8_t",
            [DataType.I8] = "int8_t",
            [DataType.U16] = "uint16_t",
            [DataType.I16] = "int16_t",
            [DataType.U32] = "uint32_t",
            [DataType.I32] = "int32_t",
            [DataType.U64] = "uint64_t",
            [DataType.I64] = "int64_t",
            [DataType.Boolean] = "FPGA_BOOL",
            [DataType.EnumU16] = "uint16_t"
        };

        public Dictionary<string, int> SizeMap = new Dictionary<string, int>()
        {
            ["U8"] = 8,
            ["I8"] = 8,
            ["U16"] = 16,
            ["I16"] = 16,
            ["U32"] = -2,
            ["I32"] = -2,
            ["U64"] = -2,
            ["I64"] = -2,
            ["Boolean"] = 1,
            ["FXP"] = -1,
            ["Cluster"] = -3,
        };

        public FRCCGenerator(FRCMapping mapping, string directory, string prefix, string typeHeaderName, string statusTypeName)
        {
            Mapping = mapping;
            Directory = directory;
            Prefix = prefix;
            TypeHeaderName = typeHeaderName;
            StatusTypeName = statusTypeName;
        }

        public void GenerateClass(string className, FRCRegister registers)
        {
            InitializeHeader(className);
            InititalizeSource(className);

            GenerateHeader(className, registers);
            GenerateSource(className, registers);

            FinalizeHeader(className);
            FinalizeSource(className);
        }

        private void InitializeHeader(string className)
        {
            headerBuilder.Clear();
            headerBuilder.AppendLine("#pragma once").AppendLine();
            headerBuilder.AppendLine($"#include \"{TypeHeaderName}\"").AppendLine();

            headerBuilder.AppendLine("#ifdef __cplusplus").AppendLine("extern \"C\" {").AppendLine("#endif").AppendLine();

        }

        private void FinalizeHeader(string className)
        {
            headerBuilder.AppendLine();
            headerBuilder.AppendLine("#ifdef __cplusplus").AppendLine("} /* extern \"C\" */").AppendLine("#endif");
            string path = Path.Combine(Directory, $"{Prefix}_{className}.h");
            File.WriteAllText(path, headerBuilder.ToString());
        }

        private void InititalizeSource(string className)
        {
            srcBuilder.Clear();
            srcBuilder.AppendLine($"#include \"{Prefix}_{className}.h\"").AppendLine();
        }

        private void FinalizeSource(string className)
        {
            string path = Path.Combine(Directory, $"{Prefix}_{className}.c");
            File.WriteAllText(path, srcBuilder.ToString());
        }

        private SourceCodeStringBuilder innerBuilder = new SourceCodeStringBuilder(2);

        private int ComputeSizeOfNestedCluster(XElement cluster)
        {
            int totalSize = 0;
            foreach (var element in cluster.Element("TypeList").Elements().Reverse())
            {
                int size = SizeMap[element.Name.LocalName];
                if (size == -2)
                {
                    // 32 or 64 bit full value
                    if (element.Name.LocalName == "U32" || element.Name.LocalName == "I32")
                    {
                        totalSize += 32;
                    }
                    else
                    {
                        totalSize += 64;
                    }
                    continue;
                }
                else if (size == -1)
                {
                    size = int.Parse(element.Element("WordLength").Value);
                }
                else if (size == -3)
                {
                    // Embedded cluster
                    totalSize += ComputeSizeOfNestedCluster(element);
                    continue;
                }
                totalSize += size;
            }
            return totalSize;
        }

        private int ComputeSizeOfCluster(XElement cluster)
        {
            int totalSize = 0;
            foreach (var element in cluster.Element("TypeList").Elements().Reverse())
            {
                int size = SizeMap[element.Name.LocalName];
                if (size == -2)
                {
                    // 32 or 64 bit full value
                    if (element.Name.LocalName == "U32" || element.Name.LocalName == "I32")
                    {
                        totalSize += 32;
                    }
                    else
                    {
                        totalSize += 64;
                    }
                    continue;
                }
                else if (size == -1)
                {
                    size = int.Parse(element.Element("WordLength").Value);
                }
                else if (size == -3)
                {
                    // Embedded cluster
                    totalSize += ComputeSizeOfNestedCluster(element);
                    continue;
                }
                totalSize += size;
            }
            if (totalSize < 32)
            {
                totalSize = 32;
            }
            return totalSize;
        }

        private int GenerateNestedCluster(XElement cluster)
        {
            string clusterName = cluster.Element("Name").Value;
            int totalSize = 0;
            foreach (var element in cluster.Element("TypeList").Elements().Reverse())
            {
                innerBuilder.AppendIndent();
                string name = $"{clusterName}_{element.Element("Name").Value}";
                int size = SizeMap[element.Name.LocalName];
                if (size == -2)
                {
                    // 32 or 64 bit full value
                    string typeVal = TypeMap[Enum.Parse<DataType>(element.Name.LocalName)];
                    innerBuilder.Append(typeVal).Append(" ").Append(name).AppendLine(";");
                    continue;
                }
                else if (size == -1)
                {
                    size = int.Parse(element.Element("WordLength").Value);
                }
                else if (size == -3)
                {
                    // Embedded cluster
                    totalSize += GenerateNestedCluster(element);
                    continue;
                }
                totalSize += size;
                innerBuilder.Append("uint32_t ").Append(name).Append(" : ").Append(size.ToString()).AppendLine(";");
            }
            return totalSize;
        }

        private string GenerateCluster(string className, XElement cluster, SourceCodeStringBuilder outerBuilder, int startOfLine)
        {
            innerBuilder.Clear();
            string structName = $"{Prefix}_{className}_{cluster.Element("Name").Value.Split(".")[^1]}";
            if (startOfLine < 0)
            {
                return structName;
            }
            innerBuilder.Append("typedef struct ").Append(structName).AppendLine(" {").AddIndent();
            int totalSize = 0;
            // Go backwards
            foreach (var element in cluster.Element("TypeList").Elements().Reverse())
            {
                string name = element.Element("Name").Value;
                int size = SizeMap[element.Name.LocalName];
                if (size == -2)
                {
                    // 32 or 64 bit full value
                    string typeVal = TypeMap[Enum.Parse<DataType>(element.Name.LocalName)];
                    innerBuilder.AppendIndent();
                    innerBuilder.Append(typeVal).Append(" ").Append(name).AppendLine(";");
                    continue;
                }
                else if (size == -1)
                {
                    size = int.Parse(element.Element("WordLength").Value);
                }
                else if (size == -3)
                {
                    // Embedded cluster
                    totalSize += GenerateNestedCluster(element);
                    continue;
                }
                totalSize += size;
                innerBuilder.AppendIndent();
                innerBuilder.Append("uint32_t ").Append(name).Append(" : ").Append(size.ToString()).AppendLine(";");
            }
            if (totalSize > 32)
            {
                throw new InvalidOperationException("Bitfields cannot be larger then 32 bits");
            }
            innerBuilder.RemoveIndent().Append("} ").Append(structName).AppendLine(";").AppendLine();
            outerBuilder.Insert(startOfLine, innerBuilder.ToString());
            return structName;
        }

        private void GenerateParameterImpl(string className, SourceCodeStringBuilder builder, DataType dataType, XElement dataNode, bool set, int startOfLine)
        {
            if (TypeMap.TryGetValue(dataType, out var typeName))
            {
                builder.Append(typeName);
            }
            else if (dataType == DataType.FXP)
            {
                bool includeOverflow = bool.Parse(dataNode.Element("IncludeOverflowStatus").Value);
                if (includeOverflow)
                {
                    if (set)
                    {
                        throw new InvalidOperationException("Cannot include overflow with a set");
                    }
                    builder.Append(TypeMap[DataType.Boolean]).Append("* did_overflow, ");
                }

                int wordLength = int.Parse(dataNode.Element("WordLength").Value);
                // Find nearest power of 2
                int powerVal = (int)Math.Pow(2, Math.Ceiling(Math.Log(wordLength) / Math.Log(2)));
                if (powerVal < 8)
                {
                    powerVal = 8;
                }
                bool signed = bool.Parse(dataNode.Element("Signed").Value);
                if (signed)
                {
                    builder.Append($"int{powerVal}_t");
                }
                else
                {
                    builder.Append($"uint{powerVal}_t");
                }
            }
            else if (dataType == DataType.Cluster)
            {
                string structName = GenerateCluster(className, dataNode, builder, startOfLine);
                builder.Append(structName);
                set = false; // Force not set to append pointer
            }
            else if (dataType == DataType.Array)
            {
                builder.Append("uint32_t index, ");
                dataNode = (XElement)dataNode.Element("Type").FirstNode;
                var localName = Enum.Parse<DataType>(dataNode.Name.LocalName);
                GenerateParameterImpl(className, builder, localName, dataNode, set, startOfLine);
                return;
            }

            if (!set)
            {
                builder.Append("*");
            }
        }

        private void GenerateParameter(string className, SourceCodeStringBuilder builder, Register register, bool set, int startOfLine)
        {
            var dataTypeNode = (XElement)register.ExtraData.Element("Datatype").FirstNode;
            GenerateParameterImpl(className, builder, register.DataType, dataTypeNode, set, startOfLine);
            builder.Append(" value");
        }

        private void GenerateHeaderGlobals(string className, IEnumerable<Register> registers)
        {
            foreach (var register in registers)
            {
                int startIndex = headerBuilder.Length;
                // Generate a set if not an indicator
                if (!register.Indicator)
                {
                    headerBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Set{register.Name}(");
                    GenerateParameter(className, headerBuilder, register, true, startIndex);
                    headerBuilder.AppendLine(");");
                }
                headerBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Get{register.Name}(");
                GenerateParameter(className, headerBuilder, register, false, startIndex);
                headerBuilder.AppendLine(");");
                headerBuilder.AppendLine();
            }
        }

        private void GenerateHeaderInstances(string className, IEnumerable<Register> registers)
        {
            foreach (var register in registers)
            {
                int startIndex = headerBuilder.Length;
                // Generate a set if not an indicator
                if (!register.Indicator)
                {
                    headerBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Set{register.Name}(uint8_t reg_index, ");
                    GenerateParameter(className, headerBuilder, register, true, startIndex);
                    headerBuilder.AppendLine(");");
                }
                headerBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Get{register.Name}(uint8_t reg_index, ");
                GenerateParameter(className, headerBuilder, register, false, startIndex);
                headerBuilder.AppendLine(");");
                headerBuilder.AppendLine();
            }
        }

        private void GenerateHeader(string className, FRCRegister registers)
        {
            GenerateHeaderGlobals(className, registers.GlobalsOfRegister);
            if (registers.RegisterMap.Count > 0)
            {
                GenerateHeaderInstances(className, registers.RegisterMap.Select(x => x.Value));
            }
        }

        

        private void GenerateSourceSetImplementation(SourceCodeStringBuilder builder, Register register, bool set)
        {
            string functionToCall = GetFunctionToCall(register, set, out var isSimple, out var sizeType);
            if (sizeType.HasValue)
            {
                builder.AppendIndent().AppendLine($"_Static_assert(sizeof({TypeMap[sizeType.Value]}) == sizeof(*value), \"Incorrect size generated for cluster\");");
            }
            if (isSimple)
            {
                builder.AppendIndent().Append("return ").Append(functionToCall).AppendLine($"({Prefix}_FPGASessionHandle, offset, value);");
            }
            else
            {

            }
        }

        private void GenerateSourceImplementationGlobal(SourceCodeStringBuilder builder, Register register, bool set)
        {
            // Generate offset, call straight into code
            builder.AppendIndent().Append("uint32_t offset = ").Append(register.Offset.ToString()).AppendLine(";");
            GenerateSourceSetImplementation(builder, register, set);
        }

        private void GenerateSourceImplementationInstance(SourceCodeStringBuilder builder, Register register, bool set)
        {
            builder.AppendIndent().AppendLine("uint32_t offset;");
            builder.AppendIndent().AppendLine("switch (reg_index) {").AddIndent();
            foreach (var item in register.OffsetList)
            {
                builder.AppendIndent().AppendLine($"case {item.Item1}: offset = {item.Item2}; break;");
            }
            builder.AppendIndent().AppendLine($"default: return {Prefix}_FPGA_Status_InvalidParameter;");
            builder.RemoveIndent().AppendIndent().AppendLine("}");
            
            GenerateSourceSetImplementation(builder, register, set);
        }

        private void GenerateSourceGlobals(string className, IEnumerable<Register> registers)
        {
            foreach (var register in registers)
            {
                int startIndex = -1;
                // Generate a set if not an indicator
                if (!register.Indicator)
                {
                    srcBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Set{register.Name}(");
                    GenerateParameter(className, srcBuilder, register, true, startIndex);
                    srcBuilder.AppendLine(") {").AddIndent();
                    GenerateSourceImplementationGlobal(srcBuilder, register, true);
                    srcBuilder.RemoveIndent().AppendLine("}").AppendLine();
                }
                srcBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Get{register.Name}(");
                GenerateParameter(className, srcBuilder, register, false, startIndex);
                srcBuilder.AppendLine(") {").AddIndent();
                GenerateSourceImplementationGlobal(srcBuilder, register, false);
                srcBuilder.RemoveIndent().AppendLine("}").AppendLine();
            }
        }

        private void GenerateSourceInstances(string className, Dictionary<string, Register> registers)
        {
            foreach (var register in registers)
            {
                int startIndex = -1;
                // Generate a set if not an indicator
                if (!register.Value.Indicator)
                {
                    srcBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Set{register.Value.Name}(uint8_t reg_index, ");
                    GenerateParameter(className, srcBuilder, register.Value, true, startIndex);
                    srcBuilder.AppendLine(") {").AddIndent();
                    GenerateSourceImplementationInstance(srcBuilder, register.Value, true);
                    srcBuilder.RemoveIndent().AppendLine("}").AppendLine();
                }
                srcBuilder.Append(StatusTypeName).Append($" {Prefix}_{className}_Get{register.Value.Name}(uint8_t reg_index, ");
                GenerateParameter(className, srcBuilder, register.Value, false, startIndex);
                srcBuilder.AppendLine(") {").AddIndent();
                GenerateSourceImplementationInstance(srcBuilder, register.Value, false);
                srcBuilder.RemoveIndent().AppendLine("}").AppendLine();
            }
        }

        private void GenerateSource(string className, FRCRegister registers)
        {
            GenerateSourceGlobals(className, registers.GlobalsOfRegister);
            if (registers.RegisterMap.Count > 0)
            {
                GenerateSourceInstances(className, registers.RegisterMap);
            }
        }

        private string GetFunctionToCall(Register register, bool set, out bool isSimple, out DataType? sizeType)
        {
            sizeType = null;
            string functionName;
            switch (register.DataType)
            {
                case DataType.U8:
                case DataType.I8:
                case DataType.I16:
                case DataType.U16:
                case DataType.I32:
                case DataType.U32:
                case DataType.U64:
                case DataType.I64:
                    functionName = register.DataType.ToString();
                    isSimple = true;
                    break;
                case DataType.Boolean:
                    functionName = "Bool";
                    isSimple = true;
                    break;
                case DataType.Cluster:
                    isSimple = true;
                    int length = ComputeSizeOfCluster((XElement)register.ExtraData.Element("Datatype").FirstNode);
                    // Find nearest power of 2
                    int powerVal = (int)Math.Pow(2, Math.Ceiling(Math.Log(length) / Math.Log(2)));
                    if (powerVal < 8)
                    {
                        powerVal = 8;
                    }
                    functionName = $"U{powerVal}";

                    if (Enum.TryParse<DataType>(functionName, out var sizeTypeLocal))
                    {
                        sizeType = sizeTypeLocal;
                    }
                    else
                    {
                        isSimple = false;
                        functionName = "unknown";
                    }
                    break;
                default:
                    functionName = "unknown";
                    isSimple = false;
                    break;
            }
            return $"{Prefix}_FPGA_{(set ? "Write" : "Read")}{functionName}";
        }
    }
}
