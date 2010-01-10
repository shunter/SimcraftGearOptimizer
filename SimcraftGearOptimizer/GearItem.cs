using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using SimcraftCLI;

namespace SimcraftGearOptimizer
{
    public class GearItem : IGearItem
    {
        private GearItem()
        {
        }

        public GearSlot Slot { get; private set; }
        public string Name { get; private set; }
        public int RedSockets { get; private set; }
        public int YellowSockets { get; private set; }
        public int BlueSockets { get; private set; }
        public int MetaSockets { get; private set; }

        private int Id { get; set; }
        private int ILevel { get; set; }
        private string OptionsStr { get; set; }

        private static readonly Regex NameSplitter = new Regex(@"(?<!^)(?=[A-Z])");

        public string ToSimcraft(string gemsStr)
        {
            return ToSimcraft("", gemsStr);
        }

        private string ToSimcraft(string slotSuffix, string gemsStr)
        {
            var slot = string.Join("_", NameSplitter.Split(Slot.ToString())).ToLowerInvariant();

            return string.Format("{0}{1}={2}{3}", slot, slotSuffix, OptionsStr, gemsStr);
        }

        public int CompareTo(IGearItem other)
        {
            return Name.CompareTo(other.Name);
        }

        public static GearItem LoadFrom(XmlElement element, string rawXML)
        {
            var result = new GearItem();

            var itemTooltip = element.SelectSingleNode("//itemTooltip");
            result.Name = ParseText(itemTooltip, "name");
            result.Id = ParseInt(itemTooltip, "id");
            result.ILevel = ParseInt(itemTooltip, "itemLevel");

            var equipData = itemTooltip.SelectSingleNode("equipData");
            if (equipData != null)
            {
                result.Slot = (GearSlot)ParseInt(equipData, "inventoryType");
            }

            var sockets = itemTooltip.SelectNodes("socketData/socket");
            foreach (XmlNode socket in sockets)
            {
                switch (socket.Attributes["color"].Value)
                {
                    case "Red":
                        result.RedSockets++;
                        break;
                    case "Yellow":
                        result.YellowSockets++;
                        break;
                    case "Blue":
                        result.BlueSockets++;
                        break;
                    case "Meta":
                        result.MetaSockets++;
                        break;
                }
            }

            if (result.Slot == GearSlot.Waist)
                result.BlueSockets++;

            result.OptionsStr = new Simcraft().GetItem(rawXML);

            return result;
        }

        private static string ParseText(XmlNode node, string xpath)
        {
            var subNode = node.SelectSingleNode(xpath);
            return subNode != null ? subNode.InnerText : null;
        }

        private static int ParseInt(XmlNode node, string xpath)
        {
            int result = 0;
            var text = ParseText(node, xpath);
            if (text != null)
            {
                int.TryParse(text, out result);
            }
            return result;
        }

        public IGemmableGearItem MakeGemmable()
        {
            return new GemmableGearItem(this);
        }

        private class GemmableGearItem : IGemmableGearItem
        {
            private readonly IGearItem gearItem;
            private readonly List<string> redGems;
            private readonly List<string> yellowGems;
            private readonly List<string> blueGems;
            private readonly List<string> metaGems;

            public GemmableGearItem(IGearItem gearItem)
            {
                this.gearItem = gearItem;
                redGems = new List<string>(gearItem.RedSockets);
                yellowGems = new List<string>(gearItem.YellowSockets);
                blueGems = new List<string>(gearItem.BlueSockets);
                metaGems = new List<string>(gearItem.MetaSockets);
            }

            public string ToSimcraft()
            {
                var gemsStr = "";
                var allGems = redGems.Union(yellowGems).Union(blueGems).Union(metaGems);
                if (allGems.Any())
                {
                    gemsStr = string.Format(",gems={0}", string.Join("_", allGems.ToArray()));
                }
                return gearItem.ToSimcraft(gemsStr);
            }

            public string ToSimcraft(string gemsStr)
            {
                throw new NotImplementedException();
            }

            public void AddRedGem(string gem)
            {
                if (redGems.Count >= gearItem.RedSockets)
                    throw new ArgumentException();
                redGems.Add(gem);
            }

            public void AddBlueGem(string gem)
            {
                if (blueGems.Count >= gearItem.BlueSockets)
                    throw new ArgumentException();
                blueGems.Add(gem);
            }

            public void AddYellowGem(string gem)
            {
                if (yellowGems.Count >= gearItem.YellowSockets)
                    throw new ArgumentException();
                yellowGems.Add(gem);
            }

            public void AddMetaGem(string gem)
            {
                if (metaGems.Count >= gearItem.MetaSockets)
                    throw new ArgumentException();
                metaGems.Add(gem);
            }

            public void ResetGems()
            {
                redGems.Clear();
                blueGems.Clear();
                yellowGems.Clear();
                metaGems.Clear();
            }

            public GearSlot Slot
            {
                get { return gearItem.Slot; }
            }

            public string Name
            {
                get { return gearItem.Name; }
            }

            public int RedSockets
            {
                get { return gearItem.RedSockets; }
            }

            public int YellowSockets
            {
                get { return gearItem.YellowSockets; }
            }

            public int BlueSockets
            {
                get { return gearItem.BlueSockets; }
            }

            public int MetaSockets
            {
                get { return gearItem.MetaSockets; }
            }

            public int CompareTo(IGearItem other)
            {
                return Name.CompareTo(other.Name);
            }

            public int CompareTo(IGemmableGearItem other)
            {
                return Name.CompareTo(other.Name);
            }

            public IGearItem WithSlotSuffix(string slotSuffix)
            {
                throw new NotImplementedException();
            }

            public IGemmableGearItem MakeGemmable()
            {
                return this;
            }
        }

        public IGearItem WithSlotSuffix(string slotSuffix)
        {
            return new SlotSuffixGearItem(this, slotSuffix);
        }

        private class SlotSuffixGearItem : IGearItem
        {
            private readonly string slotSuffix;
            private readonly GearItem gearItem;

            public SlotSuffixGearItem(GearItem gearItem, string slotSuffix)
            {
                this.gearItem = gearItem;
                this.slotSuffix = slotSuffix;
            }

            public GearSlot Slot
            {
                get { return gearItem.Slot; }
            }

            public string Name
            {
                get { return gearItem.Name; }
            }

            public int RedSockets
            {
                get { return gearItem.RedSockets; }
            }

            public int YellowSockets
            {
                get { return gearItem.YellowSockets; }
            }

            public int BlueSockets
            {
                get { return gearItem.BlueSockets; }
            }

            public int MetaSockets
            {
                get { return gearItem.MetaSockets; }
            }

            public string ToSimcraft(string gemsStr)
            {
                return gearItem.ToSimcraft(slotSuffix, gemsStr);
            }

            public int CompareTo(IGearItem other)
            {
                return gearItem.CompareTo(other);
            }

            public IGearItem WithSlotSuffix(string slotSuffix)
            {
                return gearItem.WithSlotSuffix(slotSuffix);
            }

            public IGemmableGearItem MakeGemmable()
            {
                return new GemmableGearItem(this);
            }
        }


        public static bool IsValidItem(XmlElement docElement)
        {
            var itemTooltip = docElement.SelectSingleNode("//itemTooltip");
            if (itemTooltip == null)
                return false;

            if (ParseInt(itemTooltip, "itemLevel") < 200)
                return false;

            var equipData = itemTooltip.SelectSingleNode("equipData");
            if (equipData != null)
            {
                if (ParseInt(equipData, "inventoryType") == 0)
                    return false;
            }

            return true;
        }
    }
}

