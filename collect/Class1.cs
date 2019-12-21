using Action = Styx.TreeSharp.Action;
using Styx.Common;
using Styx.CommonBot.POI;
using Styx.CommonBot;
using Styx.Pathing;
using Styx.Plugins;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using Styx.WoWInternals;
using Styx;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace Styx
{
    public class AntiDrown : HBPlugin
    {
        public override string Name { get { return "Resource Collector"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);
        private LocalPlayer Me { get { return StyxWoW.Me; } }
        bool waitingFor = false;
        bool hasLeader = false;
        bool isFollowing = false;
        WoWPlayer leader;
        List<WoWGuid> alreadyUsed = new List<WoWGuid>();

        public WoWObject findRes()
        {
            return ObjectManager.GetObjectsOfType<WoWGameObject>().Where(u => u.IsValid && u.Distance <= 10 && u.IsHerb || u.IsMineral).OrderBy(u => u.Distance).FirstOrDefault();
        }
        public override void Pulse()
        {
      
            Bots.Gatherbuddy.GatherbuddySettings.Instance.NoNinja = false;
            Bots.Gatherbuddy.GatherbuddySettings.Instance.DiagnosticLogging  = true;
            Bots.Gatherbuddy.GatherbuddySettings.Instance.GatherHerbs = true;
            Bots.Gatherbuddy.GatherbuddySettings.Instance.GatherMinerals  = true;
            Bots.Gatherbuddy.GatherbuddySettings.Instance.FaceNodes  = true;
            Bots.Gatherbuddy.GatherbuddySettings.Instance.IgnoreElites  = false;
            //WoWObject tmp = findRes();
            //tmp.Interact();
            /*
            WoWObject tmp = findRes();
            if (Me.IsGroupLeader) {
                Log(tmp.Distance.ToString());
                if (tmp.Distance <= 6)
                {
                    if (!(PartyMemberNear(5f)))
                    {
                        //Lua.DoString("JumpOrAscendStart()");
                        WoWMovement.ClickToMove(tmp.Location);
                    }
                }
            }
            else
            {
                if (tmp != null)
                {
                    //Log(string.Format("{0} name", tmp.Name.ToString()));
                    workWithRes(tmp);
                }
            }*/
        }

        public async Task workWithRes(WoWObject obj)
        {
            /*if (Me.IsGroupLeader)
            {
                if (!(PartyMemberNear(obj.InteractRange)) && (obj.Distance <= 6))
                    //Navigator.MoveTo(obj.Location);
                    WoWMovement.ClickToMove(obj.Location);
            }
            else
            {*/
            if (obj.WithinInteractRange)
            {
                Log("sind in der nähe");
                if (PartyMemberNear(obj.InteractRange))
                {
                    Log("alle members hier");
                    obj.Interact();
                }
            }
            else
            {
                Log("nicht in der nähe vom stoff");
                Navigator.MoveTo(obj.Location);
                //WoWMovement.ClickToMove(obj.Location);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Log("Plugin enabled");
        }
        public override void OnDisable()
        {
            base.OnDisable();
            Log("Plugin disabled");
        }
        private void Log(string v)
        {
            Logging.Write(Styx.Common.LogLevel.Normal, Colors.Aquamarine, string.Format("[{0}]: {1}", Name, v));
        }
        private bool PartyMemberNear(float distance)
        {
            if (!Me.GroupInfo.IsInParty)
                return false;
            int memberswithinRange = Me.PartyMembers.Count(player => player != null && Me.Location.Distance(player.Location) <= distance);
            if (memberswithinRange == Me.GroupInfo.PartySize && memberswithinRange != 0)
                return true;
            return false;
        }
        /*
                                  public async Task findLeader()
        {
            if (!hasLeader)
            {
                var myparty = Me.PartyMembers;
                foreach (var i in myparty)
                {
                    if (i.IsGroupLeader)
                    {
                        leader = i;
                        hasLeader = true;
                    }
                }
            }
        }
          
public async Task followLeader()
{
    isFollowing = true;
    //bool isNear = (leader.Distance <= 5);
    while (true)
    {
        Navigator.MoveTo(leader.Location);
        await Task.Run(async () =>
        {
            await Task.Delay(100);
        });
        //isNear = (leader.Distance <= 5);
    }
}
public async Task workWithRes(WoWObject obj)
{
    bool isNearMe = (obj.Distance <= 4);
    if (isNearMe)
    {
        if (PartyMemberNear(4))
        {
            Log("sammeln es");
            obj.Interact();
            alreadyUsed.Add(obj.Guid);
        }
    }
    else
    {
        Navigator.MoveTo(obj.Location);
    }
    Log("ende von Work");
}*/
    }
}
