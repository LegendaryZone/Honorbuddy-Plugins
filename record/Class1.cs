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
        public override string Name { get { return "Record"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);
        private LocalPlayer Me { get { return StyxWoW.Me; } }
        bool ddd = false;
        public async Task doi()
        {
            ddd = true;
            while (true)
            {
                if (!TreeRoot.IsRunning){
					ddd=false;
                    return;
				}
                await Task.Run(async () =>
                {
                    Logging.Write(Styx.Common.LogLevel.Quiet, Colors.Aquamarine, string.Format("<Hotspot X=\"{0}\" Y=\"{1}\" Z=\"{2}\" />", Me.Location.X.ToString(), Me.Location.Y.ToString(), Me.Location.Z.ToString()));
                    await Task.Delay(1000);
                });
            }
        }

        public override void Pulse()
        {
            if (!ddd) doi();
        }
    }
}
