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
    public class Match<T>
    {
        public bool Success { internal set; get; }
        public IEnumerable<T> Value { internal set; get; }
        public int Index { internal set; get; }
        public Dictionary<string, Group<T>> Groups { internal set; get; }

        internal Match()
        {
            Success = false;
            Value = null;
            Index = -1;
            Groups = new Dictionary<string, Group<T>>();
        }

        internal void SetValue(IEnumerable<T> input, int start, int length)
        {
            Success = true;
            Index = start;
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
