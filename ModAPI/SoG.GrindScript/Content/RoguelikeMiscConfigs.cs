using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public class TreatCurseConfig
    {
        public TreatCurseConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        bool _isTreat = false;

        public bool IsTreat 
        {
            get => _isTreat;
            set => _isTreat = value;
        }

        public bool IsCurse
        {
            get => !_isTreat;
            set => _isTreat = !value;
        }

        public string TexturePath { get; set; } = "";

        public string Name { get; set; } = "Candy's Shenanigans";

        public string Description { get; set; } = "It's a mysterious treat or curse!";

        public string ModID { get; set; } = "";

        public float ScoreModifier { get; set; } = 0f;
    }

    public class PerkConfig
    {
        public PerkConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public Action<PlayerView> RunStartActivator { get; set; } = null;

        public int EssenceCost { get; set; } = 15;

        public string Name { get; set; } = "Bishop's Shenanigans";

        public string Description { get; set; } = "It's some weird perk or moldable!";

        public string TexturePath { get; set; } = "";

        public string ModID { get; set; } = "";
    }
}
