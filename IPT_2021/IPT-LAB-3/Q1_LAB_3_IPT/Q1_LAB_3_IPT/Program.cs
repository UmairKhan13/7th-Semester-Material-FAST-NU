using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace k181292_Q1
{
    class Program
    {
        public static int count = 0;
        public static void search(int[] arr, int toSearch)
        {
            Console.WriteLine("Single thread search");
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == toSearch)
                {
                    Console.WriteLine("{0} found on index {1}", toSearch, i);
                }
            }
            watch.Stop();
            Console.WriteLine("Elapsed time: {0}", watch.Elapsed);
        }

        public static void Searcher(int toSearch, int startIndex, int endIndex, int[] arr)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (arr[i] == toSearch) Console.WriteLine("{0} found on index {1}", toSearch, i);
            }
        }

        public static void threadSearch(int[] arr, int toSearch)
        {
            Console.WriteLine("Multi thread Search: ");
            var watch = new Stopwatch();
            watch.Start();
            int numberOfThreads = 5;
            int range = arr.Length / numberOfThreads;
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread searcher;
                if (i == numberOfThreads - 1)
                {
                    searcher = new Thread(() => { Searcher(toSearch, i * range, arr.Length - 1, arr); });
                }
                else
                {
                    searcher = new Thread(() => { Searcher(toSearch, i * range, i * range + range - 1, arr); });
                }
                searcher.Start();
            }
            watch.Stop();
            Console.WriteLine("Elapsed time: {0}", watch.Elapsed);
        }

        static void Main(string[] args)
        {
            Random rnd = new Random();
            int[] array = new int[1000000];
            for (int i = 0; i < array.Length-1; i++)
            {
                array[i] = rnd.Next(1, 1000000);
            }
            for (int i = 0; i < array.Length-1; i++)
            {
                array[i] = rnd.Next(0, 1000000);
            }
            int toSearch = rnd.Next(0, 1000000);
            Console.WriteLine("Term to search: {0}", toSearch);
            search(array, toSearch);
            Console.WriteLine();
            threadSearch(array, toSearch);
            Console.WriteLine();
        }
    }
}