/* *
 * 
 * Copyright (c) 2012  by Hongyu Zhao <power.zju.zhy@gmail.com>
 * 
 * 
 * This file is part of GeneralRegex.
 * 
 * GeneralRegex is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * GeneralRegex is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with GeneralRegex.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * */


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
                if (stack.Count > 0)
                    return stack.Peek().Value;
                else
                    return null;
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
