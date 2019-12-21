using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx.Common;
using Styx.CommonBot.Inventory;
using Styx.Helpers;
using Styx.Pathing;
using Styx.Plugins;
using Styx.WoWInternals.Misc;
using System.Threading;
using Styx.WoWInternals.WoWObjects;
using Styx.WoWInternals;
using Styx;
using System.Windows.Media;

namespace make_guild
{
    public class make_guild : HBPlugin
    {
        public override string Name { get { return "Make Guild"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        private LocalPlayer Me { get { return StyxWoW.Me; } }
        List<string> already_invited = new List<string>();
        public override void Pulse()
        {
            List<WoWPlayer> PlayerList = ObjectManager.GetObjectsOfType<WoWPlayer>().Where(r => !r.IsDead).OrderBy(r => r.Distance).ToList();
            //List<WoWPlayer> PlayerList = ObjectManager.GetObjectsOfType<WoWPlayer>().FindAll(obj => ( obj.IsPlayer && !already_invited.Contains(obj.Name)));
            foreach (WoWPlayer r in PlayerList)
            {
                if (!already_invited.Contains(r.Name))
                {
                    r.Face();
                    r.Target();
                    slog(r.Name);
                    already_invited.Add(r.Name);
                    string tmp = string.Format("hallo {0} kannst du mir helfen meine gilde aufzumachen ?",r.Name);
                    //sendWhisper(tmp, r.Name);
                    getSign();
                    Thread.Sleep(300);
                }
            }
        }
        private void getSign()
        {
            //Lua.DoString("RunMacroText(\"/OfferPetition()\")");
            Lua.DoString("OfferPetition()");
            //Thread.Sleep(300);
        }
        private void sendWhisper(string msg,string user)
        {
            Lua.DoString(string.Format("SendChatMessage(\"{0}\", 'WHISPER', nil, \"{1}\");", msg, user));
        }
        private static void slog(string format, params object[] args) { Logging.Write(Colors.Green, "[Make Guild]: " + format, args); }
    }
}