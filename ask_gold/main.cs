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
        public override string Name { get { return "Ask for gold"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        private LocalPlayer Me { get { return StyxWoW.Me; } }
        List<string> already_invited = new List<string>();
        public override void Pulse()
        {
            List<WoWPlayer> PlayerList = ObjectManager.GetObjectsOfType<WoWPlayer>().Where(r => !r.IsDead && !r.IsAFKFlagged && !r.IsGM).OrderBy(r => r.Distance).ToList();
            //List<WoWPlayer> PlayerList = ObjectManager.GetObjectsOfType<WoWPlayer>().FindAll(obj => ( obj.IsPlayer && !already_invited.Contains(obj.Name)));
            foreach (WoWPlayer r in PlayerList)
            {
                if (!already_invited.Contains(r.Name))
                {
                    //r.Face();
                    //r.Target();
                    slog(r.Name);
                    already_invited.Add(r.Name);
                    string tmp = string.Format("hallo {0} wie geht es dir, ich hoffe gut! könntest du mir ein bisl helfen mit einem kleinem start kapital? 50g würden vollkommen reichen! wenn du mir helfen könntest schick mir bitte das gold per post zu , ich danke dir sehr!", r.Name);
                    sendWhisper(tmp, r.Name);
                    //getSign();
                    Thread.Sleep(300);
                }
            }
        }
        private void getSign()
        {
            //Lua.DoString("RunMacroText(\"/OfferPetition()\")");
            Lua.DoString("OfferPetition()");
        }
        private void sendWhisper(string msg, string user)
        {
            Lua.DoString(string.Format("SendChatMessage(\"{0}\", 'WHISPER', nil, \"{1}\");", msg, user));
        }
        private static void slog(string format, params object[] args) { Logging.Write(Colors.Green, "[Ask Gold]: " + format, args); }
    }
}