using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx.Plugins;
using Styx.WoWInternals.WoWObjects;
using Styx;
using Styx.WoWInternals;
using Common.Logging;
using System.Windows.Media;
using Styx.Common;

namespace get_mail_post
{
    public class get_mail_post : HBPlugin
    {
        public override string Name { get { return "Mail Bot"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        private LocalPlayer Me { get { return StyxWoW.Me; } }

        bool doingWork = false;

        public override void Pulse()
        {
            if (!doingWork)
                doWork();
        }
        public async Task doWork()
        {
            doingWork = true;
            await getMail();
        }
        public async Task getMail()
        {
            Log("getMail called");
            WoWGameObject mailbox = ObjectManager.GetObjectsOfType<WoWGameObject>().Where(u => u.SubType == WoWGameObjectType.Mailbox).OrderBy(u => u.Distance).FirstOrDefault();
            Log("moving to mailbox");
            bool isWithin = mailbox.WithinInteractRange;
            while (!isWithin)
            {
                if (Me.IsMoving && mailbox.Distance <= 3.8)
                    WoWMovement.MoveStop();
                WoWMovement.ClickToMove(mailbox.Location);
                await Task.Run(async () =>
                {
                    await Task.Delay(300);
                });
                isWithin = mailbox.WithinInteractRange;
            }
            Log("sind am kasten");
            mailbox.Interact();
            await Task.Run(async () =>
            {
                await Task.Delay(400);
            });
            bool hasMail = true;
            while (hasMail)
            {
                if(Lua.GetReturnVal<bool>("return MILA_INBOX:shouldReload()", 0))
                {
                    Lua.DoString("ConsoleExec(\"reloadui\")");
                    await Task.Run(async () =>
                    {
                        await Task.Delay(6000);
                    });
                }
                if (!Styx.CommonBot.Frames.MailFrame.Instance.IsVisible)
                {
                    mailbox.Interact();
                }
                Lua.DoString("MILA_INBOX:getAll()");
                await Task.Run(async () =>
                {
                    await Task.Delay(1000);
                });
                hasMail = Lua.GetReturnVal<bool>("return MILA_INBOX:hasMail()", 0);
            }
            Log("fertig mit post abholen");
            Styx.CommonBot.Frames.MailFrame.Instance.Close();
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
    }
}
