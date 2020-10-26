using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_4
{
    class Problem
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public Dictionary<string, byte[]> Images { get; set; }
        public Test[] Tests { get; set; }

        public struct Test
        {
            public string input;
            public string output;
            public bool visible;
        }
    }
}
