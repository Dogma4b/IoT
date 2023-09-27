using System;
using System.Text;
using System.Text.RegularExpressions;

namespace IoT_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Lua EnvMain = new Lua();

            while (true)
            {
                Console.Write(">");

                string command = Console.ReadLine();
                if (command == "multiline")
                {
                    string multiline = "";
                    input:
                    Console.Write(">");
                    string curline = Console.ReadLine();

                    if (curline != String.Empty)
                    {
                        multiline += curline + "\n";
                        goto input;
                    }

                    EnvMain.LuaRun(multiline, "Console");
                }
                else
                {
                    EnvMain.LuaRun(command, "Console");
                }
            }
        }
    }
}