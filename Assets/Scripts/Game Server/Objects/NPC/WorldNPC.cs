using UnityEngine;
using static GameServer.World;

namespace GameServer
{
    public class WorldNPC : IWorldObject
    {
        public long spawnID;
        public NPC npc;
        public Zone currentZone;
        public Vector3 initialSpawn;
        public float respawnTime;

        Vector3 previousPosition = Vector3.zero;
        Quaternion previousRotation = Quaternion.identity;

        public bool isDead
        {
            get
            {
                return npc.vitals.CurrentHealth <= 0;
            }
        }

        public void Spawn(long spawnID, Vector3 initialSpawn, float respawnTime, NPC npc)
        {
            this.spawnID = spawnID;
            this.initialSpawn = initialSpawn;
            this.respawnTime = respawnTime;
            this.npc = npc;

            type = ObjectType.AI;

            currentZone = getInstance.getZoneForPosition(transform.position);
        }

        void Update()
        {
            if (Utils_Maths.CloseEnough(previousPosition, transform.position, 0.01) || Utils_Maths.CloseEnough(previousRotation, transform.rotation, 0.01))
            {
                Server.getInstance.MoveNPC(spawnID, transform.position, transform.rotation, currentZone);
                previousPosition = transform.position;
                previousRotation = transform.rotation;
            }

            RegenerationTask();
        }

        public void SetLevel(int level)
        {
            int previousLevel = npc.level;
            npc.level = level;

            //clalculate stats;
        }

        public void RegenerationTask()
        {
            if (!isDead)
            {
                float healthRegeneration = 0f;
                float manaRegeneration = 0f;

                healthRegeneration = npc.baseStats.Stamina * StatsManager.STA_MOD / 100;
                manaRegeneration = npc.baseStats.Mental * StatsManager.MEN_MOD / 100;

                npc.vitals.CurrentHealth += healthRegeneration;
                npc.vitals.CurrentMana += manaRegeneration;

                npc.vitals.CurrentHealth = Mathf.Clamp(npc.vitals.CurrentHealth, 0, npc.vitals.MaxHealth);
                npc.vitals.CurrentMana = Mathf.Clamp(npc.vitals.CurrentMana, 0, npc.vitals.MaxMana);
            }
        }
    }
}