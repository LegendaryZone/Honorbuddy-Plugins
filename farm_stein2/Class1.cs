using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx.Plugins;
using Styx.Common;
using System.Windows.Media;
using Styx.WoWInternals.WoWObjects;
using Styx.WoWInternals;
using Styx.Pathing;
using Styx.CommonBot.POI;
using Styx;
using Styx.CommonBot;
using System.Threading;

namespace farm_stein
{
    public class Class1 : HBPlugin
    {
        public override string Name { get { return "Farm Stein"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);
        private LocalPlayer Me { get { return StyxWoW.Me; } }

        bool doing = false;
        bool running = false;

        public WoWUnit findMob()
        {
            return ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.FactionId == 834 && u.IsAlive && u.Attackable && !u.TaggedByOther).OrderBy(u => u.Distance).FirstOrDefault();
        }

        public async Task killTarget()
        {
            WoWUnit target = findMob();
            if (target != null && target.IsAlive && target.IsValid && target.Attackable && target.InLineOfSight)
            {
                await Task.Run(async () =>
                {
                    if (target.IsAlive && target.InLineOfSight && target.InLineOfSpellSight && (target.Distance <= 30))
                    {
                        target.Target();
                        Flightor.MountHelper.Dismount();
                        SpellManager.Cast("Moonfire");
                        await Task.Delay(200);
                        Flightor.MountHelper.MountUp();
                        //Mount.SummonMount();
                    }
                });
            }
        }

        public async Task there()
        {
            running = true;
            WoWUnit t = findMob();
            if (t != null)
            {
                while (t.Distance >= 40)
                {
                    await Task.Run(async () =>
                    {
                        Navigator.MoveTo(t.Location);
                        await Task.Delay(100);
                    });
                }
            }
            running = false;
        }

        public override void Pulse()
        {
            if(!running)
            there();
            if (!Me.IsFlying)
                killTarget();
        }
        private void Log(string v)
        {
            Logging.Write(Styx.Common.LogLevel.Normal, Colors.Aquamarine, string.Format("[{0}]: {1}", Name, v));
        }
    }
}
