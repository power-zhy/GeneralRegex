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
