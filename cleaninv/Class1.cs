using Styx.Common;
using Styx.CommonBot.Inventory;
using Styx.Helpers;
using Styx.Pathing;
using Styx.Plugins;
using Styx.WoWInternals.Misc;
using Styx.WoWInternals.WoWObjects;
using Styx.WoWInternals;
using Styx;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System;
using Newtonsoft.Json.Linq;
using Styx.CommonBot;
using Styx.WoWInternals.DB;
using System.Numerics;
using System.Timers;

namespace ah
{
    public class AH : HBPlugin
    {
        public override string Name { get { return "Clean Inv"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        private LocalPlayer Me { get { return StyxWoW.Me; } }
        System.Timers.Timer loginTimer;

        List<uint> protectedItems = new List<uint> { 6390, 6391 , 8491, 14175, 6948, 22576, 22457, 34535,4306, 8499,4632};

        public override void Pulse()
        {

        }
        private void Log(string v)
        {
            Logging.Write(LogLevel.Normal, Colors.Aquamarine, string.Format("[{0}]: {1}",Name, v));
        }

        public async Task deleteItems()
        {
            foreach (var i in Me.BagItems)
            {
                if (protectedItems.Contains(i.Entry))
                {
                    continue;
                }
                if (i.Quality == WoWItemQuality.Poor || i.Quality == WoWItemQuality.Common || i.Quality == WoWItemQuality.Uncommon || i.IsSoulbound)
                {
                    Lua.DoString("ClearCursor()");
                    i.PickUp();
                    Lua.DoString("DeleteCursorItem()");
                    await Task.Run(async () =>
                    {
                        await Task.Delay(100);
                    });
                }
            }
        }

        private void checkIfLoggedIn(object sender, ElapsedEventArgs e)
        {
            deleteItems();
        }

        #region OnEnable
        public override void OnEnable()
        {
            loginTimer = new System.Timers.Timer(60 * 1000);
            loginTimer.Elapsed += new ElapsedEventHandler(checkIfLoggedIn);
            loginTimer.Start();
        }
        #endregion
        #region OnDisable
        public override void OnDisable()
        {
            loginTimer.Stop();
        }
        #endregion
    }
}