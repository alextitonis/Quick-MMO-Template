using UnityEngine;

namespace GameServer
{
    public class Config
    {
        public static string Game_Server_Name = "Main";
        public static string Game_Server_Ip = "127.0.0.1";
        public static int Game_Server_Port = 4297;
        public static bool Game_Server_Ip_Version_IpV4 = true;
       
        public static string DbServer = "127.0.0.1";
        public static string DbName = "gs";
        public static string DbUsername = "root";
        public static string DbPassword = "";
        public static string DBSSslMode = "none";

        public static int MaxCharacters = 5;

        public static Vector3 StartingPosition = new Vector3(0f, 0f, 0f);
        public static Quaternion StartingRotation = new Quaternion(0f, 0f, 0f, 0f);

        public static int PlayerAccess = 0;
        public static int TestAccess = 1;
        public static int ReporterAccess = 2;
        public static int EventManagerAccess = 3;
        public static int GameMasterAccess = 4;
        public static int AdminAccess = 5;
        public static int BannedAccess = -1;
    }
}