using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class DataSaver
{
   private const string SaveDirectoryName = "saves";
   
   private static string directoryPath = Path.Combine(Application.persistentDataPath, SaveDirectoryName);
   private static string GetFilePath(string fileName) => Path.Combine(directoryPath,fileName);

   public static void SaveFile<T>(string filename, in T objectToPersist)
   {
      if (!Directory.Exists(directoryPath))
         Directory.CreateDirectory(directoryPath);
      
      FileStream fileStream = File.Create(GetFilePath(filename));
      BinaryFormatter binaryFormatter = new BinaryFormatter();

      var json = JsonUtility.ToJson(objectToPersist);
      binaryFormatter.Serialize(fileStream,json);
      fileStream.Close();
   }
   
   public static bool LoadFile<T>(string filename, out T objectToLoad)
   {
      string filePath = GetFilePath(filename);
      if (File.Exists(filePath))
      {
         var fileStream = File.Open(filePath, FileMode.Open);
         BinaryFormatter binaryFormatter = new BinaryFormatter();

         objectToLoad = JsonUtility.FromJson<T>((string) binaryFormatter.Deserialize(fileStream));
         fileStream.Close();

         return true;
      }

      objectToLoad = default;
      return false;
   }

   public static void ClearFile(string filename)
   {
      string filePath = GetFilePath(filename);
      if (File.Exists(filePath))
         File.Delete(filePath);
   }
}
