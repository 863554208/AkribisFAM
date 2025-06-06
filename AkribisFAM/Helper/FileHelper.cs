using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.CompilerServices;
using AkribisFAM.Manager;

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



        /// <summary>
        /// Serialize and Save the properties in MaterialManager class into json file
        /// This saving backup the original file before it overwrite it to avoid file corruption
        /// </summary>
        /// <returns>True: Save success, False : Error or exception thrown</returns>
        public static bool Save<T>(T serializeObj) where T : class
        {
            var rVal = false;
            JsonSerializer serializer = null;
            try
            {
                var fn = serializeObj.GetType().Name;
                var fp = Path.Combine($"{fn}.json");
                var fp_temp = Path.Combine($"{fn}_temp.json");
                var fp_backup = Path.Combine($"{fn}_backup.json");
                serializer = new JsonSerializer() { Formatting = Formatting.Indented };

                using (StreamWriter fWriter = new StreamWriter(fp_temp, false))
                {
                    serializer.Serialize(fWriter, serializeObj);
                }

                if (File.Exists(fp))
                    File.Replace(fp_temp, fp, fp_backup);
                else
                    File.Move(fp_temp, fp);

                rVal = true;
            }
            catch (Exception ex)
            {
                rVal = false;
            }
            return rVal;
        }

        /// <summary>
        /// Deserialize and Load the properties in MaterialManager class into json file
        /// This saving backup the original file before it overwrite it to avoid file corruption
        /// </summary>
        /// <returns>True: Save success, False : Error or exception thrown</returns>
        public static bool Load<T>(out T deserializeObj) where T : class
        {
            deserializeObj = null;
            bool rVal = false;
            JsonSerializer serializer = null;
            FileStream fStream = null;
            TextReader fReader = null;
            JsonTextReader jread = null;
            try
            {
                var fn = typeof(T).Name;
                var fp = Path.Combine($"{fn}.json");
                var fp_temp = Path.Combine($"{fn}_temp.json");
                var fp_backup = Path.Combine($"{fn}_backup.json");


                FileRecovery.RecoverFile<MaterialManager>(fp, fp_temp, fp_backup);

                serializer = new JsonSerializer();
                fStream = new FileStream(fp, FileMode.Open);
                fReader = new StreamReader(fStream);
                jread = new JsonTextReader(fReader);
                string jsonString = fReader.ReadToEnd();

                deserializeObj = JsonConvert.DeserializeObject<T>(jsonString);
                rVal = true;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                fReader?.Close();
            }
            return rVal;
        }

        public static void CopyProperties<T>(T source, T destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");

            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(source);
                    prop.SetValue(destination, value);
                }
            }
        }
    }
}
