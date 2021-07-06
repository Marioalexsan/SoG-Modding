using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SoG.Modding.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.Modding.Patches
{
    /// <summary>
    /// Holds random state, code and stuff that is used in specialized patches.
    /// </summary>
    public abstract class PatchStateWorker
    {
        protected GrindScript _modAPI;

        public string ID { get; private set; } = "";

        public PatchStateWorker(GrindScript modAPI, string id = "")
        {
            ID = id;
            _modAPI = modAPI;
        }
    }

    /// <summary>
    /// Computes the start and end of the TreatCurse menu items
    /// </summary>
    public class TCMenuWorker : PatchStateWorker
    {
        public TCMenuWorker(GrindScript modAPI, string id = "")
            : base(modAPI, id) { }

        private ShopMenu _shop => _modAPI.Game.xShopMenu;

        private int _topRow = 0;

        public int TCListStart { get; private set; } = 0;

        public int TCListEnd { get; private set; } = 0;

        public void Update()
        {
            int currentRow = _shop.iShopPosition / 5;

            if (currentRow < _topRow)
                _topRow = currentRow;

            if (currentRow > _topRow + 1)
                _topRow = currentRow - 1;

            TCListStart = _topRow * 5;
            TCListEnd = Math.Min(_shop.xTreatCurseMenu.lenTreatCursesAvailable.Count, TCListStart + 10);
        }

        public void DrawScroller(SpriteBatch spriteBatch, float scale, float alpha)
        {
            int totalRows = (_shop.xTreatCurseMenu.lenTreatCursesAvailable.Count - 1) / 5 + 1;
            if (totalRows <= 2)
                return;

            float scrollHeight = 105;
            int rowStepSize = (int)(scrollHeight / totalRows);
            int scrollerSize = 2 * rowStepSize;
            int offset = _topRow * rowStepSize;
            spriteBatch.Draw(ChallengeMenu.txScrollTop, new Vector2(518, 98 + offset), null, Microsoft.Xna.Framework.Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(ChallengeMenu.txScrollMid, new Vector2(518, 99 + offset), new Microsoft.Xna.Framework.Rectangle(0, 0, 5, scrollerSize - 2), Microsoft.Xna.Framework.Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(ChallengeMenu.txScrollBot, new Vector2(518, 99 + offset + scrollerSize - 2), null, Microsoft.Xna.Framework.Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
