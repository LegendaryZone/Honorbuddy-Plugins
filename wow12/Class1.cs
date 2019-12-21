using System;

using Styx.Common;
using Styx.Plugins;
using Styx.WoWInternals;
using Styx.TreeSharp;
using Action = Styx.TreeSharp.Action;
using Styx.WoWInternals.WoWObjects;
using System.Linq;
using Styx.CommonBot.POI;

namespace Styx
{
    public class AntiDrown : HBPlugin
    {
        public override string Name { get { return "Use WoW 12 Boost"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);

        public override void Pulse()
        {
            var me = StyxWoW.Me;
            if (!me.HasAura(219159))
            {
                var item = StyxWoW.Me.BagItems.FirstOrDefault(i => i.Entry == 139285);
                item.Use();
            }
        }
    }
}
