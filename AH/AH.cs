﻿using Styx.Common;
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
        public override string Name { get { return "Auction House Bot"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        private LocalPlayer Me { get { return StyxWoW.Me; } }

        //urls
        string url_getreply = "https://discordapp.com/api/v6/channels/245913020310487041/messages?limit=50";

        bool sleepThread = false;
        bool hasWhisper = false;
        bool isShopping = false;
        bool alreadySniping = false;
        bool alreadySending = false;
        bool attachedToChat = false;
        bool startedSniper = false;
        bool loveHorde = false;
        bool doingPost = false;
        bool doingMail = false;
        bool doingMailv2 = false;
        bool doingPostv2 = false;
        bool debug_m = false;
        bool startedAuction = false;
        bool hasAuction = false;
        bool hasNoMoreMail = false;
        string token = "";
        System.Timers.Timer aTimer;
        System.Timers.Timer loginTimer;
        System.Timers.Timer relogTimer;
        static Dictionary<int, string> whispers = new Dictionary<int, string>();

        private WoWUnit findAuctioner()
        {
            Me.ClearTarget();
            return ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.IsAuctioneer == true).OrderBy(u => u.Distance).FirstOrDefault();
        }
        public WoWGameObject GetMailBox()
        {
            return ObjectManager.GetObjectsOfType<WoWGameObject>().Where(u => u.SubType == WoWGameObjectType.Mailbox).OrderBy(u => u.Distance).FirstOrDefault();
        }
        #region doStuffWithNPC
        private void doStuffWithNPC()
        {
            Me.CurrentTarget.Face();
            if (!isShopping)
                Me.CurrentTarget.Interact();
            if (Me.CurrentTarget.WithinInteractRange)
            {
                startSnipe();
            }
        }
        #endregion
        #region moveToNPC
        private void moveToNPC()
        {
            Navigator.MoveTo(Me.CurrentTarget.Location);
            Thread.Sleep(500);
        }
        #endregion
        #region delay_reload
        async Task delay_reload()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(600000);
                //if (!doingMailv2 && !doingPostv2 && !TreeRoot.IsRunning)
                    doReload();
            });
            sleepThread = false;
        }
        #endregion
        #region doWork
        private void doWork()
        {
            if (Me.GotTarget && Me.CurrentTarget.IsAuctioneer && Me.CurrentTarget.CanInteract)
            {
                if (Me.IsMoving && Me.CurrentTarget.WithinInteractRange)
                    WoWMovement.MoveStop();
                doStuffWithNPC();
            }
            else if (Me.GotTarget && Me.CurrentTarget.IsAuctioneer && !Me.CurrentTarget.CanInteract)
            {
                Me.CurrentTarget.Face();
                moveToNPC();
            }
            else if (!Me.GotTarget || !Me.CurrentTarget.IsAuctioneer)
            {
                WoWUnit foundAuctioner = findAuctioner();
                foundAuctioner.Target();
                foundAuctioner.Face();
            }
        }
        #endregion
        public async Task GetMailv2()
        {
            
            //debug_m = true;
            doingMailv2 = true;
            Log("GetMailv2");
            WoWGameObject mail = GetMailBox();
            Log("moving to Post");
            bool canUseMail = mail.CanUseNow();
            await Task.Run(async () =>
            {
                WoWMovement.ClickToMove(mail.Location);
                await Task.Delay(3000);
            });

            Log("nach post usen schleife");
            Log("können post usen");
            //bool isopen = Styx.CommonBot.Frames.MailFrame.Instance.IsVisible;
            mail.Interact();
            bool hasMail = true;
            while (hasMail)
            {
                Log("hasMail");
                Lua.DoString("CloseAllBags()");
                if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld)
                {
                    Log("nicht im spiel");
                    await Task.Run(async () =>
                    {
                        await Task.Delay(7000);
                    });
                    mail.Interact();
                }
                //Log("wir haben post");
                bool isMailOpen = Styx.CommonBot.Frames.MailFrame.Instance.IsVisible;
                while (!isMailOpen)
                {
                    Log("mail nicht offen");
                    mail.Interact();
                    await Task.Run(async () =>
                    {
                        await Task.Delay(500);
                    });
                    isMailOpen = Styx.CommonBot.Frames.MailFrame.Instance.IsVisible;
                }
                Log("mail offen");
                if (isMailOpen)
                    Lua.DoString("MILA_INBOX:getAll()");
                await Task.Run(async () =>
                {
                    await Task.Delay(500);
                });
                if (Lua.GetReturnVal<bool>("return MILA_INBOX:shouldReload()", 0))
                {
                    Log("wir haben mehr post aber seite ist leer");
                    doReload();
                    await Task.Run(async () =>
                    {
                        await Task.Delay(9000);
                    });
                }
                while (!isMailOpen)
                {
                    Log("mail nicht offen");
                    mail.Interact();
                    await Task.Run(async () =>
                    {
                        await Task.Delay(500);
                    });
                    isMailOpen = Styx.CommonBot.Frames.MailFrame.Instance.IsVisible;
                }
                await Task.Run(async () =>
                {
                    await Task.Delay(300);
                });
                hasMail = Lua.GetReturnVal<bool>("return MILA_INBOX:hasMail()", 0);
            }
            doingPostv2 = true;
            doingMailv2 = false;
            Log("fertig mit post abholen");
            Styx.CommonBot.Frames.MailFrame.Instance.Close();
            PostItemsv2();
            /*
            doingMailv2 = true;
            Log("GetMailv2");
            WoWGameObject mail = GetMailBox();
            Log("moving to Post");
            bool canUseMail = false;// mail.WithinInteractRange;
            while (!canUseMail)
            {
                Log("können post nicht usen");
                doingMailv2 = false;
                return;
                await Task.Run(async () =>
                {
                    WoWMovement.ClickToMove(mail.Location);
                    await Task.Delay(1000);
                });
                canUseMail = mail.WithinInteractRange;
            }
            if (Me.IsMoving)
                WoWMovement.MoveStop();
            Log("können post usen");
            mail.Interact();
            bool hasMail = true;
            while (hasMail)
            {
                Lua.DoString("CloseAllBags()");
                if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld)
                {
                    await Task.Run(async () =>
                    {
                        await Task.Delay(4000);
                    });
                }
                //Log("wir haben post");
                bool isMailOpen = Styx.CommonBot.Frames.MailFrame.Instance.IsVisible;
                while (!isMailOpen)
                {
                    mail.Interact();
                    await Task.Run(async () =>
                    {
                        await Task.Delay(500);
                    });
                    isMailOpen = Styx.CommonBot.Frames.MailFrame.Instance.IsVisible;
                }
                if (isMailOpen)
                    Lua.DoString("MILA_INBOX:getAll()");
                if (Lua.GetReturnVal<bool>("return MILA_INBOX:shouldReload()", 0))
                {
                    Log("wir haben mehr post aber seite ist leer");
                    doReload();
                    await Task.Run(async () =>
                    {
                        await Task.Delay(9000);
                    });
                }
                await Task.Run(async () =>
                {
                    await Task.Delay(300);
                });
                hasMail = Lua.GetReturnVal<bool>("return MILA_INBOX:hasMail()", 0);
            }
            doingMailv2 = false;
            Log("fertig mit post abholen");
            Styx.CommonBot.Frames.MailFrame.Instance.Close();
            PostItemsv2();
            */
        }
        public async Task PostItemsv2()
        {
            doingPostv2 = true;
            Log("PostItemsv2");
            WoWUnit auctioneer = findAuctioner();
            bool canTalkToAh;
            while (true)
            {
                canTalkToAh = auctioneer.WithinInteractRange;
                await Task.Run(async () =>
                {
                    Log("sleep 400");
                    await Task.Delay(400);
                });
                if (!canTalkToAh || canTalkToAh)
                    break;
            }
            Log(string.Format("canTalkToAh {0}", canTalkToAh.ToString()));
            while (!canTalkToAh)
            {
                Log("koennen mit ah nicht reden");
                doingPostv2 = false;
                return;
                WoWMovement.ClickToMove(auctioneer.Location);
                await Task.Run(async () =>
                {
                    Log("sleep 400");
                    await Task.Delay(400);
                });
                Log("canTalkToAh");
                canTalkToAh = auctioneer.WithinInteractRange;
                if (canTalkToAh && Me.IsMoving)
                    WoWMovement.MoveStop();
            }
            Log("können mit ah reden");
            bool isAhOpen = Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible;
            while (!isAhOpen)
            {
                auctioneer.Interact();
                await Task.Run(async () =>
                {
                    await Task.Delay(700);
                });
                isAhOpen = Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible;
            }
            Log("ah ist offen");
            Lua.DoString("AuctionFrameTab5:Click()");
            await Task.Run(async () =>
            {
                await Task.Delay(500);
            });
            Lua.DoString("RunMacroText(\"/click postBtn\")");
            bool dScanning = Lua.GetReturnVal<bool>("return MILA_POSTSCAN:doneScanning()", 0);
            //Log(string.Format("dScanning {0}", dScanning));
            while (!dScanning)
            {
                Log("while dScanning");
                await Task.Run(async () =>
                {
                    await Task.Delay(1500);
                });
                if (Lua.GetReturnVal<bool>("return MILA_MANAGE:hasItemsToSell()", 0))
                {
                    Log("hasItemsToSell");
                    doingPostv2 = false;
                    return;
                }
                dScanning = Lua.GetReturnVal<bool>("return MILA_POSTSCAN:doneScanning()", 0);
            }
            bool donePosting = Lua.GetReturnVal<bool>("return MILA_MANAGE:donePosting()", 0);
            Log(string.Format("donePosting {0}", donePosting));
            while (!donePosting)
            {
                Log("while donePosting");
                KeyboardManager.PressKey('4');
                KeyboardManager.ReleaseKey('4');
                await Task.Run(async () =>
                {
                    await Task.Delay(100);
                });
                donePosting = Lua.GetReturnVal<bool>("return MILA_MANAGE:donePosting()", 0);
            }
            Lua.DoString("MILA_MANAGE:resetDonePosting()");
            Lua.DoString("MILA_POSTSCAN:resetScanning()");
            Styx.CommonBot.Frames.AuctionFrame.Instance.Close();
            Log("fertig mit posten");
            doingPostv2 = false;
        }

        public async Task checkAuction()
        {
            startedAuction = true;
            Log("checkAuction");
            bool hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
            while (true)
            {
                Log("sind in while(true)");
                if (doingMailv2 || doingPostv2)
                {
                    Log("fick dich hb");
                    startedAuction = false;
                    break;
                }
                    hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
                //Log(String.Format("{0} {1}", hasAuction.ToString(), Me.Name.ToString()));
                Log(hasAuction.ToString());
                if (hasAuction)
                {
                    while (hasAuction)
                    {
                        Log("wir haben auktion");
                        KeyboardManager.PressKey('4');
                        KeyboardManager.ReleaseKey('4');
                        await Task.Run(async () =>
                        {
                            await Task.Delay(100);
                        });
                        hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
                    }
                }
                /*
                if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld || doingMailv2 || doingPostv2|| !TreeRoot.IsRunning)
                {
                    Log("brechen abcheckAuction");
                    startedAuction = false;
                    break;
                }*/
               await Task.Run(async () =>
                {
                    await Task.Delay(100);
                }); 
            }
            Log("wtf machen wir hier");
            startedAuction = false;
        }

        public async Task buyAuction()
        {
            bool hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
            Log(String.Format("{0} {1}", hasAuction.ToString(), Me.Name.ToString()));
            if (hasAuction)
            {
                while (hasAuction)
                {
                    KeyboardManager.PressKey('4');
                    KeyboardManager.ReleaseKey('4');
                    await Task.Run(async () =>
                    {
                        await Task.Delay(100);
                    });
                    hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
                }
            }
        }
        #region Pulse
        public override void Pulse()
        {
            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld)
                return;
            if (!Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible)
            {
                alreadySniping = false;
                isShopping = false;
            }
            /*if (sleepThread == false)
            {
                delay_reload();
                sleepThread = true;
            }*/
            Lua.DoString("CloseAllBags()");
            if (hasWhisper)
            {
                checkForReplys();
            }
            if (!doingMailv2 && !doingPostv2)
            {
                doWork();
                hasAuction = Lua.GetReturnVal<bool>("return MILA_TAB:hasAuction()", 0);
                Log(String.Format("{0} {1}", hasAuction.ToString(), Me.Name.ToString()));
                if (hasAuction)
                {
                    KeyboardManager.PressKey('4');
                    KeyboardManager.ReleaseKey('4');
                }
            }
        }
        #endregion
        #region startSnipe
        async Task startSnipe()
        {
            if (Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible && !alreadySniping && !doingMail)
            {
                alreadySniping = true;
                isShopping = true;
                Log("startSnipe");
                //Lua.DoString("RunMacroText(\"/click StartSnipe\")");
                await Task.Run(async () =>
                {
                    await Task.Delay(700);
                    Lua.DoString("MILA_FRAME:changeTab()");
                    await Task.Delay(1000);
                    Lua.DoString("MILA_OTHER:startSnipe()");
                });
            }
        }
        #endregion
        #region doReload
        private void doReload()
        {
            Lua.DoString("ConsoleExec(\"reloadui\")");
        }
        #endregion
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            Log("ontimedevent");
            GetMailv2();
        }
        private void checkIfLoggedIn(object sender, ElapsedEventArgs e)
        {
            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld)
            {
                string s = Lua.GetReturnValues("return AccountLogin.UI.AccountEditBox:GetText()")[0].ToString();
            /*if (s.Contains("mail"))// == "mail@gmail.com")
            {
                Lua.DoString("AccountLogin.UI.PasswordEditBox:SetText(\"password\")"); // set password 
            }
            else
            {
                Lua.DoString("AccountLogin.UI.PasswordEditBox:SetText(\"password\")"); // set password 
            }*/
            Lua.DoString("AccountLogin.UI.PasswordEditBox:SetText(\"password\")"); // set password 
            //password
            Thread.Sleep(500);
                Lua.DoString("AccountLogin_Login()"); //do login 
                Thread.Sleep(6000);
                Lua.DoString("CharSelectEnterWorldButton:Click()"); //login to realm
            }
        }

        #region OnEnable
        public override void OnEnable()
        {
            DateTime time = DateTime.Now;
            Log(time.ToString("h:mm:ss tt"));
            //aTimer = new System.Timers.Timer(60 * 60 * 3 * 1000); //one hour in milliseconds
            //aTimer = new System.Timers.Timer(60 * 60 * 1000); //one hour in milliseconds
            aTimer = new System.Timers.Timer(30 * 1000); //one hour in milliseconds
            //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //aTimer.Start();

            loginTimer = new System.Timers.Timer(60 * 1000);
            loginTimer.Elapsed += new ElapsedEventHandler(checkIfLoggedIn);
            loginTimer.Start();

            relogTimer = new System.Timers.Timer(60*10 * 1000);
            relogTimer.Elapsed += new ElapsedEventHandler(checkIfReload);
            relogTimer.Start();

            if (attachedToChat == false)
            {
                attachedToChat = true;
                Lua.Events.AttachEvent("CHAT_MSG_SYSTEM", handleAuction);
                Lua.Events.AttachEvent("CHAT_MSG_WHISPER", handleWhisper);
                try
                {
                    string data = "{\"email\":\"mail@gmail.com\",\"password\": \"password\"}";
                    token = GetLoginToken("https://discordapp.com/api/v6/auth/login", data, "application/json");
                }
                catch
                {
                    token = "";
                }
            }
            Log(string.Format("Plugin enabled {0}", Me.Name.ToString()));
        }

        private void checkIfReload(object sender, ElapsedEventArgs e)
        {
            if(TreeRoot.IsRunning)
            doReload();
        }
        #endregion
        #region OnDisable
        public override void OnDisable()
        {
            Lua.Events.DetachEvent("CHAT_MSG_SYSTEM", handleAuction);
            Lua.Events.DetachEvent("CHAT_MSG_WHISPER", handleWhisper);
            startedSniper = false;
            aTimer.Dispose();
            loginTimer.Stop();
            relogTimer.Stop();
            attachedToChat = false;
            Log("Plugin disabled");
        }
        #endregion
        #region handleAuction
        private void handleAuction(object sender, LuaEventArgs args)
        {
            if (args.Args[0].ToString().StartsWith("Ihr habt") || args.Args[0].ToString().StartsWith("You won an auction"))
            {
                prepareSendMessage(args.Args[0].ToString(), "240534944558874625");
            }
        }
        #endregion
        #region handleWhisper
        private void handleWhisper(object sender, LuaEventArgs args)
        {
            //Log("have whisper", LogLevel.Normal);
            hasWhisper = true;
            Random r = new Random();
            int rInt = r.Next(0, 10000);
            //Log(rInt.ToString(), LogLevel.Normal);
            whispers.Add(rInt, args.Args[1].ToString());
            string tmp = string.Format("'{0}' von {1}:{2}", args.Args[0].ToString(), args.Args[1].ToString(), rInt.ToString());
            prepareSendMessage(tmp, "241166290855788547");
        }
        #endregion
        #region prepareSendMessage
        private void prepareSendMessage(string msg, string channel_id)
        {
            if (token == "")
                return;
            //Log(string.Format("Message: {0}", msg), LogLevel.Normal);
            string message = string.Format("{{\"content\":\"{0}:{1}\",\"nonce\":\"240535141317869568\",\"tts\":\"false\"}}", Me.Name, msg);
            string head = string.Format("authorization: {0}", token);
            string url = string.Format("https://discordapp.com/api/v6/channels/{0}/messages", channel_id);
            sendMessageToDiscord(url, message, "application/json", head);
        }
        #endregion
        #region Log
        private void Log(string v)
        {
            Logging.Write(LogLevel.Normal, Colors.Aquamarine, string.Format("[Auction House Bot]: {0}", v));
        }
        #endregion
        #region checkForReplys
        async Task checkForReplys()
        {
            await Task.Run(async () =>
            {
                WebRequest request = WebRequest.Create(url_getreply);
                string head = string.Format("authorization: {0}", token);
                request.Headers.Add(head);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                JArray msgs = JArray.Parse(responseFromServer);
                reader.Close();
                response.Close();
                foreach (var msg in msgs)
                {
                    try
                    {
                        string tmp = msg["content"].ToString();
                        string[] words = tmp.Split(':');
                        int tmp_int = Int32.Parse(words[0]);
                        if (whispers.ContainsKey(tmp_int))//check if int is in dict
                        {
                            if (hasWhisper)
                            {
                                hasWhisper = false;
                                alreadySending = true;
                                //Log("found matching number", LogLevel.Normal);
                                sendWhisper(words[1], whispers[tmp_int]);
                                whispers.Remove(tmp_int);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                await Task.Delay(2000);
            });
        }
        #endregion
        #region sendWhisper
        private void sendWhisper(string msg, string user)
        {
            if (alreadySending)
            {
                Lua.DoString(string.Format("SendChatMessage(\"{0}\", 'WHISPER', nil, \"{1}\");", msg, user));
                alreadySending = false;
            }
        }
        #endregion
        #region GetLoginToken
        public static string GetLoginToken(string url, string data, string ContentType)
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            string postData = data;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = ContentType;
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            char[] separatingChars = { '"' };
            string[] words = responseFromServer.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
            reader.Close();
            dataStream.Close();
            response.Close();
            return words[3];
        }
        #endregion
        #region sendMessageToDiscord
        public static void sendMessageToDiscord(string url, string data, string ContentType, string token)
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            string postData = data;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = ContentType;
            request.Headers.Add(token);
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            //Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();
            response.Close();
        }
        #endregion
    }
}