using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace SoG.GrindScript
{
    public delegate void EnemyBuilderPrototype(Enemy xEnemy);

    public delegate DynamicEnvironment DynEnvBuilderPrototype(ContentManager xContent);
}
