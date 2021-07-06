using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Core;
using SoG.Modding.Utils;

namespace SoG.Modding.Content.Configs
{
    // TODO : Rework / update / whatever this class
    public class AnimationConfig
    {
        private Directions _direction;

        public ContentManager Manager { get; set; } = APIGlobals.Game.Content;

        public string TexturePath { get; set; } = "";

        public ushort ID { get; set; } = 0;

        public int TicksPerFrame { get; set; } = 4;

        public int EndFrame { get; set; } = 1;

        public int CellWidth { get; set; } = 1;

        public int CellHeight { get; set; } = 1;

        public int FramesPerRow { get; set; } = 1;

        public bool ReversePlayback { get; set; } = false;

        public float BaseSpeed { get; set; } = 1f;

        public int LoopCount { get; set; } = 0;

        public Vector2 PositionOffset { get; set; } = Vector2.Zero;

        public List<AnimationInstruction> Instructions { get; } = new List<AnimationInstruction>();

        public Animation.LoopSettings LoopType { get; set; } = Animation.LoopSettings.ReturnToIdle;

        public Animation.CancelOptions CancelType { get; set; } = Animation.CancelOptions.RestartIfPlaying;

        public bool SpellCancellable { get; set; } = true;

        public bool MoveCancellable { get; set; } = true;

        public Directions Direction { get => _direction; set => _direction = Enum.IsDefined(typeof(Directions), value) ? value : Directions.Down; }

        public void AddInstructions(params AnimationInstruction[] instructions)
        {
            Instructions.AddRange(instructions);
        }

        public Animation Build()
        {
            List<AnimationInstruction> instructions = new List<AnimationInstruction>();
            Instructions.ForEach(x => instructions.Add(x.Clone()));

            Texture2D tex = Utils.Tools.TryLoadTex(TexturePath, Manager);
            Animation proto = new Animation(ID, (byte)Direction, tex, PositionOffset, TicksPerFrame, EndFrame, CellWidth, CellHeight, 0, 0, FramesPerRow, LoopType, CancelType, MoveCancellable, SpellCancellable);

            return proto;
        }
    }
}
