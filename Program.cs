using System;
using System.Collections.Generic;
using System.IO;

namespace binfilecoder
{
    class Program
    {
        static void Main(string[] argsfromcmd)
        {
            string ByteToHex(byte b)
            {
                int number = (byte)b;
                int b1 = number / 16;
                int b2 = number % 16;
                return "0123456789ABCDEF".Substring(b1, 1) + "0123456789ABCDEF".Substring(b2, 1);
            }

            void ShowInfo()
            {
                Console.WriteLine(
                    "binfilecoder\n" +
                    "by Zach Combs\n" +
                    "\n" +
                    "decode <sourcefile> <destinationfile> [dontplacegaps] [bytesperline=<number>]\n" +
                    "    Converts a binary file into a text file that displays each byte as a hex value\n" +
                    "    sourcefile-       Source file (must exist)\n" +
                    "    destinationfile-  Destination file (must not exist)\n" +
                    "    placegaps-        Don't place gaps between each byte (hex value)\n" +
                    "    bytesperline-     Bytes (hex values) per line any value less than or equal to\n" +
                    "                      zero results in only one line\n" +
                    "\n" +
                    "encode <sourcefile> <destinationfile>\n" +
                    "    Converts a text file (containing hex values) into a binary file\n" +
                    "        Note: A line beginning with '#' is ignored.\n" +
                    "    sourcefile-       Source file (must exist)\n" +
                    "    destinationfile-  Destination file (must not exist)");
            }
            if (argsfromcmd == null)
            {
                ShowInfo();
                return;
            }
            if (argsfromcmd.Length == 0)
            {
                ShowInfo();
                return;
            }

            //Get input from user
            string command = argsfromcmd[0];
            if (command != "decode" & command != "encode")
            {
                Console.WriteLine("ERROR: Unknown command: " + command);
                return;
            }
            if (argsfromcmd.Length < 3)
            {
                Console.WriteLine("ERROR: You must specify a source file and destination file");
                return;
            }
            string fname_source = argsfromcmd[1]; if (!File.Exists(fname_source))
            {
                Console.WriteLine("ERROR: Source file does not exist");
                return;
            }
            string fname_destination = argsfromcmd[2]; if (File.Exists(fname_destination))
            {
                Console.WriteLine("ERROR: Destination file already exists");
                return;
            }
            bool dontplacegaps = false; //Used by decode command
            int bytesperline = 16; //Used by decode command
            for (int n = 3; n < argsfromcmd.Length; n += 1)
            {
                string option = argsfromcmd[n];

                if (option == "dontplacegaps")
                {
                    dontplacegaps = true;
                }
                else
                {
                    string value = "";
                    bool optionhasvalue = false;
                    if (option.IndexOf("=") >= 0)
                    {
                        value = option.Substring(option.IndexOf("=") + 1);
                        option = option.Substring(0, option.IndexOf("="));
                        optionhasvalue = true;
                    }

                    if (option == "bytesperline")
                    {
                        if (!optionhasvalue)
                        {
                            Console.WriteLine("ERROR: You must specify a value for bytesperline");
                            return;
                        }
                        if (!int.TryParse(value, out bytesperline))
                        {
                            Console.WriteLine("ERROR: " + value + " is not a valid value for bytesperline");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Unknown option: " + option);
                        return;
                    }
                }

            }

            if (command == "decode")
            {
                Console.WriteLine("Decode \"{0}\" -> \"{1}\"", fname_source, fname_destination);
                byte[] filebytes = File.ReadAllBytes(fname_source);
                using (StreamWriter strwriter = new StreamWriter(File.OpenWrite(fname_destination)))
                {
                    int byteindexinline = 0;
                    for (int n = 0; n < filebytes.Length; n += 1)
                    {
                        strwriter.Write(ByteToHex(filebytes[n]));
                        if (!dontplacegaps) strwriter.Write(" ");
                        byteindexinline += 1; if (byteindexinline >= bytesperline)
                        {
                            byteindexinline = 0;
                            strwriter.WriteLine();
                        }
                    }
                }
                Console.WriteLine("Done");
            }
            else if (command == "encode")
            {
                Console.WriteLine("Encode \"{0}\" -> \"{1}\"", fname_source, fname_destination);
                List<byte> chars = new List<byte>();
                using (StreamReader strmreader = new StreamReader(fname_source))
                {
                    int lineIndex = 0;
                    while (!strmreader.EndOfStream)
                    {
                        string line = strmreader.ReadLine().Replace(" ", "");
                        if (line.Substring(0, 1) != "#")
                        {
                            char[] lineAsCharyArray = line.ToCharArray();
                            for (int n = 0; n < lineAsCharyArray.Length; n += 1)
                            {
                                char ccc = lineAsCharyArray[n];
                                if ((ccc >= 48 & ccc <= 57) |
                                    (ccc >= 65 & ccc <= 70) |
                                    (ccc >= 97 & ccc <= 102)
                                    ) chars.Add((byte)ccc);
                                else
                                {
                                    Console.WriteLine("ERROR: Line " + (lineIndex + 1).ToString() + "\n" +
                                        "Unexpected character: '" + ccc.ToString() + "'");
                                    return;
                                }
                            }
                        }
                        lineIndex += 1;
                    }
                }
                if ((chars.Count % 1) == 1) chars.Insert(0, 48);
                using (BinaryWriter binwriter = new BinaryWriter(File.OpenWrite(fname_destination)))
                {
                    int filesize = (int)binwriter.BaseStream.Length;
                    for (int n = 0; n < chars.Count; n += 2)
                    {
                        int b1 = (chars[n] >= 48 & chars[n] <= 57) ? (chars[n] - 48) :
                            ((chars[n] >= 65 & chars[n] <= 70) ? (chars[n] - 55) : (chars[n] - 87));
                        int b2 = (chars[n + 1] >= 48 & chars[n + 1] <= 57) ? (chars[n + 1] - 48) :
                            ((chars[n + 1] >= 65 & chars[n + 1] <= 70) ? (chars[n + 1] - 55) : (chars[n + 1] - 87));

                        binwriter.Write((byte)(b1 * 16 + b2));
                    }
                }
                Console.WriteLine("Done");
            }
        }
    }
}
