using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace k181292_Q2
{
    class Player
    {
        public string name;
        public int XP;

        public Player(string name, int XP)
        {
            this.name = name;
            this.XP = XP;
        }
    }

    class Program
    {
        static void executeThread(Player curr)
        {
            Console.WriteLine("{0} have weapon. {1} XP", curr.name, curr.XP);
            Console.WriteLine("{0} released weapon", curr.name);
            Thread.Yield();
        }
        static void Main(string[] args)
        {
            List<Player> players = new List<Player>
            {
                new Player("Umair", 5),
                new Player("Anas", 4),
                new Player("Mujtaba", 7),
                new Player("Moeiz", 6),
                new Player("Farzeel", 3),
            };
            players = players.OrderByDescending(x => x.XP).ToList<Player>();
            foreach (Player p in players)
            {
                Console.WriteLine("{0} {1}", p.name, p.XP);
            }
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(4, 4);
            Parallel.ForEach(players, (item) => executeThread(item));
        }

    }
}