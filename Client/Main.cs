using Client.Configuration;
using Common;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {


        static bool inputTrue = true;
        /*
          "-sig",
            "~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.sig",
            "- formula",
            "~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.mfotl",
            "- log",
            "~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.log",
            "- negate"
         */

        public static int Main(string[] args)
        {

            // https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory
            // https://bitbucket.org/monpoly/monpoly/src/master/
            string command = "/Users/ztg100/.opam/4.14.0/bin/monpoly -sig ~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.sig -formula ~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.mfotl -log ~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.log -negate";

            if (inputTrue) { 
                command = "/Users/ztg100/.opam/4.14.0/bin/monpoly -sig ~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.sig -formula ~/workspace/monpoly-monpoly-adf23484edfe/examples/rv11.mfotl -negate";
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {

                // FileName = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal",
                FileName = "/bin/bash",
                Arguments = "-c \" " + command + " \"",

                // FileName = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal",
                // FileName = "~/workspace/EventBenchmark/Client/script.bash",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };


            Process process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            if (inputTrue)
            {

                process.StandardInput.WriteLine("   @1307532861 approve (152) ");
                process.StandardInput.WriteLine("   @1307955600 approve (163) ");
                process.StandardInput.WriteLine("               publish (160) ");
                process.StandardInput.WriteLine("   @1308477599 approve (187) ");
                process.StandardInput.WriteLine("               publish (163) ");
                process.StandardInput.WriteLine("               publish (152) ");
                process.StandardInput.WriteLine("   @2308477599 publish (210) ");

                // FIXME maybe I need to send a signal of end of stream to monpoly?
                // it looks like the process is waiting for more input to spit output.... CONFIRMED!!!
                // how to write end of stream?

                /*
                process.StandardInput.WriteLine("   @1307532861 approve (152) ");
                process.StandardInput.WriteLine("   @1307955600 approve (163) ");
                process.StandardInput.WriteLine("               publish (160) ");
                process.StandardInput.WriteLine("   @1308477599 approve (187) ");
                process.StandardInput.WriteLine("               publish (163) (152) ");
                */

                /*
                process.StandardInput.Write("   @1307532861 approve (152) \n");
                process.StandardInput.Write("   @1307955600 approve (163) \n");
                process.StandardInput.Write("               publish (160) \n");
                process.StandardInput.Write("   @1308477599 approve (187) \n");
                process.StandardInput.Write("               publish (163) (152) \n");
                */

                while (true)
                {
                    string line = process.StandardOutput.ReadLine();
                    // do something with line
                    Console.WriteLine(line);
                }
            }
            else
            {
                /*
                process.StandardInput.Flush();
                */

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    // do something with line
                    Console.WriteLine(line);
                }
            }
            // process.WaitForExit();

            // process.StandardInput.WriteLine
            //    @2308477599 publish (210)

            Console.WriteLine("end of program");

            return 0;
        }
       

    }

}
