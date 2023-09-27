using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IoT_Server.Modules
{
    class IO
    {
        public string MainPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/";

        public void SetPath(string path)
        {
            this.MainPath += path + "/";
        }

        public bool Exists(string file)
        {
            return System.IO.File.Exists(MainPath + file);
        }

        public string Read(string file)
        {
            if (System.IO.File.Exists(MainPath + file))
                return System.IO.File.ReadAllText(MainPath + file);
            return null;
        }

        public void Write(string file, string text)
        {
            System.IO.File.WriteAllText(MainPath + file, text);
        }

        public List<string> GetFiles(string path)
        {
            List<string> files = new List<string>();

            if (Directory.Exists(MainPath + path))
            {
                foreach (string file in Directory.GetFiles(MainPath + path))
                {
                    files.Add(Path.GetFileName(file));
                }
            }
            return files;
        }

        public List<string> GetDirs(string path)
        {
            List<string> dirs = new List<string>();

            if (Directory.Exists(MainPath + path))
            {
                foreach (string dir in Directory.GetDirectories(MainPath + path))
                {
                    dirs.Add(Path.GetFileName(dir));
                }
            }
            return dirs;
        }
    }
}
