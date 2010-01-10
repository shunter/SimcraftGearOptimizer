using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using SimcraftCLI;

namespace SimcraftGearOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            GearDatabase database = GearDatabase.Initialize();

            var combinations = (from back in database.Back.AsParallel()
                                from chest in database.Chest
                                from feet in database.Feet
                                from finger1 in database.Finger
                                from finger2 in database.Finger
                                where finger1.CompareTo(finger2) < 0
                                from hands in database.Hands
                                from head in database.Head
                                from legs in database.Legs
                                from mainHand in database.MainHand
                                from neck in database.Neck
                                from offHand in database.OffHand
                                from ranged in database.Ranged
                                from shoulders in database.Shoulders
                                from trinket1 in database.Trinket
                                from trinket2 in database.Trinket
                                where trinket1.CompareTo(trinket2) < 0
                                from waist in database.Waist
                                from wrists in database.Wrists
                                select new HashSet<IGemmableGearItem>
                                {
                                    back.MakeGemmable(),
                                    chest.MakeGemmable(),
                                    feet.MakeGemmable(),
                                    finger1.WithSlotSuffix("1").MakeGemmable(),
                                    finger2.WithSlotSuffix("2").MakeGemmable(),
                                    hands.MakeGemmable(),
                                    head.MakeGemmable(),
                                    legs.MakeGemmable(),
                                    mainHand.MakeGemmable(),
                                    neck.MakeGemmable(),
                                    offHand.MakeGemmable(),
                                    ranged.MakeGemmable(),
                                    shoulders.MakeGemmable(),
                                    trinket1.WithSlotSuffix("1").MakeGemmable(),
                                    trinket2.WithSlotSuffix("2").MakeGemmable(),
                                    waist.MakeGemmable(),
                                    wrists.MakeGemmable(),
                                });

            var dpsRegex = new Regex("  DPS: ([0-9.]+)");
            var maxDps = 0d;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var setCount = combinations.Count();
            Console.WriteLine("considering {0} sets", setCount);
            sw.Stop();
            Console.WriteLine("counting sets took {0}", sw.Elapsed);

            var highestSet = combinations.First();

            object resultTabulationLock = new object();
            int completed = 0;

            Action<double, HashSet<IGemmableGearItem>> tabulateResults =
                (dps, gearset) =>
                {
                    lock (resultTabulationLock)
                    {
                        if (dps > maxDps)
                        {
                            Console.WriteLine("found set with {0} dps, a new maximum", dps);
                            maxDps = dps;
                            highestSet = gearset;
                        }
                        
                        completed++;
                        if (completed % 500 == 0)
                        {
                            var elapsed = sw.Elapsed;
                            var setsPerSec = completed / (double) elapsed.TotalSeconds;
                            var eta = TimeSpan.FromSeconds(setCount / setsPerSec);
                            Console.WriteLine("{0} sets completed in {1}, {2} sets/sec, ETA {3}.", completed, elapsed, setsPerSec, eta);
                        }
                    }
                };

            List<string> defaults = new List<string>();
            using (TextReader reader = new StreamReader(@"Data\default.simc"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    defaults.Add(line);
                }
             }
            
            Action<HashSet<IGemmableGearItem>> a = gearset =>
                {
                    FillGems(gearset);

                    var options = defaults.Union(gearset.Select(i => i.ToSimcraft())).ToArray();

                    Simcraft s = new Simcraft();
                    double dps = s.RunSim(options);
                    tabulateResults(dps, gearset);
                };

            sw.Start();
            combinations.ForAll(a);

            Console.WriteLine("max dps: " + maxDps);
            Console.WriteLine(string.Join(Environment.NewLine, highestSet.Select(i => string.Format("{0}={1}", i.Slot, i.Name)).ToArray()));
        }

        private const string MetaGem = "chaotic_skyflare";
        private const string PurpleGem = "12sp_10spi";
        private const string RedGem = "23sp";
        private const string OrangeGem = "10haste_12sp";

        private static void FillGems(HashSet<IGemmableGearItem> gearset)
        {
            foreach (var item in gearset)
            {
                item.ResetGems();
            }

            var headItem = gearset.Where(i => i.Slot == GearSlot.Head).Single();
            bool hasMeta = headItem.MetaSockets > 0;

            if (hasMeta)
            {
                headItem.AddMetaGem(MetaGem);
            }

            int bluesLeft = 2;

            foreach (var item in gearset.Where(i => i.BlueSockets > 0))
            {
                for (int i = 0; i < item.BlueSockets; ++i)
                {
                    if (hasMeta && bluesLeft > 0)
                    {
                        item.AddBlueGem(PurpleGem);
                        bluesLeft--;
                    }
                    else
                    {
                        item.AddBlueGem(RedGem);
                    }
                }
            }

            foreach (var item in gearset.Where(i => i.RedSockets > 0))
            {
                for (int i = 0; i < item.RedSockets; ++i)
                {
                    if (hasMeta && bluesLeft > 0)
                    {
                        item.AddRedGem(PurpleGem);
                        bluesLeft--;
                    }
                    else
                    {
                        item.AddRedGem(RedGem);
                    }
                }
            }

            foreach (var item in gearset.Where(i => i.YellowSockets > 0))
            {
                for (int i = 0; i < item.YellowSockets; ++i)
                {
                    if (hasMeta && bluesLeft > 0)
                    {
                        item.AddYellowGem(PurpleGem);
                        bluesLeft--;
                    }
                    else
                    {
                        item.AddYellowGem(OrangeGem);
                    }
                }
            }
        }
    }
}
