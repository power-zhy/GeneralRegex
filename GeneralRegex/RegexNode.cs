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
using System.IO;
using System.Runtime.Serialization.Json;

namespace GeneralRegex
{
    enum RegexNodeType
    {
        Empty,          // 
        Head,           // ^
        Tail,           // $
        All,            // .
        Unit,           // /word
        Json,           // /{json}
        Repeat,         // ?, +, *, {m}, {m,}, {m,n}
        Expression,     // ...
        Alternation,    // ...|...
        Include,        // [...]
        Exclude,        // [^...]
        PrevPosChk,     // (?<=...)
        PrevNegChk,     // (?<!...)
        NextPosChk,     // (?>=...)
        NextNegChk,     // (?>!...)
        CreateGroup,    // (?'...'...)
        MatchGroup,     // (?=...)
        TestGroup,      // (??...)
    }

    class RegexNode<T>
    {
        internal RegexNodeType type = RegexNodeType.Expression;
        internal T objData = default(T);
        internal string strData = null;
        internal int repeatMin = 1;     // these two are used by Repeat node only
        internal int repeatMax = 1;   // -1 for infinite
        internal int countMin = 1;
        internal int countMax = 1;   // -1 for infinite
        internal bool greedy = true;
        internal bool noBack = false;
        internal string groupName = null;
        internal string closeGroup = null;
        internal RegexNode<T> parent = null;
        internal RegexNode<T> children = null;
        internal RegexNode<T> prev = null;
        internal RegexNode<T> next = null;
        // belows are used by Match operations only
        internal RegexNode<T> matchChild = null;
        internal int matchRepeat = 0;

        internal RegexNode(RegexNodeType type, string data)
        {
            this.type = type;
            strData = data;
            if (type == RegexNodeType.Json)
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                objData = (T)serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data.Substring(1))));
            }
        }

        override public string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(type.ToString()).Append(": \"").Append(strData).Append("\"    ");
            if (repeatMax == -1)
                builder.AppendFormat(">={0}", repeatMin);
            else// if (repeatMin != 1 || repeatMax != 1)
                builder.AppendFormat("{0}-{1}", repeatMin, repeatMax);
            if (countMax == -1)
                builder.AppendFormat(" >={0}", countMin);
            else// if (countMin != countMax)
                builder.AppendFormat(" {0}-{1}", countMin, countMax);
            if (!greedy)
                builder.Append("  NoGreedy");
            if (noBack)
                builder.Append("  NoBack");
            if (groupName != null)
                builder.Append("  Group: ").Append(groupName);
            return builder.ToString();
        }
    }
}
