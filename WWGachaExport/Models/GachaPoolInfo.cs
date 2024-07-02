using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWGachaExport.Models
{
    public class GachaPoolInfo
    {
        public int PoolType;
        public string Name;
        public bool NoobPool;
        public int LevelFiveMaxDraw;
        public int LevelFourMaxDraw;

        public GachaPoolInfo(int poolType, string name, bool noobPool, int levelFiveMaxDraw, int levelFourMaxDraw)
        {
            PoolType = poolType;
            Name = name;
            NoobPool = noobPool;
            LevelFiveMaxDraw = levelFiveMaxDraw;
            LevelFourMaxDraw = levelFourMaxDraw;
        }
    }
}
