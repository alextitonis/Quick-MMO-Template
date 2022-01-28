namespace GameServer
{
    public class StatsManager
    {
        public class Stats_Vitals
        {
            public BaseStats baseStats;
            public Stats stats;
            public Vitals vitals;
        }

        public static Stats_Vitals getStartingStats(Races race, Classes _class)
        {
            Stats_Vitals stats = new Stats_Vitals();

            stats.baseStats.Strength = 10;
            stats.baseStats.Agility = 10;
            stats.baseStats.Stamina = 10;
            stats.baseStats.Intelligence = 10;
            stats.baseStats.Wit = 10;
            stats.baseStats.Mental = 10;

            switch (race)
            {
                case Races.Human:
                    switch (_class)
                    {
                        case Classes.Fighter:
                            break;
                        case Classes.Mage:
                            break;
                    }
                    break;
            }
            return stats;
        }
        public static Stats_Vitals getBaseStats(Races race, Classes _class, ushort level)
        {
            Stats_Vitals stats = new Stats_Vitals();

            stats.baseStats.Strength = (ushort)(10 + level);
            stats.baseStats.Agility = (ushort)(10 + level);
            stats.baseStats.Stamina = (ushort)(10 + level);
            stats.baseStats.Intelligence = (ushort)(10 + level);
            stats.baseStats.Wit = (ushort)(10 + level);
            stats.baseStats.Mental = (ushort)(10 + level);

            return stats;
        }

        public static float STR_MOD = 5;
        public static float AGI_MOD = 5;
        public static float STA_MOD = 5;
        public static float INT_MOD = 5;
        public static float WIT_MOD = 5;
        public static float MEN_MOD = 5;

        public static float MAX_MOVEMENT_SPEED = 10;

        public static Stats_Vitals calculateVitals(CharacterSettings settings)
        {
            Stats_Vitals stats = new Stats_Vitals();
            stats.baseStats = settings.baseStats;

            stats.stats.PhysicalAttack = stats.baseStats.Strength * STR_MOD;
            stats.stats.MagicalAttack = stats.baseStats.Intelligence * INT_MOD;

            stats.stats.AttackSpeed = stats.baseStats.Agility * AGI_MOD;
            stats.stats.CastingSpeed = stats.baseStats.Wit * WIT_MOD;

            stats.stats.PhysicalArmor = (stats.baseStats.Agility * AGI_MOD) + (stats.baseStats.Stamina * STA_MOD);
            stats.stats.MagicalArmor = (stats.baseStats.Mental * MEN_MOD) + (stats.baseStats.Stamina * STA_MOD);

            stats.stats.PhysicalCritical = ((stats.baseStats.Agility * AGI_MOD) + (stats.baseStats.Strength * STR_MOD)) / 100;
            stats.stats.MagicalCritical = ((stats.baseStats.Wit * WIT_MOD) + (stats.baseStats.Intelligence * INT_MOD)) / 100;

            stats.stats.Cooldown = (stats.baseStats.Mental * MEN_MOD) / 100;

            stats.stats.MovementSpeed = 4 + (stats.baseStats.Agility * AGI_MOD);
            if (stats.stats.MovementSpeed > MAX_MOVEMENT_SPEED)
                stats.stats.MovementSpeed = MAX_MOVEMENT_SPEED;

            stats.stats.HitChance = (stats.baseStats.Agility * AGI_MOD) / 100;

            stats.vitals.MaxHealth = stats.baseStats.Stamina * STA_MOD * 10;
            stats.vitals.MaxMana = stats.baseStats.Mental * MEN_MOD * 10;

            return stats;
        }
    }
}