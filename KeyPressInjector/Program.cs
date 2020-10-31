using System;
using System.IO;

namespace KeyPressInjector
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCP Secret Laboratory");
            var file = Path.Combine(path, "cmdbinding.txt");
            if (!File.Exists(file))
            {
                Console.WriteLine("Couldn't find cmdbinding file.");
                End();
                return;
            }

            try
            {
                File.WriteAllText(file, "0:.keypress send 0\n8:.keypress send 8\n9:.keypress send 9\n12:.keypress send 12\n13:.keypress send 13\n19:.keypress send 19\n27:.keypress send 27\n32:.keypress send 32\n33:.keypress send 33\n34:.keypress send 34\n35:.keypress send 35\n36:.keypress send 36\n37:.keypress send 37\n38:.keypress send 38\n39:.keypress send 39\n40:.keypress send 40\n41:.keypress send 41\n42:.keypress send 42\n43:.keypress send 43\n44:.keypress send 44\n45:.keypress send 45\n46:.keypress send 46\n47:.keypress send 47\n48:.keypress send 48\n49:.keypress send 49\n50:.keypress send 50\n51:.keypress send 51\n52:.keypress send 52\n53:.keypress send 53\n54:.keypress send 54\n55:.keypress send 55\n56:.keypress send 56\n57:.keypress send 57\n58:.keypress send 58\n59:.keypress send 59\n60:.keypress send 60\n61:.keypress send 61\n62:.keypress send 62\n63:.keypress send 63\n64:.keypress send 64\n91:.keypress send 91\n92:.keypress send 92\n93:.keypress send 93\n94:.keypress send 94\n95:.keypress send 95\n96:.keypress send 96\n97:.keypress send 97\n98:.keypress send 98\n99:.keypress send 99\n100:.keypress send 100\n101:.keypress send 101\n102:.keypress send 102\n103:.keypress send 103\n104:.keypress send 104\n105:.keypress send 105\n106:.keypress send 106\n107:.keypress send 107\n108:.keypress send 108\n109:.keypress send 109\n110:.keypress send 110\n111:.keypress send 111\n112:.keypress send 112\n113:.keypress send 113\n114:.keypress send 114\n115:.keypress send 115\n116:.keypress send 116\n117:.keypress send 117\n118:.keypress send 118\n119:.keypress send 119\n120:.keypress send 120\n121:.keypress send 121\n122:.keypress send 122\n123:.keypress send 123\n124:.keypress send 124\n125:.keypress send 125\n126:.keypress send 126\n127:.keypress send 127\n256:.keypress send 256\n257:.keypress send 257\n258:.keypress send 258\n259:.keypress send 259\n260:.keypress send 260\n261:.keypress send 261\n262:.keypress send 262\n263:.keypress send 263\n264:.keypress send 264\n265:.keypress send 265\n266:.keypress send 266\n267:.keypress send 267\n268:.keypress send 268\n269:.keypress send 269\n270:.keypress send 270\n271:.keypress send 271\n272:.keypress send 272\n273:.keypress send 273\n274:.keypress send 274\n275:.keypress send 275\n276:.keypress send 276\n277:.keypress send 277\n278:.keypress send 278\n279:.keypress send 279\n280:.keypress send 280\n281:.keypress send 281\n282:.keypress send 282\n283:.keypress send 283\n284:.keypress send 284\n285:.keypress send 285\n286:.keypress send 286\n287:.keypress send 287\n288:.keypress send 288\n289:.keypress send 289\n290:.keypress send 290\n291:.keypress send 291\n292:.keypress send 292\n293:.keypress send 293\n294:.keypress send 294\n295:.keypress send 295\n296:.keypress send 296\n300:.keypress send 300\n301:.keypress send 301\n302:.keypress send 302\n303:.keypress send 303\n304:.keypress send 304\n305:.keypress send 305\n306:.keypress send 306\n307:.keypress send 307\n308:.keypress send 308\n309:.keypress send 309\n310:.keypress send 310");
                Console.WriteLine("Installed all CmdBinding.\nYou can now use Synapse's KeyPress System!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Occured while Writing to File: \n\n{e}");
            }

            End();
        }

        private static void End()
        {
            Console.WriteLine("Press any Key to close the Console ...");
            Console.ReadKey();
        }
    }
}
