using System;
using BCDLib;
using System.IO;
using System.Collections.Generic;

namespace BCDComp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            BCD a = BCD.Parse("10000000");
            BCD b = BCD.Parse("-1");
            BCD c = BCD.Parse("3");

            BCD ans = a / c;

            Console.WriteLine($"ans={ans}");


            List<BCD> num = new List<BCD>();
            num.Add(BCD.Parse("2"));

            for (BCD i = BCD.Parse("2"); BCD.Parse("100") > i; i+=BCD.One)
            {
                BCD an = BCD.Zero;

                foreach (BCD n in num)
                {
                    BCD ab = i / n;
                    if (!BCD.IsZero(ab.Rem))
                        an = i;
                }
                if (!BCD.IsZero(an))
                    Console.WriteLine(an);
            }

            BCD aq = BCD.One;

            for (BCD bi = BCD.One; BCD.Parse("10000") > aq; bi+=BCD.One)
            {
                aq *= bi;
                Console.WriteLine(aq);
            }

            Console.WriteLine(a);
            Console.WriteLine(b);

            Console.WriteLine(a - b + c);

            for (int i = -5; 15 >= i; i++)
                for (int j = -3; 15 >= j; j++)
                {
                    Console.WriteLine($"{j} * {i} = {BCD.Parse(j.ToString()) * BCD.Parse(i.ToString())}");
                }

            var sw = Console.Out;
            try
            {
                //new StreamWriter(@"D:\workspace\TEST_FACTORIAL2\factorial-A1.txt");
                BCD ans1 = BCD.Parse("1");
                long f = 100;
                for (long i = 1; f >= i; i++)
                {
                    if (i % 10000 == 0)
                    {
                        sw.Flush();
                        sw.Close();
                        //sw = new StreamWriter($@"D:\workspace\TEST_FACTORIAL2\factorial-A{i}.txt");
                        Console.Write('■');
                    }
                    ans1 = ans1 * BCD.Parse(i.ToString());
                    sw.WriteLine($"{i}! = {ans1}");
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("stop.");
            }
            finally
            {
                sw.Flush();
                sw.Close();
                Console.ReadLine();
            }


            BCD az = BCD.Zero;
            int len = 100;
            for (int i = 0; len > i; i++)
            {
                if (i % (len / 10) == 0)
                    Console.Write('■');
                az = az + BCD.Parse(i.ToString());

            }
            Console.WriteLine(az);
        }
    }
}
