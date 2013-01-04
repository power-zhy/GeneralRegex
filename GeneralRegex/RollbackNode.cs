using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralRegex
{
    enum RollbackNodeType
    {
        RecurMark,
        CreateGroup,
        GreedyMatch,
        NoGreedyMatch,
        Alternation,
    }

    class RollbackNode<T>
    {
        internal RollbackNodeType type;
        internal RegexNode<T> regexNode;
        internal RegexNode<T> matchChild = null;
        internal int matchRepeat = 0;
        internal int position;
        //internal bool selectable;

        /*internal RollbackNode(RegexNode<T> regexNode, int position, bool selectable)
        {
            this.regexNode = regexNode;
            this.position = position;
            this.selectable = selectable;
            if (regexNode != null)
            {
                matchChild = regexNode.matchChild;
                matchRepeat = regexNode.matchRepeat;
            }
        }*/

        internal RollbackNode(RollbackNodeType type, RegexNode<T> regexNode, int originalIndex)
        {
            this.type = type;
            this.regexNode = regexNode;
            this.position = originalIndex;
            if (regexNode != null)
            {
                matchChild = regexNode.matchChild;
                matchRepeat = regexNode.matchRepeat;
            }
        }

        override public string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}: [{1}-{2}]  ", type, regexNode.type, regexNode.strData);
            switch (type)
            {
                case RollbackNodeType.RecurMark:
                    break;
                case RollbackNodeType.GreedyMatch:
                case RollbackNodeType.NoGreedyMatch:
                    builder.AppendFormat("Repeat at {0}.", matchRepeat);
                    break;
                case RollbackNodeType.Alternation:
                    builder.AppendFormat("Child at {0}.", matchChild.strData);
                    break;
                case RollbackNodeType.CreateGroup:
                    string str = regexNode.groupName != null ? regexNode.groupName : "";
                    str = regexNode.closeGroup != null ? str + "-" + regexNode.closeGroup : str;
                    builder.AppendFormat("Group name is {0}.", str);
                    break;
                default:
                    builder.Append("!ERROR!");
                    break;
            }
            return builder.ToString();
        }
    }
}
