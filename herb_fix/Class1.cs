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
using System.IO;

namespace farm_stein
{
    public class Class1 : HBPlugin
    {
        public override string Name { get { return "Farm fixxer"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);
        private LocalPlayer Me { get { return StyxWoW.Me; } }

        public override void Pulse()
        {
            if (Me.IsBeingAttacked)
            {
                var target = ObjectManager.GetObjectsOfType<WoWUnit>().Where(u=>u.IsTargetingMeOrPet).OrderBy(u => u.Distance).FirstOrDefault();
                if (target != null)
                {
                    //Flightor.MountHelper.Dismount();
                    Styx.CommonBot.SpellManager.CastSpellById(768);
                    target.Target();
                    target.Face();
                    BotPoi.Current = new BotPoi(target, PoiType.Kill);
                }
            }
        }
    }
}
