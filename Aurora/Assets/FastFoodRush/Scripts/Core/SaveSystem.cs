using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public static class SaveSystem
    {
        /// <summary>
        /// Saves data to a file in binary format.
        /// The data is serialized and stored in the application's persistent data path.
        /// </summary>
        /// <typeparam name="T">The type of the data to be saved.</typeparam>
        /// <param name="data">The data to be saved.</param>
        /// <param name="fileName">The name of the file to save the data in.</param>
        public static void SaveData<T>(T data, string fileName)
        {
            // Determine the file path where the data will be saved
            string filePath = Application.persistentDataPath + "/" + fileName + ".dat";

            // Create a BinaryFormatter to serialize the data
            BinaryFormatter formatter = new BinaryFormatter();

            // Create a file stream for writing the data
            FileStream fileStream = new FileStream(filePath, FileMode.Create);

            // Serialize the data and save it to the file
            formatter.Serialize(fileStream, data);
            fileStream.Close();
        }

        /// <summary>
        /// Loads data from a file in binary format.
        /// If the file doesn't exist, the default value of the data type is returned.
        /// </summary>
        /// <typeparam name="T">The type of the data to be loaded.</typeparam>
        /// <param name="fileName">The name of the file to load the data from.</param>
        /// <returns>The loaded data, or the default value if the file doesn't exist.</returns>
        public static T LoadData<T>(string fileName)
        {
            // Determine the file path from which the data will be loaded
            string filePath = Application.persistentDataPath + "/" + fileName + ".dat";

            if (!File.Exists(filePath))
                return default(T);

            // 空文件或损坏文件会导致 BinaryFormatter 抛 SerializationException，进而使 RestaurantManager.Awake 中断、data 为 null
            try
            {
                var info = new FileInfo(filePath);
                if (info.Length == 0)
                {
                    Debug.LogWarning($"[SaveSystem] 存档为空，已删除: {filePath}");
                    File.Delete(filePath);
                    return default(T);
                }

                var formatter = new BinaryFormatter();
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return (T)formatter.Deserialize(fileStream);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] 无法读取存档 '{fileName}.dat'（可能损坏或与当前 Unity 版本不兼容）: {e.Message}。将使用默认数据。");
                try
                {
                    string corruptPath = filePath + ".corrupt.bak";
                    if (File.Exists(corruptPath))
                        File.Delete(corruptPath);
                    File.Move(filePath, corruptPath);
                }
                catch
                {
                    try { File.Delete(filePath); } catch { /* ignore */ }
                }

                return default(T);
            }
        }

        /// <summary>
        /// Retrieves the name of the most recently modified save file (without the extension).
        /// </summary>
        /// <returns>The name of the latest save file without its extension, or <c>null</c> if no save files are found.</returns>
        public static string GetLatestSaveFileName()
        {
            // Get all .dat save files from the persistent data path
            var saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.dat");

            // Order the files by their last write time in descending order and return the file name without the extension
            return saveFiles.OrderByDescending(File.GetLastWriteTime)
                            .Select(Path.GetFileNameWithoutExtension)
                            .FirstOrDefault(); // Returns null if no save files are found
        }
    }
}
