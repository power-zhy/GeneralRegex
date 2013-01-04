using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralRegex
{
    public class Match<T>
    {
        public bool Success { internal set; get; }
        public IEnumerable<T> Value { internal set; get; }
        public int OriginalIndex { internal set; get; }
        public Dictionary<string, Group<T>> Groups { internal set; get; }

        internal Match()
        {
            Success = false;
            Value = null;
            OriginalIndex = -1;
            Groups = new Dictionary<string, Group<T>>();
        }

        internal void SetValue(IEnumerable<T> input, int start, int length)
        {
            Success = true;
            OriginalIndex = start;
            Value = input.Where((obj, index) => index >= start && index < start + length);
        }

        internal void PushGroup(string name, Capture<T> capture)
        {
            Group<T> group = null;
            if (Groups.TryGetValue(name, out group))
            {
                group.Push(capture);
            }
            else
            {
                group = new Group<T>(name);
                group.Push(capture);
                Groups.Add(name, group);
            }
        }

        internal void RollbackPushGroup(string name)
        {
            Group<T> group = null;
            if (Groups.TryGetValue(name, out group))
            {
                group.RollbackPush();
            }
        }

        internal void RollbackPopGroup(string name)
        {
            Group<T> group = null;
            if (Groups.TryGetValue(name, out group))
            {
                group.RollbackPop();
            }
        }

        internal Capture<T> PopGroup(string name)
        {
            Group<T> group = null;
            if (Groups.TryGetValue(name, out group))
                return group.Pop();
            else
                return null;
        }

        internal Capture<T> PeekGroup(string name)
        {
            Group<T> group = null;
            if (Groups.TryGetValue(name, out group))
                return group.Peek();
            else
                return null;
        }
    }
}
