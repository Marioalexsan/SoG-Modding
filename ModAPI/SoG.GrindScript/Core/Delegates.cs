using Microsoft.Xna.Framework.Content;

namespace SoG.Modding
{
    public delegate void CommandParser(string argList, int connection);

    public delegate void LevelBlueprintBuilder(LevelBlueprint blueprint);

    public delegate void LevelLoader(LevelBlueprint blueprint);
}
