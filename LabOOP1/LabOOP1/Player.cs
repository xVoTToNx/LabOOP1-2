using Newtonsoft.Json;
using System;
using System.IO;

namespace GameSpace
{
    public struct Player
    {
        const int deltaGhost = 7500;

        public string Name;

        public string Map;

        public string Skin;

        public int DeltaGhost;

        public bool IsNew;

        public void Reset()
        {
            Map = "map\\Alpha.txt";
            Skin = "skins\\B.txt";
            IsNew = true;
            DeltaGhost = deltaGhost;
        }

        public void Save()
        {
            string buffer = JsonConvert.SerializeObject(this);
            string safeName = makeNameSafe(Name);
            File.WriteAllText("players\\" + safeName + ".txt", buffer);
        }

        string makeNameSafe(string name)
        {

            name = name.Replace("<", "{lt}");

            name = name.Replace(">", "{gt}");

            name = name.Replace(":", "{cl}");

            name = name.Replace("\"", "{dq}");

            name = name.Replace("/", "{fs}");

            name = name.Replace("\\", "{bs}");

            name = name.Replace("|", "{vb}");

            name = name.Replace("?", "{qm}");

            name = name.Replace("*", "{ar}");

            return name;
        }
    }
}
