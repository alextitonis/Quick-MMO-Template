using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemDatabase;

public class Player : IWorldObject
{
    enum SelectedObjectType
    {
        None = -1,
        Player = 0,
        NPC = 1,
    }
    
    public static float STR_MOD = 5;
    public static float AGI_MOD = 5;
    public static float STA_MOD = 5;
    public static float INT_MOD = 5;
    public static float WIT_MOD = 5;
    public static float MEN_MOD = 5;

    public static float MAX_MOVEMENT_SPEED = 10;

    [HideInInspector] public ushort ClientID;
    [HideInInspector] public string CharName;
    [HideInInspector] public Vector3 CharPosition;
    [HideInInspector] public Quaternion CharRotation;
    [HideInInspector] public Races race;
    [HideInInspector] public Classes _class;
    [HideInInspector] public Genders gender;
    [HideInInspector] public Vitals vitals = new Vitals();
    [HideInInspector] public BaseStats baseStats;
    [HideInInspector] public Stats stats;
    [HideInInspector] public long level;

    [SerializeField] Animator anim;
    [SerializeField] GameObject clickEffect;
    [SerializeField] float timeToDestroyClickEffect;
    [SerializeField] SkinnedMeshRenderer equipingMesh;

    Camera cam;

    SelectedObjectType selectedObjectType = SelectedObjectType.None;
    GameObject selectedObject = null;

    List<InventoryItem> inventory = new List<InventoryItem>();
    Dictionary<EquipmentLocation, Item> equipment = new Dictionary<EquipmentLocation, Item>();
    Dictionary<EquipmentLocation, SkinnedMeshRenderer> equipmentObjects = new Dictionary<EquipmentLocation, SkinnedMeshRenderer>();
    List<PlayerSpell> spells = new List<PlayerSpell>();

    Vector3 previousMousePosition = Vector3.zero;

    public bool castingSpell = false;

    bool isDead { get { return vitals.CurrentHealth <= 0; } }

    public void SetUp(ushort ClientID, string CharName, Races race, Classes _class, Genders gender, Vector3 CharPosition, Quaternion CharRotation, float MaxHealth, float CurrentHealth)
    {
        this.ClientID = ClientID;
        this.CharName = CharName;
        this.CharPosition = CharPosition;
        this.CharRotation = CharRotation;
        this.race = race;
        this._class = _class;
        this.gender = gender;

        type = ObjectType.Player;
        vitals.MaxHealth = MaxHealth;
        vitals.CurrentHealth = CurrentHealth;

        cam = Camera.main;

        if (isLocal())
        {
            cam.GetComponent<CameraController>().SetTarget(gameObject);
        }
    }

    public bool isLocal() { return ClientID == Client.localID; }

    public void SetMaxHealth(float value)
    {
        vitals.MaxHealth = value;
    }
    public void SetCurrentHealth(float value)
    {
        vitals.CurrentHealth = value;
    }
    public void SetMaxMana(float value)
    {
        vitals.MaxMana = value;
    }
    public void SetCurrentMana(float value)
    {
        vitals.CurrentMana = value;
    }

