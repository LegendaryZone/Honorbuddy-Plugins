using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx.Plugins;
using Styx.WoWInternals.WoWObjects;
using Styx;
using System.Windows.Media;
using Styx.Common;
using Styx.WoWInternals;
using Styx.Pathing;
using Styx.Helpers;
using System.Timers;
using Styx.CommonBot;

namespace ah_only
{
    public class Class1 : HBPlugin
    {
        #region plugin
        public override string Author
        {
            get
            {
                return "Mila432";
            }
        }
        public override string Name
        {
            get
            {
                return "AH Only";
            }
        }
        public override Version Version
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Log("enabled");
            reloadtimer = new System.Timers.Timer(60 * 10 * 1000); //one hour in milliseconds
            reloadtimer.Elapsed += new ElapsedEventHandler(checkrelog);
            reloadtimer.Start();

            checkmail = new System.Timers.Timer(60 * 5 * 1000); //one hour in milliseconds
            checkmail.Elapsed += new ElapsedEventHandler(checkmails);
            //checkmail.Start();
        }
        private void checkmails(object sender, ElapsedEventArgs e)
        {
            mail();
        }
        private void checkrelog(object sender, ElapsedEventArgs e)
        {
            if (TreeRoot.IsRunning && !m && !p)
                doReload();
        }
        public override void OnDisable()
        {
            base.OnDisable();
            Log("disbaled");
            reloadtimer.Dispose();
            checkmail.Dispose();
        }
        #endregion
        private LocalPlayer Me { get { return StyxWoW.Me; } }
        private bool snipe = false;
        private bool p = false;
        private bool m = false;
        System.Timers.Timer reloadtimer;
        System.Timers.Timer checkmail;
        public override void Pulse()
        {
            if (!m && !p)
            {
                ah();
                startsnipe();
                checkauction();
            }
            //mail();
        }

        private async Task post()
        {
            if (!p)
            {
                p = true;
                ah();
                while (!Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible)
                {
                    await Task.Run(async () =>
                    {
                        WoWUnit auc = findAuctioner();
                        auc.Interact();
                        await Task.Delay(200);
                    });
                }
                if (Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible)
                {
                    await Task.Run(async () =>
                    {
                        Lua.DoString("AuctionFrameTab5:Click()");
                        await Task.Delay(200);
                        Lua.DoString("RunMacroText(\"/click postBtn\")");
                        bool doneScanning = Lua.GetReturnVal<bool>("return MILA_POSTSCAN:doneScanning()", 0);
                        while (!doneScanning)
                        {
                            await Task.Run(async () =>
                            {
                                await Task.Delay(200);
                                doneScanning = Lua.GetReturnVal<bool>("return MILA_POSTSCAN:doneScanning()", 0);
                            });
                        }
                        if (Lua.GetReturnVal<bool>("return MILA_MANAGE:hasItemsToSell()", 0))
                        {
                            Log("bad hasItemsToSell");
                            Lua.DoString("MILA_MANAGE:resetDonePosting()");
                            Lua.DoString("MILA_POSTSCAN:resetScanning()");
                            Styx.CommonBot.Frames.AuctionFrame.Instance.Close();
                            return;
                        }
                        bool donePosting = Lua.GetReturnVal<bool>("return MILA_MANAGE:donePosting()", 0);
                        while (!donePosting)
                        {
                            await Task.Run(async () =>
                            {
                                KeyboardManager.PressKey('4');
                                KeyboardManager.ReleaseKey('4');
                                await Task.Delay(100);
                                donePosting = Lua.GetReturnVal<bool>("return MILA_MANAGE:donePosting()", 0);
                                Log(donePosting.ToString());
                            });
                        }
                        Log("fertig mit posten");
                        Lua.DoString("MILA_MANAGE:resetDonePosting()");
                        Lua.DoString("MILA_POSTSCAN:resetScanning()");
                        Styx.CommonBot.Frames.AuctionFrame.Instance.Close();
                        p = false;
                        m = false;
                    });
                }
            }
        }
        private async Task mail()
        {
            if (!m)
            {
                m = true;
                WoWGameObject mail = findMailbox();
                while (!mail.WithinInteractRange)
                {
                    await Task.Run(async () =>
                    {
                        //Navigator.MoveTo(auc.Location);
                        WoWMovement.ClickToMove(mail.Location);
                        await Task.Delay(200);
                    });
                }
                WoWMovement.MoveStop();
                mail.Interact();
                await Task.Run(async () =>
                {
                    await Task.Delay(400);
                });
                if (Styx.CommonBot.Frames.MailFrame.Instance.IsVisible)
                {
                    bool hasMail = Lua.GetReturnVal<bool>("return MILA_INBOX:hasMail()", 0);
                    while (hasMail)
                    {
                        await Task.Run(async () =>
                        {
                            Lua.DoString("MILA_INBOX:getAll()");
                            await Task.Delay(500);
                            hasMail = Lua.GetReturnVal<bool>("return MILA_INBOX:hasMail()", 0);
                        });
                    }
                    Styx.CommonBot.Frames.MailFrame.Instance.Close();
                    post();
                }
                else
                {
                    Log("nicht offen");
                }
            }
        }
        private async Task ah()
        {
            await Task.Run(async () =>
            {
                WoWUnit auc = findAuctioner();
                auc.Face();
                auc.Target();
                while (!auc.WithinInteractRange)
                {
                    await Task.Run(async () =>
                    {
                        //Navigator.MoveTo(auc.Location);
                        WoWMovement.ClickToMove(auc.Location);
                        await Task.Delay(100);
                    });
                }
                WoWMovement.MoveStop();
                auc.Interact();
            });
        }
        private async Task startsnipe()
        {
            if (!Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible)
                snipe = false;
            if (!snipe)
            {
                if (Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible)
                {
                    snipe = true;
                    await Task.Run(async () =>
                    {
                        await Task.Delay(200);
                        Lua.DoString("MILA_FRAME:changeTab()");
                        await Task.Delay(1000);
                        Lua.DoString("MILA_OTHER:startSnipe()");
                    });
                }
            }
        }
        private async Task checkauction()
        {
            if (snipe && Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible)
            {
                bool hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
                Log(String.Format("{0} {1}", hasAuction.ToString(), Me.Name.ToString()));
                while (hasAuction)
                {
                    if (Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible)
                    {
                        await Task.Run(async () =>
                        {
                            KeyboardManager.PressKey('4');
                            KeyboardManager.ReleaseKey('4');
                            await Task.Delay(100);
                            hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
                        });
                    }
                }
            }
        }
        private WoWUnit findAuctioner()
        {
            return ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.IsAuctioneer == true).OrderBy(u => u.Distance).FirstOrDefault();
        }
        public WoWGameObject findMailbox()
        {
            return ObjectManager.GetObjectsOfType<WoWGameObject>().Where(u => u.SubType == WoWGameObjectType.Mailbox).OrderBy(u => u.Distance).FirstOrDefault();
        }
        private void doReload()
        {
            Lua.DoString("ConsoleExec(\"reloadui\")");
        }
        #region Log
        private void Log(string v)
        {
            Logging.Write(LogLevel.Normal, Colors.Aquamarine, string.Format("[{0}]: {1}", Name, v));
        }
        #endregion
    }
}
