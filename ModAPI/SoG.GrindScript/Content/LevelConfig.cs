using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public class LevelConfig
    {
        LevelBuilder _builder;
        LevelLoader _loader;
        string _defaultMusic;
        Level.WorldRegion _region = Level.WorldRegion.NotLoaded;

        public LevelConfig Builder(LevelBuilder builder)
        {
            _builder = builder;
            return this;
        }

        public LevelConfig Loader(LevelLoader loader)
        {
            _loader = loader;
            return this;
        }

        public LevelConfig MusicToPlay(string music)
        {
            _defaultMusic = music;
            return this;
        }

        public LevelConfig WorldRegion(Level.WorldRegion region)
        {
            _region = region;
            return this;
        }

        internal ModLevel CreateLevel(BaseScript owner, Level.ZoneEnum allocatedType)
        {
            return new ModLevel()
            {
                builder = _builder,
                loader = _loader,
                defaultMusic = _defaultMusic,
                region = _region,
                type = allocatedType
            };
        }
    }
}
