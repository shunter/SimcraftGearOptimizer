using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimcraftGearOptimizer
{
    public interface IGearItem : IComparable<IGearItem>
    {
        GearSlot Slot { get; }
        string Name { get; }
        int RedSockets { get; }
        int YellowSockets { get; }
        int BlueSockets { get; }
        int MetaSockets { get; }
        string ToSimcraft(string gemStr);

        IGemmableGearItem MakeGemmable();
        IGearItem WithSlotSuffix(string slotSuffix);
    }
}
