using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimcraftGearOptimizer
{
    public interface IGemmableGearItem : IGearItem, IComparable<IGemmableGearItem>
    {
        void AddRedGem(string gem);
        void AddBlueGem(string gem);
        void AddYellowGem(string gem);
        void AddMetaGem(string gem);
        void ResetGems();

        string ToSimcraft();
    }
}
