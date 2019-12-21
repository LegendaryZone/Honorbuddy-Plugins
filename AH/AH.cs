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

namespace ah
{
    public class AH : HBPlugin
    {
        public override string Name { get { return "AH"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        private LocalPlayer Me { get { return StyxWoW.Me; } }

        bool sleepThread = false;
        bool willReload = false;
        bool hasWhisper = false;
        bool attachedToChat = false;
        string token = "";
        static Dictionary<int, string> whispers = new Dictionary<int, string>();

        async Task delay_reload()
        {
            await Task.Run(async () =>
            {
                //await Task.Delay(1800000);
                await Task.Delay(600000);
                willReload = true;
                await Task.Delay(2000);
                reload();
            });
            sleepThread = false;
            willReload = false;
        }
        private void doWork()
        {
            if (sleepThread == false)
            {
                delay_reload();
                sleepThread = true;
            }
            if (Me.GotTarget)
            {
                //Log("Me.GotTarget", LogLevel.Normal);
                if (checkIfIsAuctioneer())
                {
                    //Log("checkIfIsAuctioneer()", LogLevel.Normal);
                    if (Me.CurrentTarget.CanInteractNow)
                    {
                        //Log("Me.CurrentTarget.CanInteractNow", LogLevel.Normal);
                        Me.CurrentTarget.Face();
                        Me.CurrentTarget.Interact();
                        startSnipe();
                    }
                    else
                    {
                        Me.CurrentTarget.Face();
                        //Log("Navigator.MoveTo", LogLevel.Normal);
                        if (!Me.CurrentTarget.CanInteractNow)
                        {
                            Navigator.MoveTo(Me.CurrentTarget.Location);
                            Thread.Sleep(200);
                        }
                    }
                }
                else
                {
                    //Log("!checkIfIsAuctioneer()", LogLevel.Normal);
                    Me.ClearTarget();
                    WoWUnit auctioneer = ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.IsAuctioneer == true).OrderBy(u => u.Distance).FirstOrDefault();
                    auctioneer.Face();
                    auctioneer.Target();
                    if (Me.CurrentTarget.CanInteractNow)
                    {
                        //Log("CanInteractNow", LogLevel.Normal);
                        Me.CurrentTarget.Interact();
                        startSnipe();
                    }
                    else
                    {
                        Me.CurrentTarget.Face();
                        //Log("!Me.CurrentTarget.CanInteractNow", LogLevel.Normal);
                        if (!Me.CurrentTarget.CanInteractNow)
                        {
                            Navigator.MoveTo(Me.CurrentTarget.Location);
                            Thread.Sleep(200);
                        }
                    }
                }
            }
            else
            {
                //Log("!Me.GotTarget", LogLevel.Normal);
                WoWUnit auctioneer = ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.IsAuctioneer == true).OrderBy(u => u.Distance).FirstOrDefault();
                auctioneer.Face();
                auctioneer.Target();
                if (Me.CurrentTarget.CanInteractNow)
                {
                    //Log("CanInteractNow", LogLevel.Normal);
                    Me.CurrentTarget.Interact();
                    startSnipe();
                }
                else
                {
                    //Log("!Me.CurrentTarget.CanInteractNow", LogLevel.Normal);
                    if (!Me.CurrentTarget.CanInteractNow)
                    {
                        Navigator.MoveTo(Me.CurrentTarget.Location);
                        Thread.Sleep(200);
                    }
                }
            }
        }

        public override void Pulse()
        {
            if (hasWhisper)
            {
                checkForReplys();
            }

            doWork();
            bool status = Lua.GetReturnVal<bool>("return MILA.hasAuction()", 0);
            Log(status.ToString(), LogLevel.Normal);
            if (status && !willReload)
            {
                KeyboardManager.KeyUpDown('4');
            }
        }

        private void startSnipe()
        {
            Lua.DoString("RunMacroText(\"/click StartSnipe\")");
        }

        private void reload()
        {
            Lua.DoString("RunMacroText(\"/reload\")");
        }

        public override void OnEnable()
        {
            if (attachedToChat == false)
            {
                attachedToChat = true;
                Lua.Events.AttachEvent("CHAT_MSG_SYSTEM", handleAuction);
                Lua.Events.AttachEvent("CHAT_MSG_WHISPER", handleWhisper);

                string data = "{\"email\":\"mail@gmail.com\",\"password\": \"pass\"}";
                token = GetLoginToken("https://discordapp.com/api/v6/auth/login", data, "application/json");
            }
            Log("Plugin enabled", LogLevel.Normal);
        }

        public override void OnDisable()
        {
            Lua.Events.DetachEvent("CHAT_MSG_SYSTEM", handleAuction);
            Lua.Events.DetachEvent("CHAT_MSG_WHISPER", handleWhisper);
            attachedToChat = false;
            Log("Plugin disabled", LogLevel.Normal);
        }

        private void handleAuction(object sender, LuaEventArgs args)
        {
            if (args.Args[0].ToString().StartsWith("Ihr habt") || args.Args[0].ToString().StartsWith("You won an auction"))
            {
                prepareSendMessage(args.Args[0].ToString(), "240534944558874625");
            }
        }

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

        private void prepareSendMessage(string msg, string channel_id)
        {
            //Log(string.Format("Message: {0}", msg), LogLevel.Normal);
            string message = string.Format("{{\"content\":\"{0}:{1}\",\"nonce\":\"240535141317869568\",\"tts\":\"false\"}}", Me.Name, msg);
            string head = string.Format("authorization: {0}", token);
            string url = string.Format("https://discordapp.com/api/v6/channels/{0}/messages", channel_id);
            sendMessage(url, message, "application/json", head);
        }

        private void Log(string v, LogLevel l)
        {
            Logging.Write(l, Colors.Aquamarine, string.Format("[AH]: {0}", v));
        }
        private bool checkIfIsAuctioneer()
        {
            return Me.CurrentTarget.IsAuctioneer;
        }

        async Task checkForReplys()
        {
            await Task.Run(async () =>
            {
                WebRequest request = WebRequest.Create("https://discordapp.com/api/v6/channels/245913020310487041/messages?limit=50");
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
                    string tmp = msg["content"].ToString();
                    string[] words = tmp.Split(':');
                    int tmp_int = Int32.Parse(words[0]);
                    if (whispers.ContainsKey(tmp_int))//check if int is in dict
                    {
                        hasWhisper = false;
                        //Log("found matching number", LogLevel.Normal);
                        answerMessage(words[1], whispers[tmp_int]);
                        whispers.Remove(tmp_int);
                    }
                }
                await Task.Delay(2000);
            });
        }

        private void answerMessage(string msg, string user)
        {
            Lua.DoString(string.Format("SendChatMessage(\"{0}\", 'WHISPER', nil, \"{1}\");", msg, user));
        }

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
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            char[] separatingChars = { '"' };
            string[] words = responseFromServer.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
            //Console.WriteLine(words[3]);
            reader.Close();
            dataStream.Close();
            response.Close();
            return words[3];
        }
        public static void sendMessage(string url, string data, string ContentType, string token)
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
    }
}