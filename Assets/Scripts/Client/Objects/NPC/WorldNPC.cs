using UnityEngine;

public class WorldNPC : IWorldObject
{
    public NPC data;
    public long spawnID;
    public int level;

    public void SetUp(NPC data, long spawnID, int level = 1)
    {
        this.data = data;
        this.spawnID = spawnID;
        this.level = level;

        type = ObjectType.AI;
    }
}