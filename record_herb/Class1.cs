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
using System.Threading;
using System.IO;

namespace farm_stein
{
    public class Class1 : HBPlugin
    {
        public override string Name { get { return "Record Resources"; } }
        public override string Author { get { return "Mila432"; } }
        public override Version Version { get { return _version; } }
        private readonly Version _version = new Version(1, 0, 0, 0);
        private LocalPlayer Me { get { return StyxWoW.Me; } }
        HashSet<WoWGuid> alreadyseen = new HashSet<WoWGuid>();
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public static void WriteToFile(string text, string path)
        {
            _readWriteLock.EnterWriteLock();
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(text);
                sw.Close();
            }
            _readWriteLock.ExitWriteLock();
        }

        public override void Pulse()
        {
            var targets = ObjectManager.GetObjectsOfType<WoWGameObject>().Where(u => u.IsHerb ||u.IsMineral).OrderBy(u => u.Distance);
            foreach (var t in targets)
            {
                if (t != null)
                {
                    if (!alreadyseen.Contains(t.Guid))
                    {
                        alreadyseen.Add(t.Guid);
                        string tmp = string.Format("<Hotspot Name=\"{0}\" X=\"{1}\" Y=\"{2}\" Z=\"{3}\" />", t.Name.ToString(), t.Location.X.ToString(), t.Location.Y.ToString(), t.Location.Z.ToString());
                        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        //string file = string.Format("C:/Users/Admin/Desktop/wow_profiles/{0}.xml", Me.ZoneText);
                        string fin = string.Format("{0}/{1}", desktop,"wow_profiles");
                        Directory.CreateDirectory(fin);
                        string file = string.Format("{0}/{1}.xml", fin, Me.ZoneText);
                        WriteToFile(tmp, file);
                        //Logging.Write(Styx.Common.LogLevel.Quiet, Colors.Aquamarine, string.Format("<Hotspot Name=\"{0}\" X=\"{1}\" Y=\"{2}\" Z=\"{3}\" />",t.Name.ToString(), t.Location.X.ToString(), t.Location.Y.ToString(), t.Location.Z.ToString()));
                    }
                }
            }
        }
    }
}
