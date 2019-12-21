using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx;
using Styx.Plugins;
using Styx.Common;
using System.Windows.Media;
using Styx.WoWInternals;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Styx.WoWInternals.WoWObjects;

namespace whisper_to_discord
{
    public class whisper_to_discord : HBPlugin
    {
        public override string Name { get { return "Whisper To Discord"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }

        string token;
        bool hasWhisper;
        bool alreadySending;
        string url_getreply = "https://discordapp.com/api/v6/channels/245913020310487041/messages?limit=50";
        static Dictionary<int, string> whispers = new Dictionary<int, string>();
        private LocalPlayer Me { get { return StyxWoW.Me; } }

        public override void Pulse()
        {
            if (hasWhisper)
            {
                checkForReplys();
            }
        }
        private void prepareSendMessage(string msg, string channel_id)
        {
            if (token == "")
                return;
            string message = string.Format("{{\"content\":\"{0}:{1}\",\"nonce\":\"240535141317869568\",\"tts\":\"false\"}}", Me.Name, msg);
            string head = string.Format("authorization: {0}", token);
            string url = string.Format("https://discordapp.com/api/v6/channels/{0}/messages", channel_id);
            sendMessageToDiscord(url, message, "application/json", head);
        }
        private void sendWhisper(string msg, string user)
        {
            if (alreadySending)
            {
                Lua.DoString(string.Format("SendChatMessage(\"{0}\", 'WHISPER', nil, \"{1}\");", msg, user));
                alreadySending = false;
            }
        }
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
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
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
                        if (whispers.ContainsKey(tmp_int))
                        {
                            if (hasWhisper)
                            {
                                hasWhisper = false;
                                alreadySending = true;
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
        private void handleWhisper(object sender, LuaEventArgs args)
        {
            hasWhisper = true;
            Random r = new Random();
            int rInt = r.Next(0, 10000);
            whispers.Add(rInt, args.Args[1].ToString());
            string tmp = string.Format("'{0}' von {1}:{2}", args.Args[0].ToString(), args.Args[1].ToString(), rInt.ToString());
            prepareSendMessage(tmp, "241166290855788547");
        }
        private void handleAuction(object sender, LuaEventArgs args)
        {
            if (args.Args[0].ToString().StartsWith("Ihr habt") || args.Args[0].ToString().StartsWith("You won an auction"))
            {
                prepareSendMessage(args.Args[0].ToString(), "240534944558874625");
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Lua.Events.AttachEvent("CHAT_MSG_SYSTEM", handleAuction);
            Lua.Events.AttachEvent("CHAT_MSG_WHISPER", handleWhisper);
            try
            {
                string data = "{\"email\":\"mail@gmail.com\",\"password\": \"password\"}";
                token = GetLoginToken("https://discordapp.com/api/v6/auth/login", data, "application/json");
            }
            catch
            {
                Log("fehler beim token");
                token = "";
            }
            Log("Plugin enabled");
        }
        public override void OnDisable()
        {
            Lua.Events.DetachEvent("CHAT_MSG_SYSTEM", handleAuction);
            Lua.Events.DetachEvent("CHAT_MSG_WHISPER", handleWhisper);
            Log("Plugin disabled");
            base.OnDisable();
        }
        private void Log(string v)
        {
            Logging.Write(LogLevel.Normal, Colors.Aquamarine, string.Format("[{0}]: {1}", Name, v));
        }
    }
}