    void Update()
    {
        if (!isLocal())
            return;

        if (Input.GetKeyDown(KeyCode.Return))
            ChatManager.getInstance.Enter();

        if (InputManager.getInstance.Horizontal != 0.0f || InputManager.getInstance.Vertical != 0.0f)
            if (!castingSpell)
                Client.getInstance.PlayerMovement(InputManager.getInstance.Horizontal, InputManager.getInstance.Vertical);

        if (Input.GetKeyDown(KeyCode.Space))
            Client.getInstance.Jump();

        if (Input.GetKeyDown(KeyCode.Tab))
            PlayerStatsManager.getInstance.UdpateUI(this);

        if (Input.GetKeyDown(KeyCode.I))
            InventoryManager.getInstance.UpdateUI(this);

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 pos = hit.point;

                if (hit.collider.tag == "Map")
                {
                    GameObject go = Instantiate(clickEffect, hit.point, Quaternion.identity);
                    Destroy(go, timeToDestroyClickEffect);

                    if (previousMousePosition != pos)
                    {
                        Client.getInstance.MoveWithMouse(pos);
                        previousMousePosition = pos;
                    }
                }
                else if (hit.collider.tag == "Player")
                {
                    selectedObject = hit.collider.gameObject;
                    if (hit.collider.GetComponent<IWorldObject>().type == ObjectType.Player)
                    {
                        print("player");
                        selectedObjectType = SelectedObjectType.Player;
                        Player p = selectedObject.GetComponent<Player>();
                        SelectedObjectManager.getInstance.OpenAsPlayer(p);
                        Client.getInstance.SetTargetPlayer(p.ClientID);
                    }
                    else
                    {
                        print("npc");
                        selectedObjectType = SelectedObjectType.NPC;
                        WorldNPC npc = selectedObject.GetComponent<WorldNPC>();
                        SelectedObjectManager.getInstance.OpenAsNPC(npc);
                        print(npc.spawnID);
                        Client.getInstance.SetTargetNPC(npc.spawnID);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (selectedObject != null)
            {
                selectedObject = null;
                selectedObjectType = SelectedObjectType.None;
                SelectedObjectManager.getInstance.Close();
                Client.getInstance.SetTargetNPC(-1);
            }
            // else open menu
        }

        RegenerationTask();
        UpdateCooldownTask();
    }

    public void SendAnimation(AnimationType type, string animationName, string value)
    {
        Client.getInstance.PlayerAnimation(type, animationName, value);
    }
    public void SendPlayAnimation(string anim)
    {
        Client.getInstance.SendPlayAnimation(anim);
    }
    public void ApplyAnimation(AnimationType type, string animationName, string value)
    {
        if (anim == null)
            return;

        if (type == AnimationType.Bool)
        {
            bool _value = false;
            bool parsed = bool.TryParse(value, out _value);

            if (parsed)
                anim.SetBool(animationName, _value);
        }
        else if (type == AnimationType.Float)
        {
            float _value = 0f;
            bool parsed = float.TryParse(value, out _value);

            if (parsed)
                anim.SetFloat(animationName, _value);
        }
        else if (type == AnimationType.Integer)
        {
            int _value = 0;
            bool parsed = int.TryParse(value, out _value);

            if (parsed)
                anim.SetInteger(animationName, _value);
        }
        else if (type == AnimationType.Trigger)
        {
            bool isOn = false;
            if (value == "1") isOn = true;

            if (isOn)
                anim.SetTrigger(animationName);
            else
                anim.ResetTrigger(animationName);
        }
    }
    public void PlayAnimation(string anim)
    {
        if (anim == null)
            return;

        this.anim.Play(anim);
    }

    public void AddItem(InventoryItem item)
    {
        if (inventory.Find(x => (x.item == item.item) && (x.slot == item.slot)) != null)
        {
            InventoryItem _item = inventory.Find(x => (x.item == item.item) && (x.slot == item.slot));
            _item.quantity = item.quantity;
        }
        else
            inventory.Add(item);

        if (InventoryManager.getInstance.isActive)
            InventoryManager.getInstance.SetUp(inventory, this);
    }
    public void RemoveItem(int slot, bool fromServer = true)
    {
        if (fromServer) inventory.RemoveAt(inventory.FindIndex(x => x.slot == slot));
        else Client.getInstance.RemoveItem(slot);

        if (fromServer)
            if (InventoryManager.getInstance.isActive)
                InventoryManager.getInstance.SetUp(inventory, this);
    }

    public List<InventoryItem> getInventory() { return inventory; }
    public Dictionary<EquipmentLocation, Item> getEquipment() { return equipment; }

    public void Equip(Item item)
    {
        if (castingSpell)
            return;

        if (item == null)
            return;

        if (equipment.ContainsKey(item.EquipmentLocation))
            UnEquip(item.EquipmentLocation);

        equipment.Add(item.EquipmentLocation, item);

        SkinnedMeshRenderer newMesh = Instantiate(item.EquipmentMesh);
        newMesh.transform.parent = equipingMesh.transform;

        newMesh.bones = equipingMesh.bones;
        newMesh.rootBone = equipingMesh.rootBone;
        equipmentObjects.Add(item.EquipmentLocation, newMesh);
        SetEquipmentWeight(item, 100);

        if (EquipmentManager.getInstance.isActive)
            EquipmentManager.getInstance.SetUp(equipment, this);
    }
    public void UnEquip(EquipmentLocation slot)
    {
        if (castingSpell)
            return;

        if (equipment.ContainsKey(slot))
        {
            Item item = equipment[slot];

            equipment.Remove(slot);

            SetEquipmentWeight(item, 0);
            Destroy(equipmentObjects[slot]);
            equipmentObjects.Remove(slot);

            if (EquipmentManager.getInstance.isActive)
                EquipmentManager.getInstance.SetUp(equipment, this);
        }
    }
    void SetEquipmentWeight(Item item, int weight)
    {
        foreach (var i in item.CoveredRegions)
            equipingMesh.SetBlendShapeWeight((int)i, weight);
    }

    public void UseSpell(Spell spell, bool fromServer, Vector3 target)
    {
        switch (fromServer)
        {
            case true:
                if (target == null)
                    return;

                switch (spell.CostType)
                {
                    case SpellCostType.Health:
                        vitals.CurrentHealth -= spell.Cost;
                        break;
                    case SpellCostType.Mana:
                        vitals.CurrentMana -= spell.Cost;
                        break;
                }

                UpdateCooldown(spell, spell.Cooldown / stats.Cooldown);

                //spawn effect and guide it to target
                break;
            case false:
                if (!isLocal())
                    return;

                if (castingSpell)
                    return;

                if (isDead)
                    return;


                if (!HasSpell(spell))
                    return;
                foreach (var i in spells)
                    if (i.spell.ID == spell.ID)
                        if (i.currentCooldown > 0)
                            return;

                if (spell.CostType == SpellCostType.Health && vitals.CurrentHealth <= spell.Cost)
                    return;
                else if (spell.CostType == SpellCostType.Mana && vitals.CurrentMana <= spell.Cost)
                    return;

                if (spell.OperateType == SpellOperateType.Active)
                {
                    if (spell.Target == SpellTarget.Target && selectedObject == null)
                        return;

                    Client.getInstance.UseSpell(spell);
                    StartCoroutine(castSpell(spell));
                }
                break;
        }
    }
    IEnumerator castSpell(Spell spell)
    {
        castingSpell = true;
        //show animation and cast time
        yield return new WaitForSeconds(spell.CastDuration + 1f);
        castingSpell = false;
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
    public void AddSpell(Spell spell, float currentCooldown)
    {
        if (HasSpell(spell.ID))
            return;

        spells.Add(new PlayerSpell { spell = spell, currentCooldown = currentCooldown });
    }
    public void RemoveSpell(Spell spell)
    {
        if (!HasSpell(spell.ID))
            return;

        foreach (var i in spells)
            if (i.spell.ID == spell.ID)
                spells.Remove(i);
    }
    public void UpdateCooldown(Spell spell, float currentCooldown)
    {
        if (!HasSpell(spell))
            return;

        for (int i = 0; i < spells.Count; i++)
            if (spells[i].spell.ID == spell.ID)
                spells[i].currentCooldown = currentCooldown;
    }

    void UpdateCooldownTask()
    {
        for (int i = 0; i < spells.Count; i++)
        {
            spells[i].currentCooldown -= 1;
            if (spells[i].currentCooldown <= 0)
                spells[i].currentCooldown = 0;
        }
    }

    public void RegenerationTask()
    {
        if (!isDead)
        {
            float healthRegeneration = 0f;
            float manaRegeneration = 0f;

            healthRegeneration = baseStats.Stamina * STA_MOD / 100;
            manaRegeneration = baseStats.Mental * MEN_MOD / 100;

            vitals.CurrentHealth += healthRegeneration;
            vitals.CurrentMana += manaRegeneration;

            vitals.CurrentHealth = Mathf.Clamp(vitals.CurrentHealth, 0, vitals.MaxHealth);
            vitals.CurrentMana = Mathf.Clamp(vitals.CurrentMana, 0, vitals.MaxMana);
        }
    }
}