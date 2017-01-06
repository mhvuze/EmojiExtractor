using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EmojiExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Variables
            string input = "";
            int min_width = 0;
            int count = 1;
            int skip_count = 0;
            long last_start = 0;
            long last_end = 0;
            uint width = 0;
            int size = 0;

            // Print header
            Console.WriteLine("=========================");
            Console.WriteLine("EmojiExtractor by MHVuze");
            Console.WriteLine("=========================");

            // Handle arguments
            if (args.Length < 2) { Console.WriteLine("ERROR: Please specify input file and minimum width."); return; }
            input = args[0];
            bool parse = int.TryParse(args[1], out min_width);

            // Check arguments
            if (!File.Exists(input)) { Console.WriteLine("ERROR: Specified input file doesn't exist."); return; }
            if (parse == false) { Console.WriteLine("ERROR: Specified minimum width could not be parsed."); return; }

            // Extraction routine
            using (BinaryReader reader = new BinaryReader(File.OpenRead(input)))
            {
                // Check file magic
                if (reader.ReadInt32() != 0x66637474) { Console.WriteLine("ERROR: Specified input file is not a valid ttc font."); return; }

                // Create output directory next to input file
                string folder_path = new FileInfo(input).Directory.FullName + "\\extracted\\";
                if (!Directory.Exists(folder_path)) { Directory.CreateDirectory(folder_path); }

                // Scan for png
                Console.WriteLine("Working... please be patient. This may take a few minutes.");
                int read = 0;

                // Search for file start
                while (reader.BaseStream.Position < reader.BaseStream.Length - 4)
                {
                    read = reader.ReadInt32();

                    if (read == 0x474E5089)
                    {
                        //Console.WriteLine("Found png file #{0} at 0x{1}.", count.ToString("D5"), (reader.BaseStream.Position + 4).ToString("X8"));

                        last_start = reader.BaseStream.Position - 4;

                        // Get image dimensions
                        reader.BaseStream.Seek(0x0C, SeekOrigin.Current);
                        width = swapEndianness(reader.ReadUInt32());

                        if (width > min_width)
                        {
                            // Search for file end
                            while ((read != 0x444E4549) && (reader.BaseStream.Position != reader.BaseStream.Length))
                            {
                                read = reader.ReadInt32();
                                reader.BaseStream.Seek(-3, SeekOrigin.Current);
                            }
                            last_end = reader.BaseStream.Position + 4;

                            // Read and write file buffer
                            size = Convert.ToInt32((last_end) - (last_start));
                            reader.BaseStream.Seek(last_start, SeekOrigin.Begin);
                            byte[] file_array = reader.ReadBytes(size);
                            File.WriteAllBytes(folder_path + count.ToString("D5") + ".png", file_array);
                        }
                        else
                        {
                            //Console.WriteLine("Skipped due to dimensions.");
                            skip_count++;
                        }                        
                        count++;
                    }
                    else
                    {
                        reader.BaseStream.Seek(-3, SeekOrigin.Current);
                    }
                }
            }

            // End
            Console.WriteLine("Found {0} Emojis in total. {1} matched your filter and have been extracted.", count - 1, count - 1 - skip_count);
        }

        // from http://stackoverflow.com/a/3294698/5343630
        private static uint swapEndianness(uint x)
        {
            return ((x & 0x000000ff) << 24) +  // First byte
                   ((x & 0x0000ff00) << 8) +   // Second byte
                   ((x & 0x00ff0000) >> 8) +   // Third byte
                   ((x & 0xff000000) >> 24);   // Fourth byte
        }
    }
}
