using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MBTP_Server
{
    public static class Extentions
    {
        public static string ToLiteral(this string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        public static string TrimNullBytes(this string input)
        {
            return input.Trim('\0');
        }

        public static string TrimEscapes(this string input)
        {
            string temp = string.Empty;
            temp = input.TrimEnd('\0');
            temp = temp.TrimEnd('\r');
            temp = temp.TrimEnd('\n');
            temp = temp.TrimEnd('\r');
            temp = temp.TrimEnd('\n');
            return temp;
        }
    }
}
