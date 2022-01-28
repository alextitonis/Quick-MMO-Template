using DarkRift;
using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Net;
using UnityEngine;
using static ItemDatabase;
using static SpellDatabase;

public class Client : MonoBehaviour
{
    public static Client getInstance;
    public UnityClient client;
    public static ushort localID;
    public static int maxCharacters = 0;

    [SerializeField] float TimeOutTimeChecker = 5 * 60;

    void Awake()
    {
        getInstance = this;

        client.MessageReceived += MessageReceived;
        client.Disconnected += ServerDisconnected;
    }

    public void Connect()
    {
        try
        {
            if (client.ConnectionState != ConnectionState.Connected && client.ConnectionState != ConnectionState.Connecting)
                client.Connect(client.Address, client.Port, client.IPVersion);

            MenuManager.getInstance.ChangeScreen(Screen.Login);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MenuManager.getInstance.ChangeScreen(Screen.Offline);
        }
    }
    public void Reconnect(_GameServer gs)
    {
        CloseConnection();
        client.Connect(IPAddress.Parse(gs.IP), gs.Port, gs.ipVersion);
        Welcome(LoginManager.getInstance.email, LoginManager.getInstance.password);
    }
    public void CloseConnection()
    {
        client.Disconnect();
    }

    private void ServerDisconnected(object sender, DarkRift.Client.DisconnectedEventArgs e)
    {
        if (!e.LocalDisconnect)
            MenuManager.getInstance.ChangeScreen(Screen.Offline);
    }
    private void MessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        HandleData.getInstance.Handle(sender, e);
    }

    void Send(DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
    {
        writer = Cryptography.EncryptWriter(writer);
        using (Message msg = Message.Create(packetID, writer)) client.SendMessage(msg, sendMode);
    }

    public void Disconnect()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(0);

            Send(writer, Packets.Disconnect);
        }
    }
    public void Login(string email, string password)
    {
        Utils.getInstance.Loading(true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(email);
            writer.Write(password);

            Send(writer, Packets.Login);
        }
    }
    public void Register(string email, string password)
    {
        Utils.getInstance.Loading(true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(email);
            writer.Write(password);

            Send(writer, Packets.Register);
        }
    }
    public void ForgotPassword(string email)
    {
        Utils.getInstance.Loading(true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(email);

            Send(writer, Packets.ForgotPassowrd);
        }
    }

    public void RequestCharacters()
    {
        Utils.getInstance.Loading(true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(0);

            Send(writer, Packets.RequestCharacters);
        }

        MenuManager.getInstance.ChangeScreen(Screen.CharacterSelect);
    }
    public void CreateCharacter(CharacterSettings settings)
    {
        Utils.getInstance.Loading(true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(settings.Name);
            writer.Write((int)settings.Race);
            writer.Write((int)settings.Class);
            writer.Write((int)settings.Gender);

            Send(writer, Packets.CreateCharacter);
        }
    }
    public void DeleteCharacter(string ID)
    {
        Utils.getInstance.Loading(true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(ID);

            Send(writer, Packets.DeleteCharacter);
        }
    }
    public void EnterGame(string ID)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(ID);

            Send(writer, Packets.EnterGame);
        }
    }

    public void RequestGameServers()
    {
        Utils.getInstance.Loading(true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(0);

            Send(writer, Packets.RequestGameServers);
        }
    }
    public void SelectServer(ushort ID)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(ID);

            Send(writer, Packets.ServerSelected);
        }
    }
    public void Welcome(string email, string password)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(email);
            writer.Write(password);

            Send(writer, Packets.Welcome);
        }
    }

    bool receivedTimeOurCheck;
    public IEnumerator TimeOutCheckRoutine()
    {
        receivedTimeOurCheck = true;
        TimeOutCheck();
        yield return new WaitForSeconds(TimeOutTimeChecker);
        receivedTimeOurCheck = false;
        yield return new WaitForSeconds(60f);

        if (!receivedTimeOurCheck)
            MenuManager.getInstance.ChangeScreen(Screen.Offline);
    }
    public void TimeOutCheck(bool isOnline = true)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(isOnline);

            Send(writer, Packets.TimeOut);
        }
    }

    public void PlayerMovement(float horizontal, float vertical)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(horizontal);
            writer.Write(vertical);

            Send(writer, Packets.PlayerMovement);
        }
    }
    public void Jump()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((byte)0);

            Send(writer, Packets.Jump);
        }
    }
    public void PlayerAnimation(AnimationType animationType, string animationName, string value)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((int)animationType);
            writer.Write(animationName);
            writer.Write(value);

            Send(writer, Packets.PlayerAnimation);
        }
    }
    public void SendPlayAnimation(string anim)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(anim);

            Send(writer, Packets.PlayAnimation);
        }
    }

    public void _SendMessage(_Message msg)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(msg.Content);
            writer.Write((int)msg.Type);

            Send(writer, Packets._SendMessage);
        }
    }

    public void RemoveItem(int slot)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(slot);

            Send(writer, Packets.RemoveItem);
        }
    }

    public void MoveWithMouse(Vector3 pos)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            Send(writer, Packets.MoveWithMouse);
        }
    }

    public void SetTargetPlayer(ushort id)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(id);

            Send(writer, Packets.SetTargetPlayer);
        }
    }
    public void SetTargetNPC(long id)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(id);

            Send(writer, Packets.SetTargetNPC);
        }
    }

    public void ReportAProblem(string data)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(data);

            Send(writer, Packets.ReportAProblem);
        }
    }

    public void UseItem(Item item)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(item.ID);

            Send(writer, Packets.UseItem);
        }
    }

    public void UnEquipItem(EquipmentLocation slot)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((int)slot);

            Send(writer, Packets.EquipItem);
        }
    }

    public void UseSpell(Spell spell)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(spell.ID);

            Send(writer, Packets.UseSpell);
        }
    }
}