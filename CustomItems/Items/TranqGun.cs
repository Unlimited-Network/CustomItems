using System.Collections.Generic;
using CustomItems.API;
using Exiled.Events.EventArgs;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace CustomItems.Items
{
    public class TranqGun : CustomWeapon
    {
        public TranqGun(ItemType type, int clipSize, int itemId) : base(type, clipSize, itemId)
        {
        }

        public override string Name { get; set; } = "TG-119";

        public override string Description { get; set; } =
            "This modifier USP fires non-lethal tranquilizing darts. Those affected will be rendered unconcious for a short duration. Unreliable against SCPs. Repeated tranquilizations of the same person will render them resistant to it's effect.";

        protected override void LoadEvents()
        {
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            base.LoadEvents();
        }

        protected override void UnloadEvents()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            base.UnloadEvents();
        }

        private Dictionary<Player, int> TranquilizedPlayers = new Dictionary<Player, int>();

        private void OnHurting(HurtingEventArgs ev)
        {
            if (CheckItem(ev.Attacker.CurrentItem))
            {
                ev.Amount = 0;
                
                if (ev.Target.Team == Team.SCP && Plugin.Singleton.Config.ItemConfigs.TranqCfg.ResistantScps)
                    if (Plugin.Singleton.Rng.Next(100) <= Plugin.Singleton.Config.ItemConfigs.TranqCfg.ScpResistChance)
                        return;

                float dur = Plugin.Singleton.Config.ItemConfigs.TranqCfg.Duration;
                if (!TranquilizedPlayers.ContainsKey(ev.Target))
                    TranquilizedPlayers.Add(ev.Target, 0);
                
                dur -= (TranquilizedPlayers[ev.Target] * Plugin.Singleton.Config.ItemConfigs.TranqCfg.ResistanceModifier);
                
                if (dur > 0f)
                    Timing.RunCoroutine(DoTranquilize(ev.Target, dur));
            }
        }

        private IEnumerator<float> DoTranquilize(Player player, float duration)
        {
            Vector3 pos = player.Position;
            
            if (Plugin.Singleton.Config.ItemConfigs.TranqCfg.DropItems)
                player.DropItems();

            Map.SpawnRagdoll(player, DamageTypes.None, pos, allowRecall: false);
            player.Position = new Vector3(0, 0, 0);

            yield return Timing.WaitForSeconds(duration);

            player.Position = pos;
        }
    }
}