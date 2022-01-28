using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] InputField messageInput;
    [SerializeField] int maxMessages = 25;
    [SerializeField] GameObject messagePrefab;
    [SerializeField] Transform spawnLocation;
    [SerializeField] Button button;
    [SerializeField] _MessageType currentType;

    List<GameObject> chatMessages = new List<GameObject>();

    void Start()
    {
        button.onClick.AddListener(delegate { UpdateMessageType(); });
    }

    public void GetMessage(_Message msg)
    {
        string message = "[" + msg.Sender + "]: " + msg.Content;

        if (chatMessages.Count > maxMessages)
        {
            Destroy(chatMessages[0]);
            chatMessages.Remove(chatMessages[0]);
        }

        GameObject _msg = Instantiate(messagePrefab, spawnLocation);
        _msg.GetComponentInChildren<Text>().text = message;
        _msg.GetComponentInChildren<Text>().color = getColor(msg.Type);

        chatMessages.Add(_msg);
    }

    public void SendMessage()
    {
        string _msg = messageInput.text;
        if (string.IsNullOrEmpty(_msg))
            return;

        messageInput.text = "";

        _Message msg = new _Message("", _msg, currentType);
        Client.getInstance._SendMessage(msg);
    }

    public void UpdateMessageType()
    {
        currentType++;

        if ((int)currentType >= 3)
            currentType = 0;

        button.GetComponentInChildren<Text>().text = currentType.ToString();
    }

    public void Enter()
    {
        if (!string.IsNullOrEmpty(messageInput.text))
            SendMessage();
        else
            messageInput.ActivateInputField();
    }

    Color getColor(_MessageType type)
    {
        switch (type)
        {
            case _MessageType._Chat_General:
                return Color.white;
            case _MessageType._Chat_Global:
                return Color.white;
            case _MessageType._Chat_Whisper:
                return Color.magenta;
            case _MessageType._Server_Info:
                return Color.green;
            case _MessageType._Server_Error:
                return Color.red;
            default:
                return Color.white;
        }
    }
}