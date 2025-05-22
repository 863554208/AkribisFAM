using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.CompilerServices;

namespace AkribisFAM.Helper
{
    public static class FileHelper
    {
        /// <summary>
        /// 从 JSON 文件加载配置对象
        /// </summary>
        public static bool LoadConfig<T>(string jsonFile, out T config) where T : class
        {
            config = null;

            if (string.IsNullOrEmpty(jsonFile) || !File.Exists(jsonFile))
            {
                return false;
            }

            try
            {
                string content = File.ReadAllText(jsonFile);
                config = JsonConvert.DeserializeObject<T>(content);
                if (config == null)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 将配置对象保存为 JSON 文件
        /// </summary>
        public static bool SaveToJson<T>(string jsonFile, T config) where T : class
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                string content = JsonConvert.SerializeObject(config, Formatting.Indented, settings);
                File.WriteAllText(jsonFile, content);

                return File.Exists(jsonFile);
            }
            catch
            {
                return false;
            }
        }
    }
}
