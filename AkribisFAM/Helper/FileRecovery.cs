using AkribisFAM.Manager;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AkribisFAM.Helper
{
    public static class FileRecovery
    {
      
        public static bool RecoverFile<T>(string originalFile, string tempFile, string backupFile)
        {
            RecoverFile(originalFile, tempFile, backupFile);

            // Check if the file can be deserialized to the specified type
            if (!IsFileSerializableToClass<T>(originalFile))
            {
                if (!File.Exists(backupFile)) return false;
                File.Copy(backupFile, originalFile, overwrite: true);
                Console.WriteLine("File was corrupted; restored from backup.");
                //return false;
            }

            return true;
        }
        public static bool RecoverFile(string originalFile, string tempFile, string backupFile)
        {
            // Complete the replacing to original file
            if (File.Exists(tempFile))
            {
                Console.WriteLine("File replace was not completed; restored from temp file.");
                if (File.Exists(originalFile))
                    File.Replace(tempFile, originalFile, backupFile);
                else
                    File.Move(tempFile, originalFile);
            }

            // Verify the integrity of the new file
            if (IsFileCorrupted(originalFile))
            {
                if (!File.Exists(backupFile)) return false;
                // If verification fails, restore from backup
                File.Copy(backupFile, originalFile, overwrite: true);
                Console.WriteLine("File was corrupted; restored from backup.");
            }


            return true;
        }
        private static bool IsFileSerializableToClass<T>(string filePath)
        {
            try
            {
                // Read file content
                string fileContent = File.ReadAllText(filePath);

                // Attempt to deserialize the content to an object of type T
                JsonConvert.DeserializeObject<T>(fileContent);

                // If deserialization succeeds, return true
                return true;
            }
            catch (JsonException ex)
            {
                // If there is an error during deserialization, log it and return false
                Console.WriteLine($"Error deserializing file to {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        public static bool IsFileCorrupted(string originalFilePath)
        {
            try
            {
                // Define "weird" characters as anything not in the ASCII range (0-127)
                //if (Regex.IsMatch(File.ReadAllText(originalFilePath), @"[^\u0000-\u007F]"))
                    if (Regex.IsMatch(File.ReadAllText(originalFilePath), @"[^\u0000-\u007F]|(\0{2,})"))
                    {
                    Console.WriteLine("Original file contains weird characters.");
                    return true; // File is considered corrupted
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading original file: {ex.Message}");
                return true; // Considered corrupted if we cannot read the file
            }

            // If all checks pass, the file is not considered corrupted
            return false;
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
