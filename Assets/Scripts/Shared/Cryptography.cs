using DarkRift;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Cryptography
{
    static byte xorConstant = 0x53;
    public static string Encrypt(string input)
    {
        string output = "";

        byte[] data = Encoding.UTF8.GetBytes(input);
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(data[i] ^ xorConstant);

        output = Encoding.UTF8.GetString(data);
        return output;
    }
    public static string Encrypt_Custom(string input)
    {
        string output = "";

        foreach (var letter in input)
        {
            switch (letter)
            {
                case 'A':
                    output += 'D';
                    break;
                case 'B':
                    output += 'F';
                    break;
                case 'C':
                    output += 'H';
                    break;
                case 'D':
                    output += 'A';
                    break;
                case 'E':
                    output += 'G';
                    break;
                case 'F':
                    output += 'B';
                    break;
                case 'G':
                    output += 'E';
                    break;
                case 'H':
                    output += 'C';
                    break;
                case 'I':
                    output += 'K';
                    break;
                case 'J':
                    output += 'N';
                    break;
                case 'K':
                    output += 'I';
                    break;
                case 'L':
                    output += 'P';
                    break;
                case 'M':
                    output += 'R';
                    break;
                case 'N':
                    output += 'J';
                    break;
                case 'O':
                    output += 'U';
                    break;
                case 'P':
                    output += 'L';
                    break;
                case 'Q':
                    output += 'Z';
                    break;
                case 'R':
                    output += 'M';
                    break;
                case 'S':
                    output += 'Y';
                    break;
                case 'T':
                    output += 'X';
                    break;
                case 'U':
                    output += 'O';
                    break;
                case 'V':
                    output += 'W';
                    break;
                case 'W':
                    output += 'V';
                    break;
                case 'X':
                    output += 'T';
                    break;
                case 'Y':
                    output += 'S';
                    break;
                case 'Z':
                    output += 'Q';
                    break;

                case 'a':
                    output += 'd';
                    break;
                case 'b':
                    output += 'f';
                    break;
                case 'c':
                    output += 'h';
                    break;
                case 'd':
                    output += 'a';
                    break;
                case 'e':
                    output += 'g';
                    break;
                case 'f':
                    output += 'b';
                    break;
                case 'g':
                    output += 'e';
                    break;
                case 'h':
                    output += 'c';
                    break;
                case 'i':
                    output += 'k';
                    break;
                case 'j':
                    output += 'l';
                    break;
                case 'k':
                    output += 'i';
                    break;
                case 'l':
                    output += 'p';
                    break;
                case 'm':
                    output += 'r';
                    break;
                case 'n':
                    output += 'j';
                    break;
                case 'o':
                    output += 'u';
                    break;
                case 'p':
                    output += 'l';
                    break;
                case 'q':
                    output += 'z';
                    break;
                case 'r':
                    output += 'm';
                    break;
                case 's':
                    output += 'y';
                    break;
                case 't':
                    output += 'x';
                    break;
                case 'u':
                    output += 'o';
                    break;
                case 'v':
                    output += 'w';
                    break;
                case 'w':
                    output += 'v';
                    break;
                case 'x':
                    output += 't';
                    break;
                case 'y':
                    output += 's';
                    break;
                case 'z':
                    output += 'q';
                    break;

                case '0':
                    output += '9';
                    break;
                case '1':
                    output += '4';
                    break;
                case '2':
                    output += '7';
                    break;
                case '3':
                    output += '8';
                    break;
                case '4':
                    output += '1';
                    break;
                case '5':
                    output += '6';
                    break;
                case '6':
                    output += '5';
                    break;
                case '7':
                    output += '2';
                    break;
                case '8':
                    output += '3';
                    break;
                case '9':
                    output += '0';
                    break;

                default:
                    output += letter;
                    break;
            }
        }

        return output;
    }
    public static string Decrypt(string input)
    {
        string output = "";

        byte[] data = Encoding.UTF8.GetBytes(input);
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(data[i] ^ xorConstant);

        output = Encoding.UTF8.GetString(data);
        return output;
    }
    public static void CryptFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        byte[] data = File.ReadAllBytes(filePath);
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(data[i] ^ xorConstant);

        File.Delete(filePath);
        var file = File.Create(filePath);
        file.Close();
        File.WriteAllBytes(filePath, data);
    }
    public static byte[] CryptByteArray(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(data[i] ^ xorConstant);

        return data;
    }


    public struct CryptoResult_Text
    {
        public string Text;
        public bool Done;
    }
    public struct CryptoResult_Bytes
    {
        public byte[] Bytes;
        public bool Done;
    }

    public static CryptoResult_Text AES_Encrypt(string text, string password, byte[] saltBytes)
    {
        CryptoResult_Text result = new CryptoResult_Text();
        result.Text = text;
        result.Done = false;

        if (string.IsNullOrEmpty(text))
            return result;

        if (string.IsNullOrEmpty(password))
            return result;

        byte[] data = Encoding.UTF8.GetBytes(text);
        byte[] _password = Encoding.UTF8.GetBytes(password);

        _password = SHA256.Create().ComputeHash(_password);

        CryptoResult_Bytes _result = AES_Encrypt(data, _password, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        if (!_result.Done)
            return result;

        result.Done = true;
        result.Text = Encoding.UTF8.GetString(_result.Bytes);

        return result;
    }
    public static CryptoResult_Bytes AES_Encrypt(byte[] data, string password, byte[] saltBytes)
    {
        byte[] _password = Encoding.UTF8.GetBytes(password);
        return AES_Encrypt(data, _password, saltBytes);
    }
    //saltBytes should be atleast 8 (example: { 1, 2, 3, 4, 5, 6, 7, 8 });
    public static CryptoResult_Bytes AES_Encrypt(byte[] data, byte[] password, byte[] saltBytes)
    {
        CryptoResult_Bytes result = new CryptoResult_Bytes();
        result.Bytes = data;
        result.Done = false;

        using (MemoryStream ms = new MemoryStream())
        using (RijndaelManaged AES = new RijndaelManaged())
        {
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes, 1000);

            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CBC;

            using (CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.Close();
            }

            data = ms.ToArray();

            result.Bytes = data;
            result.Done = true;
        }

        return result;
    }

    public static CryptoResult_Text AES_Decrypt(string text, string password, byte[] saltBytes)
    {
        CryptoResult_Text result = new CryptoResult_Text();
        result.Text = text;
        result.Done = false;

        if (string.IsNullOrEmpty(text))
            return result;

        if (string.IsNullOrEmpty(password))
            return result;

        byte[] data = Convert.FromBase64String(text);
        byte[] _password = Encoding.UTF8.GetBytes(password);

        _password = SHA256.Create().ComputeHash(_password);

        CryptoResult_Bytes _result = AES_Decrypt(data, _password, saltBytes);
        if (!_result.Done)
            return result;

        result.Done = _result.Done;
        result.Text = Encoding.UTF8.GetString(_result.Bytes);

        return result;
    }
    public static CryptoResult_Bytes AES_Decrypt(byte[] data, string password, byte[] saltBytes)
    {
        byte[] _password = Encoding.UTF8.GetBytes(password);
        return AES_Decrypt(data, _password, saltBytes);
    }
    public static CryptoResult_Bytes AES_Decrypt(byte[] data, byte[] password, byte[] saltBytes)
    {
        CryptoResult_Bytes result = new CryptoResult_Bytes();
        result.Bytes = data;
        result.Done = false;

        using (MemoryStream ms = new MemoryStream())
        using (RijndaelManaged AES = new RijndaelManaged())
        {
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes, 1000);

            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CBC;

            using (CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.Close();
            }

            result.Done = true;
            result.Bytes = ms.ToArray();
        }

        return result;
    }

    public static DarkRiftWriter EncryptWriter(DarkRiftWriter writer)
    {
        Message msg = Message.Create(0, writer);
        DarkRiftReader reader = msg.GetReader();

        byte[] data = reader.ReadRaw(reader.Length);
        data = CryptByteArray(data);

        writer = DarkRiftWriter.Create();
        writer.WriteRaw(data, 0, data.Length);

        msg.Dispose();
        reader.Dispose();
        return writer;
    }
    public static DarkRiftReader DecryptReader(DarkRiftReader reader)
    {
        byte[] data = reader.ReadRaw(reader.Length);
        data = CryptByteArray(data);

        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.WriteRaw(data, 0, data.Length);

        Message msg = Message.Create(0, writer);
        DarkRiftReader newReader = msg.GetReader();

        msg.Dispose();
        writer.Dispose();
        reader.Dispose();

        return newReader;
    }
}