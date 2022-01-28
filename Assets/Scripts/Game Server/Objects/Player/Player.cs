using DarkRift.Server;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    [System.Serializable]
    public class Player
    {
        IClient client;
        CharacterSettings settings;
        public float speed = 0.3f;
        public int maxInventorySlots = 25;
        List<Player> selflList = new List<Player>();
        List<InventoryItem> inventory = new List<InventoryItem>();
        List<PlayerSpell> spells = new List<PlayerSpell>();
        Dictionary<EquipmentLocation, Item> equipment = new Dictionary<EquipmentLocation, Item>();
        public float baseAttackRange { get { return 10; } }
        public WorldPlayer worldPlayer;
        public bool isDead
        {
            get
            {
                return settings.vitals.CurrentHealth <= 0;
            }
        }
        public ushort level
        {
            get
            {
                return ExperienceManager.calculateLevel(settings.experience);
            }
        }

        public enum SelectedObjectType
        {
            None = -1,
            Player = 0,
            NPC = 1,
        }
        public SelectedObjectType targetType { get; private set; }
        public WorldPlayer playerTarget { get; private set; }
        public WorldNPC npcTarget { get; private set; }

        public bool castingSpell = false;

        public void SetTarget(WorldPlayer player, WorldNPC npc)
        {
            Server.getInstance.Log("got new target", DarkRift.LogType.Info);
            if (player != null)
            {
                if (playerTarget == player)
                    worldPlayer.GoTo(player.transform.position);
            }
            else if (npc != null)
            {
                if (npcTarget == npc)
                    worldPlayer.GoTo(npc.transform.position);
            }

            playerTarget = player;
            npcTarget = npc;

            if (player != null)
                targetType = SelectedObjectType.Player;
            else if (player != null)
                targetType = SelectedObjectType.NPC;
            else if (player == null && npc == null)
                targetType = SelectedObjectType.None;
        }

        public Player(IClient client, CharacterSettings settings)
        {
            this.client = client;
            this.settings = settings;

            selflList.Add(this);

            World.getInstance.SpawnPlayer(this);

            inventory = Database.getInstance.loadInventory(this);
            Server.getInstance.SendVitals(settings.vitals, client);
            if (settings.vitals.CurrentHealth <= 0)
                worldPlayer.DoDie();

            equipment = Database.getInstance.loadEquipment(this);
            foreach (var i in equipment.Values)
                EquipItem(i);
            spells = Database.getInstance.loadPlayerSpells(this);
            foreach (var i in spells)
            {
                Server.getInstance.UpdateSpell(this, i.spell, SpellPacketUpdateType.Add, i.currentCooldown);

                if (i.spell.OperateType == SpellOperateType.Passive)
                {
                }
            }
        }

        public IClient getClient() { return client; }
        public CharacterSettings getSettings() { return settings; }

        public void setClient(IClient value) { client = value; }
        public void setSettings(CharacterSettings value) { settings = value; }

        public void setPosition(Vector3 value)
        {
            settings.Position = value;
        }
        public void setRotation(Quaternion value)
        {
            settings.Rotation = value;
        }

        public Vector3 getPosition() { return settings.Position; }
        public Quaternion getRotation() { return settings.Rotation; }

        public Stats getStats() { return settings.stats; }
        public BaseStats getBaseStats() { return settings.baseStats; }
        public void setStats(Stats value)
        {
            settings.stats = value;
            Server.getInstance.SendStats(this);
        }
        public void setBaseStats(BaseStats value)
        {
            settings.baseStats = value;
            StatsManager.Stats_Vitals _stats = StatsManager.calculateVitals(settings);

            worldPlayer.SetSpeed(_stats.stats.MovementSpeed);

            float healthDifference = 0f;
            float manaDifference = 0f;

            healthDifference = _stats.vitals.MaxHealth - settings.vitals.MaxHealth;
            manaDifference = _stats.vitals.MaxMana - settings.vitals.MaxMana;

            settings.vitals.MaxHealth = _stats.vitals.MaxHealth;
            settings.vitals.MaxMana = _stats.vitals.MaxMana;

            if (!isDead)
            {
                if (healthDifference < 0 || manaDifference < 0)
                {
                    if (healthDifference < 0)
                    {
                        settings.vitals.CurrentHealth = settings.vitals.MaxHealth;
                        settings.vitals.CurrentMana = settings.vitals.MaxMana;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                    if (manaDifference < 0)
                    {
                    }
                    else
                    {
                        settings.vitals.CurrentHealth += healthDifference;
                        settings.vitals.CurrentMana += manaDifference;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                }
            }

            Server.getInstance.SendVitals(settings.vitals, client);

            setStats(_stats.stats);
            Server.getInstance.SendVitals(settings.vitals, client);
        }

        public List<InventoryItem> getInventory() { return inventory; }

        public void Move(float horizontal, float vertical)
        {
            worldPlayer.Move(horizontal, vertical);
        }
        public void Jump()
        {
            worldPlayer.Jump();
        }

        public void SendInfoMessage(string msg)
        {
            _Message _msg = new _Message("Server", msg, _MessageType._Server_Info);
            Server.getInstance._SendMessage(_msg, selflList);
        }
        public void SendErrorMessage(string msg)
        {
            _Message _msg = new _Message("Server", msg, _MessageType._Server_Error);
            Server.getInstance._SendMessage(_msg, selflList);
        }

        public bool AddItem(Item item, int quantity)
        {
            if (inventory.Count >= maxInventorySlots)
            {
                SendInfoMessage("Your inventory is full!");
                return false;
            }

            InventoryItem _item = null;

            if (HasItem(item))
            {
                _item = inventory.Find(x => x.item == item);
                _item.Quantity += quantity;
            }
            else
            {
                _item = new InventoryItem();
                int slot = 0;
                while (inventory.Find(x => x.slot == slot) != null)
                    slot++;
                _item.item = item;
                _item.Quantity = quantity;
                _item.slot = slot;
            }

            bool added = Database.getInstance.addInventoryItem(this, _item);

            if (!added)
            {
                SendInfoMessage("Error on adding item in inventory (internal)!");
                return false;
            }

            SendInfoMessage("Added " + item.Name + " x " + quantity + " into your inventory!");
            Server.getInstance.SendSlotData(_item, this);

            return true;
        }
        public bool RemoveItem(Item item, int quantity)
        {
            if (inventory.Count <= 0)
                return false;

            if (!HasItem(item, quantity))
                return false;

            InventoryItem _item = inventory.Find(x => (x.item == item) && (x.Quantity == quantity));
            if (_item.Quantity == quantity)
                return RemoveItem(_item.slot);
            else
                _item.Quantity -= quantity;

            bool done = Database.getInstance.removeInventoryItem(this, _item.slot, quantity);
            if (done)
                Server.getInstance.SendSlotData(_item, this);

            return done;
        }
        public bool RemoveItem(Item item)
        {
            if (inventory.Count <= 0)
                return false;

            if (!HasItem(item))
                return false;

            return RemoveItem(inventory.Find(x => x.item == item).slot);
        }
        public bool RemoveItem(int slot)
        {
            if (inventory.Count <= 0)
                return false;

            if (inventory.Find(x => x.slot == slot) == null)
                return false;

            inventory.RemoveAt(inventory.FindIndex(x => x.slot == slot));
            bool done = Database.getInstance.removeInventoryItem(this, slot);

            InventoryItem _item = new InventoryItem();
            _item.item = null;
            _item.Quantity = -1;
            _item.slot = slot;

            if (done)
                Server.getInstance.SendSlotData(_item, this);

            return done;
        }
        public bool HasItem(Item item)
        {
            return inventory.Find(x => x.item == item) != null;
        }
        public bool HasItem(Item item, int quantity)
        {
            if (!HasItem(item))
                return false;

            InventoryItem i = inventory.Find(x => x.item == item);
            return i.Quantity >= quantity;
        }
        public int GetItemQuantity(Item item)
        {
            if (!HasItem(item))
                return 0;

            return inventory.Find(x => x.item == item).Quantity;
        }

        public void TakeDamage(float value)
        {
            SendInfoMessage("You have taken damage: " + value);

            settings.vitals.CurrentHealth -= value;
            if (settings.vitals.CurrentHealth <= 0)
                settings.vitals.CurrentHealth = 0;

            Server.getInstance.SendCurrentHealth(worldPlayer, settings.vitals.CurrentHealth);

            if (settings.vitals.CurrentHealth <= 0)
                worldPlayer.DoDie();
        }
        public void HealDamage(float value)
        {
            SendInfoMessage("You have healed damage: " + value);

            settings.vitals.CurrentHealth += value;
            if (settings.vitals.CurrentHealth >= settings.vitals.MaxHealth)
                settings.vitals.CurrentHealth = settings.vitals.MaxHealth;

            Server.getInstance.SendCurrentHealth(worldPlayer, settings.vitals.CurrentHealth);
        }
        public void RemoveMana(float value)
        {
            settings.vitals.CurrentMana -= value;
            if (settings.vitals.CurrentMana <= 0)
                settings.vitals.CurrentMana = 0;

            Server.getInstance.SendCurrentMana(settings.vitals.CurrentMana, client);
        }
        public void AddMana(float value)
        {
            settings.vitals.CurrentMana += value;
            if (settings.vitals.CurrentMana >= settings.vitals.MaxMana)
                settings.vitals.CurrentMana = settings.vitals.MaxMana;

            Server.getInstance.SendCurrentMana(settings.vitals.CurrentMana, client);
        }

        public void SetAccess(int value)
        {
            settings.access = value;

            if (value == -1)
                Ban();
        }

        public void Kick()
        {
        }
        public void Ban()
        {
        }

        public void RegenerationTask()
        {
            if (!isDead)
            {
                float healthRegeneration = 0f;
                float manaRegeneration = 0f;

                healthRegeneration = settings.baseStats.Stamina * StatsManager.STA_MOD / 100;
                manaRegeneration = settings.baseStats.Mental * StatsManager.MEN_MOD / 100;

                settings.vitals.CurrentHealth += healthRegeneration;
                settings.vitals.CurrentMana += manaRegeneration;

                settings.vitals.CurrentHealth = Mathf.Clamp(settings.vitals.CurrentHealth, 0, settings.vitals.MaxHealth);
                settings.vitals.CurrentMana = Mathf.Clamp(settings.vitals.CurrentMana, 0, settings.vitals.MaxMana);
            }
        }

        public void AddExperience(long value)
        {
            ushort previousLevel = level;
            settings.experience += value;

            if (level > previousLevel)
                setBaseStats(StatsManager.getBaseStats(settings.Race, settings.Class, level).baseStats);

            foreach (var item in equipment.Values)
            {
                //calculate stats
                BaseStats newStats = settings.baseStats;
                newStats.Strength += item.Strength;
                newStats.Agility += item.Agility;
                newStats.Stamina += item.Stamina;
                newStats.Intelligence += item.Intelligence;
                newStats.Wit += item.Wit;
                newStats.Mental += item.Mental;
                setBaseStats(newStats);

                float healthDifference = 0f;
                float manaDifference = 0f;

                Vitals vitals = settings.vitals;
                vitals.MaxHealth += item.MaxHealth;
                vitals.MaxMana += item.MaxMana;

                healthDifference = vitals.MaxHealth - settings.vitals.MaxHealth;
                manaDifference = vitals.MaxMana - settings.vitals.MaxMana;

                settings.vitals.MaxHealth = vitals.MaxHealth;
                settings.vitals.MaxMana = vitals.MaxMana;
                if (healthDifference < 0 || manaDifference < 0)
                {
                    if (healthDifference < 0)
                    {
                        settings.vitals.CurrentHealth = settings.vitals.MaxHealth;
                        settings.vitals.CurrentMana = settings.vitals.MaxMana;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                    if (manaDifference < 0)
                    {
                    }
                    else
                    {
                        settings.vitals.CurrentHealth += healthDifference;
                        settings.vitals.CurrentMana += manaDifference;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                }

                Server.getInstance.SendMaxHealth(worldPlayer, settings.vitals.MaxHealth);
                Server.getInstance.SendCurrentHealth(worldPlayer, settings.vitals.CurrentHealth);
                Server.getInstance.SendMaxMana(settings.vitals.MaxMana, client);
                Server.getInstance.SendCurrentMana(settings.vitals.CurrentMana, client);

                Stats _newStats = settings.stats;
                _newStats.PhysicalAttack += item.PhysicalAttack;
                _newStats.PhysicalAttack += item.MagicalAttack;

                _newStats.PhysicalAttack += item.AttackSpeed;
                _newStats.PhysicalAttack += item.CastingSpeed;

                _newStats.PhysicalAttack += item.PhysicalArmor;
                _newStats.PhysicalAttack += item.MagicalArmor;

                _newStats.PhysicalAttack += item.PhysicalCritical;
                _newStats.PhysicalAttack += item.MagicalCritical;

                _newStats.PhysicalAttack += item.Cooldown;

                _newStats.PhysicalAttack += item.MovementSpeed;

                _newStats.PhysicalAttack += item.HitChance;
                setStats(_newStats);
            }
        }

        public void UseItem(Item item)
        {
            if (castingSpell)
                return;

            bool itemFound = HasItem(item);

            if (!itemFound)
            {
                Server.getInstance.Log("Player " + settings.Name + " tried to use an item that he doesnt own!", DarkRift.LogType.Warning);
                return;
            }

            switch (item.Type)
            {
                case ItemType.Currency:
                    break;
                case ItemType.Equipment:
                    EquipItem(item);
                    break;
                case ItemType.Material:
                    break;
                case ItemType.Potion:
                    break;
                case ItemType.Quest:
                    break;
                default:
                    Server.getInstance.Log("Item type (" + item.Type + ") not found - item: " + item.Name, DarkRift.LogType.Warning);
                    break;
            }
        }
        void EquipItem(Item item)
        {
            if (castingSpell)
                return;

            if (!Database.getInstance.EquipItem(this, item))
                return;

            bool canRemoveItemFromInventory = RemoveItem(item, 1);
            if (!canRemoveItemFromInventory)
            {
                SendInfoMessage("Cant equip the item!");
                return;
            }

            if (equipment.ContainsKey(item.EquipmentLocation))
                UnEquipItem(item.EquipmentLocation);

            equipment.Add(item.EquipmentLocation, item);

            Server.getInstance.EquipItem(this, item, true);

            //calculate stats
            BaseStats newStats = settings.baseStats;
            newStats.Strength += item.Strength;
            newStats.Agility += item.Agility;
            newStats.Stamina += item.Stamina;
            newStats.Intelligence += item.Intelligence;
            newStats.Wit += item.Wit;
            newStats.Mental += item.Mental;
            setBaseStats(newStats);

            float healthDifference = 0f;
            float manaDifference = 0f;

            Vitals vitals = settings.vitals;
            vitals.MaxHealth += item.MaxHealth;
            vitals.MaxMana += item.MaxMana;

            healthDifference = vitals.MaxHealth - settings.vitals.MaxHealth;
            manaDifference = vitals.MaxMana - settings.vitals.MaxMana;

            settings.vitals.MaxHealth = vitals.MaxHealth;
            settings.vitals.MaxMana = vitals.MaxMana;
            if (!isDead)
            {
                if (healthDifference < 0 || manaDifference < 0)
                {
                    if (healthDifference < 0)
                    {
                        settings.vitals.CurrentHealth = settings.vitals.MaxHealth;
                        settings.vitals.CurrentMana = settings.vitals.MaxMana;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                    if (manaDifference < 0)
                    {
                    }
                    else
                    {
                        settings.vitals.CurrentHealth += healthDifference;
                        settings.vitals.CurrentMana += manaDifference;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                }
            }

            Server.getInstance.SendVitals(settings.vitals, client);

            Stats _newStats = settings.stats;
            _newStats.PhysicalAttack += item.PhysicalAttack;
            _newStats.PhysicalAttack += item.MagicalAttack;

            _newStats.PhysicalAttack += item.AttackSpeed;
            _newStats.PhysicalAttack += item.CastingSpeed;

            _newStats.PhysicalAttack += item.PhysicalArmor;
            _newStats.PhysicalAttack += item.MagicalArmor;

            _newStats.PhysicalAttack += item.PhysicalCritical;
            _newStats.PhysicalAttack += item.MagicalCritical;

            _newStats.PhysicalAttack += item.Cooldown;

            _newStats.PhysicalAttack += item.MovementSpeed;

            _newStats.PhysicalAttack += item.HitChance;
            setStats(_newStats);
        }
        public void UnEquipItem(EquipmentLocation slot)
        {
            if (castingSpell)
                return;

            if (!equipment.ContainsKey(slot))
                return;

            if (!Database.getInstance.UnEquipItem(this, slot))
                return;

            Item item = equipment[slot];
            bool canAddBackToInventory = AddItem(item, 1);
            if (!canAddBackToInventory)
            {
                SendInfoMessage("Cant add item back to inventory!");
                return;
            }

            Server.getInstance.EquipItem(this, item, false);

            //calculate stats
            BaseStats newStats = settings.baseStats;
            newStats.Strength -= item.Strength;
            newStats.Agility -= item.Agility;
            newStats.Stamina -= item.Stamina;
            newStats.Intelligence -= item.Intelligence;
            newStats.Wit -= item.Wit;
            newStats.Mental -= item.Mental;
            setBaseStats(newStats);

            float healthDifference = 0f;
            float manaDifference = 0f;

            Vitals vitals = settings.vitals;
            vitals.MaxHealth -= item.MaxHealth;
            vitals.MaxMana -= item.MaxMana;

            healthDifference = vitals.MaxHealth - settings.vitals.MaxHealth;
            manaDifference = vitals.MaxMana - settings.vitals.MaxMana;

            settings.vitals.MaxHealth = vitals.MaxHealth;
            settings.vitals.MaxMana = vitals.MaxMana;
            if (!isDead)
            {
                if (healthDifference < 0 || manaDifference < 0)
                {
                    if (healthDifference < 0)
                    {
                        settings.vitals.CurrentHealth = settings.vitals.MaxHealth;
                        settings.vitals.CurrentMana = settings.vitals.MaxMana;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                    if (manaDifference < 0)
                    {
                    }
                    else
                    {
                        settings.vitals.CurrentHealth += healthDifference;
                        settings.vitals.CurrentMana += manaDifference;

                        Mathf.Clamp(settings.vitals.CurrentHealth, 1f, settings.vitals.MaxHealth);
                        Mathf.Clamp(settings.vitals.CurrentMana, 1f, settings.vitals.MaxMana);
                    }
                }
            }

            Server.getInstance.SendVitals(settings.vitals, client);

            Stats _newStats = settings.stats;
            _newStats.PhysicalAttack -= item.PhysicalAttack;
            _newStats.PhysicalAttack -= item.MagicalAttack;

            _newStats.PhysicalAttack -= item.AttackSpeed;
            _newStats.PhysicalAttack -= item.CastingSpeed;

            _newStats.PhysicalAttack -= item.PhysicalArmor;
            _newStats.PhysicalAttack -= item.MagicalArmor;

            _newStats.PhysicalAttack -= item.PhysicalCritical;
            _newStats.PhysicalAttack -= item.MagicalCritical;

            _newStats.PhysicalAttack -= item.Cooldown;

            _newStats.PhysicalAttack -= item.MovementSpeed;

            _newStats.PhysicalAttack -= item.HitChance;
            setStats(_newStats);
        }

        public Item getWeapon()
        {
            if (equipment.ContainsKey(EquipmentLocation.Left_Hand))
                return equipment[EquipmentLocation.Left_Hand];

            return null;
        }
        public float getAttackRange()
        {
            if (equipment.ContainsKey(EquipmentLocation.Left_Hand))
                return equipment[EquipmentLocation.Left_Hand].Range;

            return baseAttackRange;
        }

        public bool HasSpell(Spell spell)
        {
            return HasSpell(spell.ID);
        }
        public bool HasSpell(int ID)
        {
            for (int i = 0; i < spells.Count; i++)
                if (spells[i].spell.ID == ID)
                    return true;

            return false;
        }
        public void AddSpell(Spell spell)
        {
            if (spell == null)
                return;

            if (HasSpell(spell.ID))
                return;

            if (Database.getInstance.addPlayerSpell(this, spell))
            {
                spells.Add(new PlayerSpell { spell = spell, currentCooldown = 0f });
                Server.getInstance.UpdateSpell(this, spell, SpellPacketUpdateType.Add);
            }

            if (spell.OperateType == SpellOperateType.Passive)
            {
            }
        }
        public void RemoveSpell(Spell spell)
        {
            if (spell == null)
                return;

            if (!HasSpell(spell.ID))
                return;

            if (Database.getInstance.removePlayerSpell(this, spell))
            {
                foreach (var i in spells)
                    if (i.spell.ID == spell.ID)
                        spells.Remove(i);
                Server.getInstance.UpdateSpell(this, spell, SpellPacketUpdateType.Remove);
            }
        }

        public void UseSpell(int ID)
        {
            UseSpell(SpellDatabase.getInstance.getSpell(ID));
        }
        public void UseSpell(Spell spell)
        {
            if (spell == null)
                return;

            if (isDead)
                return;

            if (castingSpell)
                return;

            bool done = false;

            if (spell.OperateType == SpellOperateType.Active)
            {
                switch (spell.CostType)
                {
                    case SpellCostType.Health:
                        if (settings.vitals.CurrentHealth >= spell.Cost)
                        {
                            settings.vitals.CurrentHealth -= spell.Cost;
                            done = true;
                        }
                        break;
                    case SpellCostType.Mana:
                        if (settings.vitals.CurrentMana >= spell.Cost)
                        {
                            settings.vitals.CurrentMana -= spell.Cost;
                            done = true;
                        }
                        break;
                }
            }
            if (!done)
                return;

            for (int i = 0; i < spells.Count; i++)
            {
                if (spells[i].spell.ID == spell.ID)
                    spells[i].currentCooldown = spell.Cooldown / settings.stats.Cooldown;
            }

            //use spell

            worldPlayer.CastSpell(spell);
        }

        public void UpdateCooldownTask()
        {
            for (int i = 0; i < spells.Count; i++)
            {
                if (spells[i].spell.OperateType == SpellOperateType.Active)
                    if (spells[i].currentCooldown > 0)
                        spells[i].currentCooldown -= 1;
            }
        }

        public List<PlayerSpell> getSpells() { return spells; }
    }
}