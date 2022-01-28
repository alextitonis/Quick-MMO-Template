using UnityEngine;
using System.Collections.Generic;

namespace GameServer {
    public class NPCDatabase : MonoBehaviour
    {

        public static NPCDatabase getInstance;
        void Awake()
        {
            getInstance = this;
        }

        [SerializeField] NPC[] npcs;

        public NPC getNpc(int ID)
        {
            for (int i = 0; i < npcs.Length; i++)
            {
                if (npcs[i].ID == ID)
                    return npcs[i];
            }

            return null;
        }
    }
}