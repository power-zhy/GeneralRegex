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
    class GroupInfo<T>
    {
        internal string name;
        internal int lengthMin;
        internal int lengthMax;

        internal GroupInfo(string name)
        {
            this.name = name;
            lengthMin = -1;
            lengthMax = -1;
        }
    }

    class GroupInfoSet<T>
    {
        Dictionary<string, GroupInfo<T>> groups;

        internal GroupInfoSet()
        {
            groups = new Dictionary<string, GroupInfo<T>>();
        }

        internal void addGroupInfo(string name, int min, int max)
        {
            GroupInfo<T> groupInfo = null;
            if (groups.TryGetValue(name, out groupInfo))
            {
                if (groupInfo.lengthMin > min)
                    groupInfo.lengthMin = min;
                if (groupInfo.lengthMax >= 0 && (max == -1 || groupInfo.lengthMax < max))
                    groupInfo.lengthMax = max;
            }
            else
            {
                groupInfo = new GroupInfo<T>(name);
                groupInfo.lengthMin = min;
                groupInfo.lengthMax = max;
                groups.Add(name, groupInfo);
            }
        }

        internal bool getGroupInfo(string name, out int min, out int max)
        {
            GroupInfo<T> groupInfo = null;
            if (groups.TryGetValue(name, out groupInfo))
            {
                min = groupInfo.lengthMin;
                max = groupInfo.lengthMax;
                return true;
            }
            min = max = -1;
            return false;
        }
    }
}
