using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimcraftGearOptimizer
{
    public class GearDatabase
    {
        private const string DataDir = @"Data";
        private ILookup<GearSlot, IGearItem> items;

        private GearDatabase()
        {
        }

        public IEnumerable<IGearItem> Back
        {
            get
            {
                return items[GearSlot.Back];
            }
        }

        public IEnumerable<IGearItem> Chest
        {
            get
            {
                return items[GearSlot.Chest];
            }
        }

        public IEnumerable<IGearItem> Feet
        {
            get
            {
                return items[GearSlot.Feet];
            }
        }

        public IEnumerable<IGearItem> Finger
        {
            get
            {
                return items[GearSlot.Finger];
            }
        }

        public IEnumerable<IGearItem> Hands
        {
            get
            {
                return items[GearSlot.Hands];
            }
        }

        public IEnumerable<IGearItem> Head
        {
            get
            {
                return items[GearSlot.Head];
            }
        }

        public IEnumerable<IGearItem> Legs
        {
            get
            {
                return items[GearSlot.Legs];
            }
        }

        public IEnumerable<IGearItem> MainHand
        {
            get
            {
                return items[GearSlot.MainHand];
            }
        }

        public IEnumerable<IGearItem> Neck
        {
            get
            {
                return items[GearSlot.Neck];
            }
        }

        public IEnumerable<IGearItem> OffHand
        {
            get
            {
                return items[GearSlot.OffHand];
            }
        }

        public IEnumerable<IGearItem> Ranged
        {
            get
            {
                return items[GearSlot.Ranged];
            }
        }

        public IEnumerable<IGearItem> Shoulders
        {
            get
            {
                return items[GearSlot.Shoulders];
            }
        }

        public IEnumerable<IGearItem> Trinket
        {
            get
            {
                return items[GearSlot.Trinket];
            }
        }

        public IEnumerable<IGearItem> Waist
        {
            get
            {
                return items[GearSlot.Waist];
            }
        }

        public IEnumerable<IGearItem> Wrists
        {
            get
            {
                return items[GearSlot.Wrists];
            }
        }

        internal static GearDatabase Initialize()
        {
            var result = new GearDatabase();

            var items = new List<IGearItem>();
            var loader = new GearItemLoader(Path.Combine(DataDir, "Cache"));

            using (TextReader reader = new StreamReader(Path.Combine(DataDir, "items.txt")))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    var itemid = line.Split(',')[0];
                    var gearItem = loader.LoadGearItem(itemid);
                    if (gearItem != null)
                        items.Add(gearItem);
                }
            }

            result.items = items.ToLookup(i => i.Slot, i => i);

            Console.WriteLine("Database initialized with {0} items:", items.Count);

            foreach (var item in items)
            {
                Console.WriteLine(item.Name);
            }

            return result;
        }
    }
}
