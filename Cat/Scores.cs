#define TESTING

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cat.Helpers.BinaryFileHandler;
using static Cat.Scores;

namespace Cat
{
    internal static partial class Scores
    {
        internal static bool LoadR6()
        {
            FPSGame r6 = new FPSGame("Rainbow 6 Siege", 4, 1, 0, 0, []);
            R6Operator[] operators = [
                new("Fuze", 16, 6, [1560, 1175, 1265], true),
                new("Ash", 3, 5, [1720, 1625], true),
                new("Montagne", 9, 2, [1100, 500], true),
                new("Doc", 4, 5, [980, 1800], false),
                new("Mira", 15, 5, [1215, 1160, 920, 2005], false),
                new("Alibi", 5, 3, [760, ], false),
                new("Mozzie", 7, 5, [850,], false),
                new("Dokkaebi", 15, 4, [1400,], true),
                new("Kapkan", 5, 5, [920,], false),
                new("Rook", 3, 5, [890,], false),
            ];

            R6Map[] maps = [
                new("Lair", [2600,], 0, 1),
                new("Bank", [1830,], 1, 0),
                new("Outback", [430,], 0, 1),
                new("NightHaven Labs", [920,], 1, 0),
                new("Kafe Dostoyevsky", [0, ], 0, 0)
            ];


            using (Helpers.BinaryFileHandler bfh = new Helpers.BinaryFileHandler(StatsFile, false))
            {
                foreach (var op in operators)
                    bfh.AddObject(op, 0);
                foreach (var map in maps)
                    bfh.AddObject(map, 1);
            }
            return true;
        }
    }
}
