using System;

using Styx.Common;
using Styx.Plugins;
using Styx.WoWInternals;
using Styx.TreeSharp;
using Action = Styx.TreeSharp.Action;
using Styx.WoWInternals.WoWObjects;
using System.Linq;
using Styx.CommonBot.POI;
using Styx.CommonBot.Profiles;
using System.Threading.Tasks;

namespace Styx
{
    public class AntiDrown : HBPlugin
    {
        public override string Name { get { return "Loot"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);
        bool doing = false;
        public async Task doloot()
        {
            while (true)
            {
                await Task.Run(async () =>
                {
                    var me = StyxWoW.Me;
                    if (!me.IsBeingAttacked)
                    {
                        var target = ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.IsDead && !u.Looting && u.Lootable && u.CanLoot && (u.Distance <= 6)).OrderBy(u => u.Distance).FirstOrDefault();
                        if (target != null && target.CanLoot && target.Lootable)
                        {
                            //if (me.IsMoving)
                            //WoWMovement.MoveStop();
                            //BotPoi.Current = new BotPoi(target, PoiType.Loot);
                            target.Interact();
                            //await Task.Delay(100);
                        }
                    }
                    await Task.Delay(100);
                });
            }
        }

        public override void Pulse()
        {
            //if (!doing)
            //doloot();
            var me = StyxWoW.Me;
            var target = ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.IsDead && !u.Looting && u.Lootable && u.CanLoot && (u.Distance <= 10)).OrderBy(u => u.Distance).FirstOrDefault();
            if (target != null && target.CanLoot && target.Lootable)
            {
                //if (me.IsMoving)
                //WoWMovement.MoveStop();
                //BotPoi.Current = new BotPoi(target, PoiType.Loot);
                target.Interact();
                //await Task.Delay(100);
            }
        }
    }
}