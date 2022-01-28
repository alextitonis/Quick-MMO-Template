using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    public class SpellDatabase : MonoBehaviour
    {
        public static SpellDatabase getInstance;
        void Awake() { getInstance = this; }

        [SerializeField] Spell[] spells;

        public Spell getSpell(int ID)
        {
            for (int i = 0; i < spells.Length; i++)
            {
                if (spells[i].ID == ID)
                    return spells[i];
            }

            return null;
        }

        public List<Spell> getSpells()
        {
            List<Spell> res = new List<Spell>();

            for (int i = 0; i < spells.Length; i++)
                res.Add(spells[i]);

            return res;
        }
    }
}