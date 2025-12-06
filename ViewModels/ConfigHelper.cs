using System.IO;
using System.Linq;

namespace ATC4_HQ.ViewModels
{
    public static class ConfigHelper
    {
        public static bool LoadBtConsent(string iniPath)
        {
            if (!File.Exists(iniPath)) return false;
            var lines = File.ReadAllLines(iniPath);
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("BTEnabled="))
                {
                    var val = line.Split('=')[1].Trim();
                    return val == "1";
                }
            }
            return false;
        }

        public static void SaveBtConsent(string iniPath, bool agreed)
        {
            string[] lines = File.Exists(iniPath) ? File.ReadAllLines(iniPath) : new string[] {"[main]"};
            bool found = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("BTEnabled="))
                {
                    lines[i] = $"BTEnabled={(agreed ? "1" : "0")}";
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var list = lines.ToList();
                list.Add($"BTEnabled={(agreed ? "1" : "0")}");
                lines = list.ToArray();
            }
            File.WriteAllLines(iniPath, lines);
        }
    }
}
