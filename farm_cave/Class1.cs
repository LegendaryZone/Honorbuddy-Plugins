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

namespace farm_stein
{
    public class Class1 : HBPlugin
    {
        public override string Name { get { return "Farm Cave"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);
        private LocalPlayer Me { get { return StyxWoW.Me; } }

        bool doing = false;

        public WoWUnit findMob()
        {
            return ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.Entry == 40959 && u.IsAlive && u.Attackable && u.InLineOfSight).OrderBy(u => u.Distance).FirstOrDefault();
        }

        public async Task killTarget()
        {
            WoWUnit target = findMob();
            if (target != null && target.IsAlive && target.IsValid && target.Attackable && target.InLineOfSight)
            {
                await Task.Run(async () =>
                {
                    if (target.IsAlive && target.InLineOfSight && target.InLineOfSpellSight && (target.Distance <=30))
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

        public override void Pulse()
        {
            killTarget();
        }
        private void Log(string v)
        {
            Logging.Write(Styx.Common.LogLevel.Normal, Colors.Aquamarine, string.Format("[{0}]: {1}", Name, v));
        }
    }
}
