using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralRegex
{
    public class Group<T>
    {
        Stack<Capture<T>> stack;
        Stack<Capture<T>> rollback;

        public string Name { internal set; get; }

        public IEnumerable<T> Value
        {
            get
            {
                return stack.Peek().Value;
            }
        }

        public Capture<T>[] Captures
        {
            get
            {
                return stack.ToArray();
            }
        }

        internal Group(string name)
        {
            Name = name;
            stack = new Stack<Capture<T>>();
            rollback = new Stack<Capture<T>>();
        }

        internal void Push(Capture<T> capture)
        {
            capture.Name = Name;
            stack.Push(capture);
        }

        internal void RollbackPush()
        {
            if (stack.Count > 0)
                stack.Pop();
        }

        internal Capture<T> Pop()
        {
            Capture<T> capture = null;
            if (stack.Count > 0)
            {
                capture = stack.Pop();
                rollback.Push(capture);
            }
            return capture;
        }

        internal void RollbackPop()
        {
            if (rollback.Count > 0)
            {
                Capture<T> capture = rollback.Pop();
                stack.Push(capture);
            }
        }

        internal Capture<T> Peek()
        {
            if (stack.Count > 0)
                return stack.Peek();
            else
                return null;
        }
    }
}
