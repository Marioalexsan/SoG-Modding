using Microsoft.Xna.Framework.Content;

namespace SoG.Modding.Core
{
    public delegate void CommandParser(string argList, int connection);

    public delegate void LevelBuilder(LevelBlueprint blueprint);

    public delegate void LevelLoader(bool staticOnly);
}
