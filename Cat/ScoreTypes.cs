using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Scores
    {
        internal record class FPSGame(string name, int kills, int deaths, int wins, int losses, List<object> remarks);

        internal record class CardGame(string name, int wins, int losses, List<object> remarks);

        internal record class R6Operator(string name, int kills, int deaths, List<int> scores, bool attack);
    
        internal record class R6Map(string name, List<int> scores, int wins, int losses);
    }
}
