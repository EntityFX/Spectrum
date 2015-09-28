using System;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Text;
using EntityFX.Core.Common;

namespace EntityFX.Spectrum.UI
{
    public static class MemoryDumpHelper
    {
        private static char[] _codePageMap = new[]
        {
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
            '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
            'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_',
            '£', 'a', 'b', 'v', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
            'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '{', 'I', '}', '~', '©',
            ' ', '▝', '▘', '▀', '▗', '▐', '▚', '▜', '▖', '▞', '▌', '▛', '▄', '▟', '▙', '█',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '
        };

        public static string DumpAsText(IMemory memory)
        {
            var dumpStringBuilder = new StringBuilder(299320);
            dumpStringBuilder.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F  0123456789ABCDEF");
            var ip = 0;
            while (ip < memory.Size)
            {
                int rowStartPointer = ip;
                dumpStringBuilder.Append(ip.ToString("X4"));
                dumpStringBuilder.Append("  ");
                for (int columnIndex = 0; columnIndex < 0x10; columnIndex++)
                {
                    dumpStringBuilder.Append(memory.Raw[rowStartPointer].ToString("X2"));
                    dumpStringBuilder.Append(' ');
                    rowStartPointer++;
                }
                dumpStringBuilder.Append(' ');
                rowStartPointer = ip;
                for (int columnIndex = 0; columnIndex < 0x10; columnIndex++)
                {
                    dumpStringBuilder.Append(_codePageMap[memory.Raw[rowStartPointer]]);
                    rowStartPointer++;
                }
                ip += 0x10;
                dumpStringBuilder.AppendLine();
            }

            return dumpStringBuilder.ToString();
        }
    }
}