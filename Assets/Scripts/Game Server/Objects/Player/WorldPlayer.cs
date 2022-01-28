using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static GameServer.World;

namespace GameServer
{
    public class WorldPlayer : IWorldObject
    {
        float walkSpeed = 6.0f;
        float jumpSpeed = 8.0f;
        float gravity = 20.0f;
        [SerializeField] float stoppingDinstance;

        CharacterController controller;

        public Player player { get; private set; }
        public Zone currentZone;

        Vector3 moveDirection = Vector3.zero;
        Vector3 previousPosition = Vector3.zero;
        Quaternion previousRotation = Quaternion.identity;
        byte previusAnimationSpeed = 0;
        float previousVertical = 0f, previousHorizontal = 0f;
        InputBuffer inputBuffer = new InputBuffer();

        [HideInInspector] public bool setup = false;
        [HideInInspector] public bool moveToTargetForAttack = false;

        public void SetUp(Player player)
        {
            this.player = player;

            controller = GetComponent<CharacterController>();
            type = ObjectType.Player;

            SetSpeed(player.getStats().MovementSpeed);

            Invoke("UpdateClientWithVitals", 5f);
            Invoke("UpdateClientSpellsCooldown", 5.5f);
        }

        public void SetSpeed(float speed)
        {
            walkSpeed = speed;
            jumpSpeed = speed + 2f;
            gravity = speed + 14f;
        }

        void Update()
        {
            if (!setup)
                return;

            Move();

            if (!player.isDead)
            {
                if (Utils_Maths.CloseEnough(previousPosition, transform.position, 0.01) || Utils_Maths.CloseEnough(previousRotation, transform.rotation, 0.01))
                {
                    Server.getInstance.PlayerMovement(this, transform.position, transform.rotation);

                    player.setPosition(transform.position);
                    player.setRotation(transform.rotation);
                    previousPosition = transform.position;
                    previousRotation = transform.rotation;
                    getInstance.UpdateZoneForPlayer(this);
                }
            }

            player.RegenerationTask();
            player.UpdateCooldownTask();
        }

        void Move()
        {
            if (player.isDead)
                return;

            if (player.castingSpell)
                return;

            _Input i = new _Input(0f, 0f);
            if (inputBuffer.hasInput)
                i = inputBuffer.Get();

            previousHorizontal = i.horizontal;
            previousVertical = i.vertical;

            if (i.horizontal == 0.0f && i.vertical == 0.0f)
                return;

            moveToTargetForAttack = false;

            float _speed = walkSpeed;

            if (controller.isGrounded)
            {
                moveDirection = new Vector3(i.horizontal, 0.0f, i.vertical);
                moveDirection *= _speed;
            }

            if (!controller.isGrounded)
                moveDirection.y -= gravity * Time.deltaTime;

            controller.Move(moveDirection * Time.deltaTime);
        }
        public void MoveWithmouse(Vector3 pos)
        {
            if (player.castingSpell)
                return;

            if (player.isDead)
                return;

            inputBuffer.Clear();

            moveToTargetForAttack = false;

            float px = transform.rotation.x;
            float pz = transform.rotation.z;
            transform.LookAt(pos);
            transform.rotation = Quaternion.Euler(px, transform.rotation.eulerAngles.y, pz);
            if (pos == transform.position)
                return;

            Vector3 moveDiff = pos - transform.position;
            Vector3 moveDir = moveDiff.normalized * 15f * Time.deltaTime;
            if (moveDiff.sqrMagnitude < moveDiff.sqrMagnitude)
                controller.Move(moveDir);
            else
                controller.Move(moveDiff);
        }
        public void Move(float horizontal, float vertical)
        {
            inputBuffer.Add(horizontal, vertical);
        }
        public void GoTo(Vector3 pos)
        {
            MoveWithmouse(pos);
        }
        public void Jump()
        {
            if (player.castingSpell)
                return;

            if (controller.isGrounded)
            {
                moveDirection.y = jumpSpeed;

                controller.Move(moveDirection * Time.deltaTime);
            }
        }

