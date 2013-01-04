using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralRegex
{
    public class Capture<T>
    {
        public string Name { internal set; get; }
        public IEnumerable<T> Value { internal set; get; }
        public int OriginalIndex { internal set; get; }

        internal Capture(IEnumerable<T> input, int start, int length)
        {
            Name = null;
            OriginalIndex = start;
            Value = input.Where((obj, index) => index >= start && index < start + length);
        }
    }
}
