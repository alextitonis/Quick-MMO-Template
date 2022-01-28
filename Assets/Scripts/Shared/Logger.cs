using System.IO;

public class Logger
{
    public static bool Log(string text, string path, string fileName, string fileExtention = ".txt")
    {
        string _path = path + fileName + fileExtention;
        if (File.Exists(_path))
            return false;

        FileStream file = File.Create(_path);
        file.Close();

        File.WriteAllText(_path, text);

        return true;
    }
    public static bool Log(string[] text, string path, string fileName, string fileExtention = ".txt")
    {
        string _path = path + fileName + fileExtention;
        if (File.Exists(_path))
            return false;

        FileStream file = File.Create(_path);
        file.Close();

        File.WriteAllLines(_path, text);

        return true;
    }
    public static bool Log(byte[] data, string path, string fileName, string fileExtention = ".txt")
    {
        string _path = path + fileName + fileExtention;
        if (File.Exists(_path))
            return false;

        FileStream file = File.Create(_path);
        file.Close();

        File.WriteAllBytes(_path, data);

        return true;
    }
}