using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solskogen2017.GRAPHICS;
using Solskogen2017.DIAGNOSTICS;

namespace Solskogen2017.EFFECTS
{
    class Part
    {
        public enum RenderPass
        {
            DIFFUSE,
            SHADOW,
            UNK,
            NORMAL
        }
        public virtual void tick(float dt, float time)
        {

        }
        public virtual void render(Context context, RenderPass pass, float dt, float time)
        {
            Debug.DebugConsole("Part", "WARNING: Part base render is called");
        }
    }
}