        public void DoDie()
        {
            player.SendInfoMessage("You have died!");
            StartCoroutine(respawn());
        }
        public IEnumerator respawn()
        {
            yield return new WaitForSeconds(5f);
            player.HealDamage(player.getSettings().vitals.MaxHealth);
            player.setPosition(Config.StartingPosition);
            player.setRotation(Config.StartingRotation);
            player.SendInfoMessage("You have been respawned!");
        }

        public void Attack(WorldPlayer player)
        {
            float weapon_range = this.player.getAttackRange();

            if (Vector3.Distance(transform.position, player.transform.position) > weapon_range)
            {
                StopCoroutine(moveToTarget(player, weapon_range));
                StartCoroutine(moveToTarget(player, weapon_range));
            }
            else
            {
                //attack
            }
        }
        public void Attack(WorldNPC npc)
        {
            if (npc.isDead)
                return;

            float weapon_range = player.getAttackRange();

            if (Vector3.Distance(transform.position, npc.transform.position) > weapon_range)
            {
                StopCoroutine(moveToTarget(npc, weapon_range));
                StartCoroutine(moveToTarget(npc, weapon_range));
            }
            else
            {
                //attack
            }
        }

        IEnumerator moveToTarget(WorldNPC target, float range)
        {
            moveToTargetForAttack = true;
            bool reached = false;

            while (moveToTargetForAttack)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, player.speed);

                if (Vector3.Distance(transform.position, target.transform.position) > range)
                {
                    reached = false;
                    moveToTargetForAttack = false;
                }

                yield return null;

            }

            if (reached)
                Attack(target);

            yield return null;
        }
        IEnumerator moveToTarget(WorldPlayer target, float range)
        {
            moveToTargetForAttack = true;
            bool reached = false;

            while (moveToTargetForAttack)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, player.speed);

                if (Vector3.Distance(transform.position, target.transform.position) > range)
                {
                    reached = false;
                    moveToTargetForAttack = false;
                }

                yield return null;

            }

            if (reached)
                Attack(target);

            yield return null;
        }

        void UpdateClientWithVitals()
        {
            Server.getInstance.SendMaxHealth(this, player.getSettings().vitals.MaxHealth);
            Server.getInstance.SendCurrentHealth(this, player.getSettings().vitals.CurrentHealth);
            Server.getInstance.SendMaxMana(player.getSettings().vitals.MaxMana, player.getClient());
            Server.getInstance.SendCurrentMana(player.getSettings().vitals.CurrentMana, player.getClient());

            Invoke("UpdateClientWithVitals", 5f);
        }
        void UpdateClientSpellsCooldown()
        {
            foreach (var i in player.getSpells())
                if (i.spell.OperateType == SpellOperateType.Active)
                    if (i.currentCooldown != 0f)
                        Server.getInstance.UpdateSpell(player, i.spell, SpellPacketUpdateType.UpdateCooldown, i.currentCooldown);

            Invoke("UpdateClientSpellsCooldown", 5.5f);
        }

        public void CastSpell(Spell spell)
        {
            if (player.castingSpell)
                return;

            if (spell == null)
                return;

            if (player.isDead)
                return;

            StartCoroutine(castSpell(spell));
        }
        IEnumerator castSpell(Spell spell)
        {
            player.castingSpell = true;
            yield return new WaitForSeconds(spell.CastDuration);

            if (player.targetType == Player.SelectedObjectType.Player)
                Server.getInstance.UseSpell(player, spell, player.playerTarget.transform.position);
            else if (player.targetType == Player.SelectedObjectType.NPC)
                Server.getInstance.UseSpell(player, spell, player.npcTarget.transform.position);

            player.castingSpell = false;
        }
    }
}