public class _Message
{
    public string Sender;
    public string Content;
    public _MessageType Type;

    public _Message(string Sender, string Content, _MessageType Type)
    {
        this.Sender = Sender;
        this.Content = Content;
        this.Type = Type;
    }
}

public enum _MessageType
{
    _None = -1,
    _Chat_General = 0,
    _Chat_Global = 1,
    _Chat_Guild = 2,
    _Chat_Whisper = 3,
    _Server_Info = 4,
    _Server_Error = 5,
    _Command = 6,
}