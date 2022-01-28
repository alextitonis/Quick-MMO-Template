using UnityEngine;
using System.Collections.Generic;

public class NPCDatabase : MonoBehaviour
{
    public static NPCDatabase getInstance;
    void Awake()
    {
        getInstance = this;
    }

    [SerializeField] NPC[] npcs;

    public GameObject getObject(int ID)
    {
        NPC temp = getNPC(ID);

        if (temp != null)
            return temp.clientPrefab;
        else
            return null;
    }
    public NPC getNPC(int ID)
    {
        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i].ID == ID)
                return npcs[i];
        }

        return null;
    }
    public string getNPCName(int ID)
    {
        NPC temp = getNPC(ID);

        if (temp != null)
            return temp.Name;
        else
            return "";
    }
}