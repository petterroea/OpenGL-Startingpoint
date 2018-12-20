using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solskogen2017.BACKGROUND
{
    public class BackgroundManager
    {
        private static TEXTURE.Texture tex;
        public BackgroundManager()
        {

        }
        public TEXTURE.Texture GetTexture()
        {
            return tex;
        }
    }
}
