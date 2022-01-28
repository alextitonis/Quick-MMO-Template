namespace LoginServer
{
    public class Config
    {
        public static string DbServer = "127.0.0.1";
        public static string DbName = "ls";
        public static string DbUsername = "root";
        public static string DbPassword = "";
        public static string DBSSslMode = "none";

        public static int MaxCharacters = 5;

        public static bool Enable_Emailing_System = true;
        public static string Mail_From = "";
        public static string Mail_From_Password = "";
        public static string SMTP_Client = "smtp.gmail.com";
        public static int SMTP_Port = 587;
        public static bool Email_Enable_SSL = false;   
    }
}