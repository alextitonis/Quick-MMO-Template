using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

public class ServerSelectionManager : MonoBehaviour
{
    public static ServerSelectionManager getInstance;
    void Awake() { getInstance = this; }

    public List<_GameServer> GameServers = new List<_GameServer>();
    public _GameServer gameServer = null;

    [SerializeField] GameObject gsPrefab;
    [SerializeField] Transform spawnLocation;
    List<GameObject> gsObjects = new List<GameObject>();

    public void SetUp(bool request = true)
    {
        foreach (var go in gsObjects)
            Destroy(go);

        gsObjects.Clear();

        can = false;

        if (request)
            Client.getInstance.RequestGameServers();

        if (!request)
        {
            foreach (var gs in GameServers)
            {
                GameObject go = Instantiate(gsPrefab, spawnLocation);
                go.GetComponent<HUD_GameServer>().SetUp(gs, Utils.getInstance.GetPing(gs.IP));

                gsObjects.Add(go);
            }
        }
    }

    bool can = false;
    public void Connect()
    {
        if (!can)
            return;

        Utils.getInstance.Loading(true);

        try
        {
            Client.getInstance.Reconnect(gameServer);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message); MenuManager.getInstance.ChangeScreen(Screen.Offline);
            Utils.getInstance.Loading(false);
            return;
        }

        StartCoroutine(ConnectionDelay());
    }

    IEnumerator ConnectionDelay()
    {
        float time = 0f;
        using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
        {
            time = ping.Send(gameServer.IP).RoundtripTime;
        }
        
        yield return new WaitForSeconds(3f);

        MenuManager.getInstance.ChangeScreen(Screen.CharacterSelect);
        Utils.getInstance.Loading(false);
        Client.getInstance.RequestCharacters();
    }
    public void OnSelectGameServer(_GameServer gameServer)
    {
        this.gameServer = gameServer;
        can = true;

        foreach (var gs in gsObjects)
        {
            if (gs.GetComponent<HUD_GameServer>().gameServer == gameServer)
                gs.GetComponent<HUD_GameServer>().UpdateImg(true);
            else
                gs.GetComponent<HUD_GameServer>().UpdateImg(false);
        }
    }
}