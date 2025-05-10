using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AkribisFAM.CommunicationProtocol
{
    public static class StrClass1
    {
        #region 数据发送组包
        /// <summary>
        /// 数据发送组包
        /// </summary>
        /// <param name="senddatatop">指令头部对象</param>
        /// <param name="camreapositionslist"> 拍照位置对象的集合</param>
        /// <param name="sendcommanddata"> 返回组包的字符串</param>
        /// <returns></returns>
        public static string BuildPacket(object senddatatop, List<object> camreapositionslist)
        {
            try
            {
                var sendcommand = new StringBuilder();
                Type typetop = senddatatop.GetType();

                // 遍历senddatatop对象类型的字段
                foreach (FieldInfo field in typetop.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    object fieldValue = field.GetValue(senddatatop);
                    sendcommand.Append(fieldValue).Append(",");
                    //Console.WriteLine($"字段名: {field.Name}, 值: {fieldValue}");
                }

                // 遍历camreapositionslist集合
                foreach (var camreapositions in camreapositionslist)
                {
                    Type typedata = camreapositions.GetType();
                    // 遍历camreapositionslist集合中各对象的字段
                    foreach (FieldInfo field in typedata.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        object fieldValue = field.GetValue(camreapositions);
                        sendcommand.Append(fieldValue).Append(",");
                        //Console.WriteLine($"字段名: {field.Name}, 值: {fieldValue}");
                    }
                }
                if (sendcommand.Length > 0)
                {
                    sendcommand.Length--; // 删除最后一个逗号
                }
                //sendcommand.Append("\r\n");
                string sendcommanddata = sendcommand.ToString();
                return sendcommanddata;
            }
            catch (Exception ex)
            {
                string sendcommanddata = "组包失败:" + ex.ToString();
                return null;
            }
        }
        #endregion

        #region 解析数据包 
        /// <summary>
        /// 使用正则表达式解析数据包
        /// </summary>
        /// <param name="readdatatop">需要匹配的头部指令</param>
        /// <param name="acceptpacket">接受包的字符串</param>
        /// <param name="readcommandtop">返回正则表达式匹配的命令头部</param>
        /// <param name="list_readdata">返回列表对象数据</param>
        /// <param name="List_Type">列表对象的类型</param>
        /// <returns></returns>
        public static bool TryParsePacket(string readdatatop, string acceptpacket,  object readcommandtop,List<object> list_readdata, Type List_Type)
        {
            try
            {
                //readcommandtop = null;
                //list_readdata = null;
               //list_readdata.Clear();
                string pattern = $"^{readdatatop}";// 正则匹配命令与数据
                Match match = Regex.Match(acceptpacket, pattern);
                if (!match.Success)
                {
                    readcommandtop = null;
                    list_readdata = null;
                    return false; // 匹配失败返回 false
                }

                //将字符串转化为对象信息
                string str_readcommandtop = match.Groups[0].Value; // 提取命令
                List<string> liststr_readcommandtop = str_readcommandtop.Split(',').ToList();
                if (!Fieldassignment(readcommandtop, liststr_readcommandtop))
                {
                    readcommandtop = null;
                    list_readdata = null;
                    return false;//为对象类型字段赋值失败
                }

                //将字符串转化为对应位置的列表信息
                string str_readdata = acceptpacket.Replace(str_readcommandtop, "");// 提取数据
                List<string> liststr_readdata = str_readdata.Split(',').ToList();//将字符串转化为列表                   
                if (list_readdata != null && list_readdata.Count > 0)// 列表中有对象
                {
                    for (int i = 0; i < list_readdata.Count; ++i)//循环列表中的对象,为字段赋值
                    {

                        if (liststr_readdata == null || liststr_readdata.Count == 0)
                        {
                            break;
                        }

                        if (!Fieldassignment(list_readdata[i], liststr_readdata))
                        {
                            readcommandtop = null;
                            list_readdata = null;
                            return false;//为对象类型字段赋值失败
                        }
                    }

                    while (true)//当列表中的对象字段容纳不完字符串中的内容时,再创建对象字段，直到容纳
                    {
                        if (liststr_readdata == null || liststr_readdata.Count == 0)
                        {
                            break;
                        }
                        object list_obj = Activator.CreateInstance(List_Type);
                        if (!Fieldassignment(list_obj, liststr_readdata))
                        {
                            readcommandtop = null;
                            list_readdata = null;
                            return false;//为对象类型字段赋值失败
                        }
                        list_readdata.Add(list_obj);//添加对象
                    }
                    return true; //列表对象的字段赋值成功
                }
                else
                {
                    while (true)//当列表中无对象时字段容纳不完字符串中的内容时,再创建对象字段，直到容纳
                    {
                        if (liststr_readdata == null || liststr_readdata.Count == 0)
                        {
                            break;
                        }
                        object list_obj = Activator.CreateInstance(List_Type);
                        if (!Fieldassignment(list_obj, liststr_readdata))
                        {
                            readcommandtop = null;
                            list_readdata = null;
                            return false;//为对象类型字段赋值失败
                        }
                        list_readdata.Add(list_obj);//添加对象
                    }
                    List<object> list_readdata2=list_readdata;
                    return true; //列表对象的字段赋值成功
                }              
            }
            catch (Exception ex)
            {
                //readcommandtop = "解包失败:" + ex.ToString();
                //readdata = "解包失败:" + ex.ToString();
                readcommandtop = null;
                list_readdata = null;
                return false;
            }

        }
        #endregion

        #region 
        /// <summary>
        /// 为对象类型的各字段赋值
        /// </summary>
        /// <param name="objectassignment">传入的对象</param>
        /// <param name="liststr">字符串列表</param>
        /// <returns></returns>
        public static bool Fieldassignment(object objectassignment, List<string> liststr)
        {
            try
            {
                Type type_onereaddata = objectassignment.GetType();//获取对象类型
                FieldInfo[] fields_onereaddata = type_onereaddata.GetFields(BindingFlags.Public | BindingFlags.Instance);//获取对象类型的各字段
                if (fields_onereaddata.Length > 0 && liststr.Count > 0)//判断字段的长度和列表中是否有元素
                {
                    for (int j = 0; j < fields_onereaddata.Length; ++j)//循环字段的长度,为其赋值
                    {
                        var field = fields_onereaddata[j];
                        string strValue = "";
                        if (liststr != null)
                        {
                            if (liststr.Count != 0)
                            {
                                strValue = liststr[0];
                                liststr.RemoveAt(0);//移除第一个元素
                            }
                            
                        }
                        object convertedValue = Convert.ChangeType(strValue, field.FieldType);// 自动转换字段类型
                        field.SetValue(objectassignment, convertedValue); //为具体对象的字段赋值                                 
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

    }
}
