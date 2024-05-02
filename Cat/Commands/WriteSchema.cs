using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        internal static bool WriteSchema()
        {
            bool returned = false;
            string name = commandstruct.Value.Parameters[0][0].ToString(), entry = commandstruct.Value.Parameters[0][1].ToString();
            if (entry == null || name == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            string[] parts = new string (entry.Where(c => char.IsLetter(c) || c == ' ').ToArray()).Split(' ');
            List<(Helpers.BinaryFileHandler.Types, string) > segments = new();
            for (int i = 0; i < parts.Length - 1; i += 2)
            {
                Helpers.BinaryFileHandler.Types type = parts[i].ToLower() switch
                {
                    "sevenbitencodedint" => Helpers.BinaryFileHandler.Types.SevenBitEncodedInt,
                    "sevenbitencodedint64" => Helpers.BinaryFileHandler.Types.SevenBitEncodedInt64,
                    "boolean" => Helpers.BinaryFileHandler.Types.Boolean,
                    "bool" => Helpers.BinaryFileHandler.Types.Boolean,
                    "byte" => Helpers.BinaryFileHandler.Types.Byte,
                    "bytes" => Helpers.BinaryFileHandler.Types.Bytes,
                    "char" => Helpers.BinaryFileHandler.Types.Char,
                    "chars" => Helpers.BinaryFileHandler.Types.Chars,
                    "decimal" => Helpers.BinaryFileHandler.Types.Decimal,
                    "double" => Helpers.BinaryFileHandler.Types.Double,
                    "half" => Helpers.BinaryFileHandler.Types.Failed,
                    "int16" => Helpers.BinaryFileHandler.Types.Int16,
                    "short" => Helpers.BinaryFileHandler.Types.Int16,
                    "int32" => Helpers.BinaryFileHandler.Types.Int32,
                    "int" => Helpers.BinaryFileHandler.Types.Int32,
                    "int64" => Helpers.BinaryFileHandler.Types.Int64,
                    "long" => Helpers.BinaryFileHandler.Types.Int64,
                    "sbyte" => Helpers.BinaryFileHandler.Types.SByte,
                    "single" => Helpers.BinaryFileHandler.Types.Single,
                    "float" => Helpers.BinaryFileHandler.Types.Single,
                    "string" => Helpers.BinaryFileHandler.Types.String,
                    "uint16" => Helpers.BinaryFileHandler.Types.UInt16,
                    "ushort" => Helpers.BinaryFileHandler.Types.UInt16,
                    "uint32" => Helpers.BinaryFileHandler.Types.UInt32,
                    "uint" => Helpers.BinaryFileHandler.Types.UInt32,
                    "uint64" => Helpers.BinaryFileHandler.Types.UInt64,
                    "ulong" => Helpers.BinaryFileHandler.Types.UInt64,
                    "list" => Helpers.BinaryFileHandler.Types.List,
                    "array" => Helpers.BinaryFileHandler.Types.List,
                    _ => Helpers.BinaryFileHandler.Types.Failed,
                };
                if (type == Helpers.BinaryFileHandler.Types.Failed)
                {
                    Interface.AddLog($"Unrecognised type '{parts[i]}'.");
                    returned = true;
                }
                else
                    segments.Add((type, parts[i + 1]));
            }
            if (returned)
            {
                Interface.AddLog($"Expected types:\n - {string.Join("\n - ", Enum.GetNames(typeof(Helpers.BinaryFileHandler.Types)))}");
                return false;
            }
            using var bfh = new Helpers.BinaryFileHandler(SchemaFile, null);
            bool b = bfh.AddSchema(out int index, name, segments.ToArray());
            if (b)
            {
                Interface.AddLog($"Successfully added schema {name} (schema id {index})!");
                return true;
            }

            Interface.AddLog("Something went wrong.");
            return false; 
        }
    }
}
