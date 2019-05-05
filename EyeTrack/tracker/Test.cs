using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeTrack.tracker
{
    [Serializable]
    class Test
    {
        public Test(string foo, string bar)
        {
            this.Foo = foo;
            this.Bar = bar;
        }

        public string Foo;
        public string Bar;
    }
}
