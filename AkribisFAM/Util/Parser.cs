using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Util
{
    using System;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using AAMotion;
    using AkribisFAM.CommunicationProtocol;

    public class Parser
    {
        /// <summary>
        /// 解析形如 =+0000.36+9999.99 的字符串，返回第 index 个值（1 或 2）。
        /// </summary>
        public static double TryParseTwoValues(string input, int index = 1,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            void Log(string reason)
            {
                Console.WriteLine(
                    $"[Parser.{memberName}] (Line {lineNumber}) 错误：{reason}\n" +
                    $"→ 原始输入: \"{input}\"\n" +
                    $"→ 期望格式: =+0000.36+9999.99 或 =-123.45-678.90\n" +
                    $"→ 来源文件: {filePath}\n"
                );
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Log("输入为空或仅包含空白字符。");
                return double.NaN;
            }

            if (!input.StartsWith("="))
            {
                Log("输入不以 '=' 开头。");
                return double.NaN;
            }

            if (input.Length < 3)
            {
                Log("输入过短，无法包含两个合法值。");
                return double.NaN;
            }

            try
            {
                string trimmed = input.Substring(1); // 去掉 '='

                if (trimmed[0] != '+' && trimmed[0] != '-')
                {
                    Log("第一个数缺少正负号。");
                    return double.NaN;
                }

                int secondSignIndex = -1;
                for (int i = 1; i < trimmed.Length; i++)
                {
                    if (trimmed[i] == '+' || trimmed[i] == '-')
                    {
                        secondSignIndex = i;
                        break;
                    }
                }

                if (secondSignIndex == -1)
                {
                    Log("未找到第二个数的正负号，缺少分隔符。");
                    return double.NaN;
                }

                string part1 = trimmed.Substring(0, secondSignIndex);
                string part2 = trimmed.Substring(secondSignIndex);

                if (index == 1)
                    return double.Parse(part1, CultureInfo.InvariantCulture);
                else if (index == 2)
                    return double.Parse(part2, CultureInfo.InvariantCulture);
                else
                {
                    Log("索引非法，仅支持 1 或 2。");
                    return double.NaN;
                }
            }
            catch (FormatException fe)
            {
                Log($"格式解析错误: {fe.Message}");
                return double.NaN;
            }
            catch (Exception ex)
            {
                Log($"未知错误: {ex.Message}");
                return double.NaN;
            }
        }

        

    public static Dictionary<string, object> ToPropertyDictionary(object obj)
        {
            return obj.GetType()
                      .GetProperties()
                      .ToDictionary(p => p.Name, p => p.GetValue(obj));
        }
        public class PropertyDisplayItem
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }
        public static List<PropertyDisplayItem> FlattenObject(object obj)
        {
            var result = new List<PropertyDisplayItem>();

            if (obj == null) return result;

            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);

                if (value is IEnumerable<object> list && !(value is string))
                {
                    result.Add(new PropertyDisplayItem
                    {
                        Name = prop.Name,
                        Value = list.ToList()
                    });
                }
                else
                {
                    result.Add(new PropertyDisplayItem
                    {
                        Name = prop.Name,
                        Value = value
                    });
                }
            }

            return result;
        }


    }
}
