using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace GeneralRegex
{
    enum RegexOption
    {
        None = 0,
        //MatchStart = 1,
        //MatchEnd = 2,
        Reverse = 4,
        Posix = 8,
        AllContainNull = 16,
    }

    class Regex<T>
    {
        string pattern = null;
        RegexNode<T> root = null;
        GroupInfoSet<T> groupInfoSet = null;

        public Regex(string pattern)
        {
            this.pattern = pattern;
            root = GenTree(pattern, 0);
            groupInfoSet = new GroupInfoSet<T>();
            CalcCount(root);
        }

        // return the end position + 1
        private int SearchPair(string text, int start, char left, char right, int basePos)
        {
            int curr = start + 1;
            int count = 1;
            while (curr < text.Length)
            {
                char ch = text[curr++];
                if (ch == left)
                    count++;
                else if (ch == right)
                    count--;
                if (count == 0)
                    return curr;
            }
            throw new FormatException(string.Format("Syntax error at index {0}: '{1}' and '{2}' pair mismatch", basePos + start, left, right));
        }

        // return the end position + 1
        private int SearchWord(string text, int start, int basePos)
        {
            int curr = start;
            while (curr < text.Length && (char.IsLetterOrDigit(text[curr]) || text[curr] == '_'))
                curr++;
            if (curr == start)
                throw new FormatException(string.Format("Syntax error at index {0}: Expect word.", basePos + start));
            return curr;
        }

        private bool IsWord(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            foreach (char ch in text)
            {
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                    return false;
            }
            return true;
        }

        /*private void SetNoBack(RegexNode<T> node, bool recur)
        {
            node.noBack = true;
            if (recur)
            {
                RegexNode<T> child = node.children;
                while (child != null)
                    SetNoBack(child, true);
            }
        }*/

        private RegexNode<T> GenTree(string pattern, int basePos)
        {
            if (pattern == null)
                throw new ArgumentNullException("Pattern can't be null", "pattern");

            int curr = 0;
            int end;
            while (curr < pattern.Length && char.IsWhiteSpace(pattern[curr]))
                curr++;
            if (curr >= pattern.Length)
                return new RegexNode<T>(RegexNodeType.Empty, "");
            RegexNode<T> root = new RegexNode<T>(RegexNodeType.Expression, pattern);
            RegexNode<T> child = null;
            if (pattern[curr] == '?')
            {
                RegexNode<T> node = null;
                string name = null;
                if (curr + 1 >= pattern.Length)
                    throw new FormatException(string.Format("Syntax error at index {0}: Nothing behind '?'", basePos + curr));
                switch (pattern[curr + 1])
                {
                    case '<':
                        if (curr + 2 >= pattern.Length || (pattern[curr + 2] != '=' && pattern[curr + 2] != '!'))
                            throw new FormatException(string.Format("Syntax error at index {0}: Expect '=' or '!'.", basePos + curr + 2));
                        root.type = pattern[curr + 2] == '=' ? RegexNodeType.PrevPosChk : RegexNodeType.PrevNegChk;
                        if (curr + 3 >= pattern.Length)
                            node = new RegexNode<T>(RegexNodeType.Empty, "");
                        else
                            node = GenTree(pattern.Substring(curr + 3), basePos + curr + 3);
                        node.parent = root;
                        root.children = node;
                        break;
                    case '>':
                        if (curr + 2 >= pattern.Length || (pattern[curr + 2] != '=' && pattern[curr + 2] != '!'))
                            throw new FormatException(string.Format("Syntax error at index {0}: Expect '=' or '!'.", basePos + curr + 2));
                        root.type = pattern[curr + 2] == '=' ? RegexNodeType.NextPosChk : RegexNodeType.NextNegChk;
                        if (curr + 3 >= pattern.Length)
                            node = new RegexNode<T>(RegexNodeType.Empty, "");
                        else
                            node = GenTree(pattern.Substring(curr + 3), basePos + curr + 3);
                        node.parent = root;
                        root.children = node;
                        break;
                    case '*':
                        if (curr + 2 >= pattern.Length)
                            node = new RegexNode<T>(RegexNodeType.Empty, "");
                        else
                            node = GenTree(pattern.Substring(curr + 2), basePos + curr + 2);
                        node.noBack = true;
                        root = node;
                        break;
                    case '\'':
                        end = -1;
                        if (curr + 2 < pattern.Length)
                            end = pattern.IndexOf('\'', curr + 2);
                        if (end < 0)
                            throw new FormatException(string.Format("Syntax error at index {0}: Can't find the ''' pair.", basePos + curr + 1));
                        name = pattern.Substring(curr + 2, end - curr - 2).Trim();
                        int dash = name.IndexOf('-');
                        string open = null, close = null;
                        if (dash < 0)
                            open = name;
                        else if (dash == 0)
                            close = name.Substring(1).Trim();
                        else
                        {
                            open = name.Substring(0, dash).Trim();
                            close = name.Substring(dash + 1).Trim();
                        }
                        if ((open != null && !IsWord(open)) || (close != null && !IsWord(close)))
                            throw new FormatException(string.Format("Syntax error at index {0}: Invalid group name '{1}'.", basePos + curr + 2, name));
                        root.type = RegexNodeType.CreateGroup;
                        root.groupName = open;
                        root.closeGroup = close;
                        node = GenTree(pattern.Substring(end + 1), basePos + end + 1);
                        node.parent = root;
                        root.children = node;
                        break;
                    case '=':
                        name = null;
                        if (curr + 2 < pattern.Length)
                            name = pattern.Substring(curr + 2).Trim();
                        if (name == null || !IsWord(name))
                            throw new FormatException(string.Format("Syntax error at index {0}: Invalid group name '{1}'.", basePos + curr + 2, name));
                        root.type = RegexNodeType.MatchGroup;
                        root.groupName = name;
                        break;
                    case '?':
                        name = null;
                        if (curr + 2 < pattern.Length)
                            name = pattern.Substring(curr + 2).Trim();
                        if (name == null || !IsWord(name))
                            throw new FormatException(string.Format("Syntax error at index {0}: Invalid group name '{1}'.", basePos + curr + 2, name));
                        root.type = RegexNodeType.TestGroup;
                        root.groupName = name;
                        break;
                    default:
                        throw new FormatException(string.Format("Syntax error at index {0}: Illegal text after '?'.", basePos + curr + 1));
                }
                return root;
            }

            while (curr < pattern.Length)
            {
                RegexNode<T> node = null;
                switch (pattern[curr])
                {
                    case '|':
                        node = GenTree(pattern.Substring(curr + 1), basePos + curr + 1);
                        root.strData = pattern.Substring(0, curr);
                        if (root.children == null)
                        {
                            child = new RegexNode<T>(RegexNodeType.Empty, "");
                        }
                        else if (root.children != child)
                        {
                            child = root;
                        }
                        if (node.type == RegexNodeType.Alternation)
                        {
                            child.parent = node;
                            child.next = node.children;
                            node.children.prev = child;
                            node.children = child;
                            node.strData = pattern;
                        }
                        else
                        {
                            RegexNode<T> newroot = new RegexNode<T>(RegexNodeType.Alternation, pattern);
                            child.parent = newroot;
                            newroot.children = child;
                            node.parent = newroot;
                            child.next = node;
                            node.prev = child;
                            node = newroot;
                        }
                        return node;
                    case '(':
                        end = SearchPair(pattern, curr, '(', ')', basePos);
                        node = GenTree(pattern.Substring(curr + 1, end - curr - 2), basePos + curr + 1);
                        curr = end;
                        break;
                    case '/':
                        if (pattern[curr + 1] == '{')
                        {
                            end = SearchPair(pattern, curr + 1, '{', '}', basePos);
                            try
                            {
                                node = new RegexNode<T>(RegexNodeType.Json, pattern.Substring(curr, end - curr));
                            }
                            catch (SerializationException)
                            {
                                throw new FormatException(string.Format("Syntax error at index {0}: Invalid JSON data.", basePos + curr));
                            }
                        }
                        else
                        {
                            end = SearchWord(pattern, curr + 1, basePos);
                            node = new RegexNode<T>(RegexNodeType.Unit, pattern.Substring(curr, end - curr));
                        }
                        curr = end;
                        break;
                    case '[':
                        end = SearchPair(pattern, curr, '[', ']', basePos);
                        bool exclude = false;
                        if (pattern[curr + 1] == '^')
                        {
                            exclude = true;
                            curr++;
                        }
                        node = GenTree(pattern.Substring(curr + 1, end - curr - 2), basePos + curr + 1);
                        if (node.type != RegexNodeType.Expression)
                            throw new FormatException(string.Format("Syntax error at index {0}: Invalid pattern in []", basePos + curr));
                        node.type = exclude ? RegexNodeType.Exclude : RegexNodeType.Include;
                        curr = end;
                        break;
                    case '.':
                        node = new RegexNode<T>(RegexNodeType.All, ".");
                        curr++;
                        break;
                    case '^':
                        node = new RegexNode<T>(RegexNodeType.Head, "^");
                        curr++;
                        break;
                    case '$':
                        node = new RegexNode<T>(RegexNodeType.Tail, "$");
                        curr++;
                        break;
                    case '?':
                        if (child == null || (child.type != RegexNodeType.All && child.type != RegexNodeType.Expression && child.type != RegexNodeType.Unit && child.type != RegexNodeType.Json && child.type != RegexNodeType.Alternation
                            && child.type != RegexNodeType.Include && child.type != RegexNodeType.Exclude && child.type != RegexNodeType.CreateGroup && child.type != RegexNodeType.MatchGroup))
                            throw new FormatException(string.Format("Syntax error at index {0}: Quantifier '?' must be following an expression.", basePos + curr));
                        node = new RegexNode<T>(RegexNodeType.Repeat, child.strData);   // FIXME this strData is not very accurate, occurred 4 times
                        node.repeatMin = 0;
                        node.repeatMax = 1;
                        child.parent = node;
                        node.children = child;
                        child = child.prev;
                        curr++;
                        if (curr < pattern.Length && pattern[curr] == '?')
                        {
                            node.greedy = false;
                            curr++;
                        }
                        break;
                    case '*':
                        if (child == null || (child.type != RegexNodeType.All && child.type != RegexNodeType.Expression && child.type != RegexNodeType.Unit && child.type != RegexNodeType.Json && child.type != RegexNodeType.Alternation
                            && child.type != RegexNodeType.Include && child.type != RegexNodeType.Exclude && child.type != RegexNodeType.CreateGroup && child.type != RegexNodeType.MatchGroup))
                            throw new FormatException(string.Format("Syntax error at index {0}: Quantifier '?' must be following an expression.", basePos + curr));
                        node = new RegexNode<T>(RegexNodeType.Repeat, child.strData);
                        node.repeatMin = 0;
                        node.repeatMax = -1;
                        child.parent = node;
                        node.children = child;
                        child = child.prev;
                        curr++;
                        if (curr < pattern.Length && pattern[curr] == '?')
                        {
                            node.greedy = false;
                            curr++;
                        }
                        break;
                    case '+':
                        if (child == null || (child.type != RegexNodeType.All && child.type != RegexNodeType.Expression && child.type != RegexNodeType.Unit && child.type != RegexNodeType.Json && child.type != RegexNodeType.Alternation
                            && child.type != RegexNodeType.Include && child.type != RegexNodeType.Exclude && child.type != RegexNodeType.CreateGroup && child.type != RegexNodeType.MatchGroup))
                            throw new FormatException(string.Format("Syntax error at index {0}: Quantifier '?' must be following an expression.", basePos + curr));
                        node = new RegexNode<T>(RegexNodeType.Repeat, child.strData);
                        node.repeatMin = 1;
                        node.repeatMax = -1;
                        child.parent = node;
                        node.children = child;
                        child = child.prev;
                        curr++;
                        if (curr < pattern.Length && pattern[curr] == '?')
                        {
                            node.greedy = false;
                            curr++;
                        }
                        break;
                    case '{':
                        if (child == null || (child.type != RegexNodeType.All && child.type != RegexNodeType.Expression && child.type != RegexNodeType.Unit && child.type != RegexNodeType.Json && child.type != RegexNodeType.Alternation
                            && child.type != RegexNodeType.Include && child.type != RegexNodeType.Exclude && child.type != RegexNodeType.CreateGroup && child.type != RegexNodeType.MatchGroup))
                            throw new FormatException(string.Format("Syntax error at index {0}: Quantifier '?' must be following an expression.", basePos + curr));
                        node = new RegexNode<T>(RegexNodeType.Repeat, child.strData);
                        end = SearchPair(pattern, curr, '{', '}', basePos);
                        string tmp = pattern.Substring(curr + 1, end - curr - 2).Trim();
                        int dot = tmp.IndexOf(',');
                        try
                        {
                            if (dot < 0)
                            {
                                node.repeatMin = node.repeatMax = int.Parse(tmp);
                            }
                            else
                            {
                                node.repeatMin = int.Parse(tmp.Substring(0, dot).Trim());
                                tmp = tmp.Substring(dot + 1).Trim();
                                if (string.IsNullOrEmpty(tmp))
                                    node.repeatMax = -1;
                                else
                                    node.repeatMax = int.Parse(tmp);
                                if (node.repeatMin >= node.repeatMax || node.repeatMin < 0)
                                    throw new FormatException(string.Format("Syntax error at index {0}: Quantifier '{m,n}' is illegal.", basePos + curr));
                            }
                        }
                        catch (FormatException)
                        {
                            throw new FormatException(string.Format("Syntax error at index {0}: Quantifier '{m,n}' is not numbers.", basePos + curr));
                        }
                        catch (OverflowException)
                        {
                            throw new FormatException(string.Format("Syntax error at index {0}: Quantifier '{m,n}' is out of range.", basePos + curr));
                        }
                        child.parent = node;
                        node.children = child;
                        child = child.prev;
                        curr = end;
                        if (curr < pattern.Length && pattern[curr] == '?')
                        {
                            node.greedy = false;
                            curr++;
                        }
                        break;
                    default:
                        if (!char.IsWhiteSpace(pattern[curr]))
                            throw new FormatException(string.Format("Syntax error at index {0}: Expect expression.", basePos + curr));
                        curr++;
                        break;
                }
                if (node != null)
                {
                    node.parent = root;
                    if (child == null)
                    {
                        root.children = node;
                        child = node;
                    }
                    else
                    {
                        node.prev = child;
                        child.next = node;
                        child = node;
                    }
                }
            }
            if (child != null && root.children == child)
            {
                child.parent = null;
                return child;
            }
            else
                return root;
        }

        private void CalcCount(RegexNode<T> node)
        {
            if (node == null)
                return;
            RegexNode<T> child = node.children;
            int min, max;
            switch (node.type)
            {
                case RegexNodeType.Head:
                    if (node.prev != null)
                        throw new FormatException("Syntax Error: Head flag '^' must be at the head of an expression.");
                    node.countMin = node.countMax = 0;
                    break;
                case RegexNodeType.Tail:
                    if (node.next != null)
                        throw new FormatException("Syntax Error: Tail flag '$' must be at the tail of an expression.");
                    node.countMin = node.countMax = 0;
                    break;
                case RegexNodeType.Empty:
                case RegexNodeType.PrevPosChk:
                case RegexNodeType.PrevNegChk:
                case RegexNodeType.NextPosChk:
                case RegexNodeType.NextNegChk:
                case RegexNodeType.TestGroup:
                    node.countMin = node.countMax = 0;
                    break;
                case RegexNodeType.All:
                case RegexNodeType.Unit:
                case RegexNodeType.Json:
                    node.countMin = node.countMax = 1;
                    break;
                case RegexNodeType.Repeat:
                    Debug.Assert(child != null);
                    CalcCount(child);
                    node.countMin = child.countMin * node.repeatMin;
                    node.countMax = (node.repeatMax == -1 || child.countMax == -1) ? -1 : child.countMax * node.repeatMax;
                    break;
                case RegexNodeType.Expression:
                    min = max = 0;
                    while (child != null)
                    {
                        CalcCount(child);
                        min += child.countMin;
                        if (max >= 0)
                        {
                            if (child.countMax < -1)
                                max = -1;
                            else
                                max += child.countMax;
                        }
                        child = child.next;
                    }
                    node.countMin = min;
                    node.countMax = max;
                    break;
                case RegexNodeType.Alternation:
                    CalcCount(child);
                    min = child.countMin;
                    max = child.countMax;
                    child = child.next;
                    while (child != null)
                    {
                        CalcCount(child);
                        if (min > child.countMin)
                            min = child.countMin;
                        if (max >= 0 && (child.countMax == -1 || max < child.countMax))
                            max = child.countMax;
                        child = child.next;
                    }
                    node.countMin = min;
                    node.countMax = max;
                    break;
                case RegexNodeType.Include:
                case RegexNodeType.Exclude:
                    while (child != null)
                    {
                        CalcCount(child);
                        //if (child.countMin != 1 || child.countMax != 1)
                        if (child.type != RegexNodeType.Unit && child.type != RegexNodeType.Json)
                            throw new FormatException(string.Format("Syntax Error: Only single objects are allowed between '[' and ']', now '{0}' found.", child.strData));
                        child = child.next;
                    }
                    node.countMin = node.countMax = 1;
                    break;
                case RegexNodeType.CreateGroup:
                    Debug.Assert(child != null);
                    CalcCount(child);
                    node.countMin = child.countMin;
                    node.countMax = child.countMax;
                    groupInfoSet.addGroupInfo(node.groupName, node.countMin, node.countMax);
                    break;
                case RegexNodeType.MatchGroup:
                    if (!groupInfoSet.getGroupInfo(node.groupName, out node.countMin, out node.countMax))
                        throw new FormatException(string.Format("Syntax Error: Group '{0}' must be created before back referencing.", node.groupName));
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            if (node.countMax == 0 && node.repeatMax < 0)
                throw new FormatException("Syntax Error: Zero-width assertion can't be followed by a infinite quantifier.");
            return;
        }


        T[] input;
        Func<string, T, bool> comparer;
        Match<T> match;
        Stack<RollbackNode<T>> status;

        // currPos is the count from starting point(input.Length-1 when Reverse option is set), not the index of input data
        private T GetObj(int currPos, RegexOption option, int offset = 0)
        {
            if ((option & RegexOption.Reverse) != 0)
                currPos = input.Length - currPos - offset - 1;
            else
                currPos = currPos + offset;
            return input[currPos];
        }

        private void PushStatus(RollbackNode<T> rollback)
        {
            status.Push(rollback);
            Debug.WriteLine("Push Status: " + rollback);
        }

        private RollbackNode<T> PopStatus(RegexNode<T> node)
        {
            while (true)
            {
                RollbackNode<T> rollback = status.Pop();
                Debug.WriteLine("Pop Status: " + rollback);
                Debug.Assert(rollback != null);
                if (rollback.type == RollbackNodeType.RecurMark)
                {
                    if (rollback.regexNode == node)
                        return null;
                    else
                        continue;
                }
                else if (rollback.type == RollbackNodeType.CreateGroup)
                {
                    string open = rollback.regexNode.groupName;
                    string close = rollback.regexNode.closeGroup;
                    if (!string.IsNullOrEmpty(open))
                        match.RollbackPushGroup(open);
                    if (!string.IsNullOrEmpty(close))
                        match.RollbackPopGroup(close);
                }
                else
                    return rollback;
            }
        }

        // match a node without caring about quantifiers
        // return how much objects matched by node, -1 for errors
        // position is the count from starting point(input.Length-1 when Reverse option is set), not the index of input data
        // if an invoke to this returns -1, than all stacks must be balanced
        private int MatchNode(RegexNode<T> node, int position, RollbackNode<T> rollback, RegexOption option)
        {
            if (rollback != null && rollback.regexNode == node)
            {
                node.matchChild = rollback.matchChild;
                node.matchRepeat = rollback.matchRepeat;
                position = rollback.position;
            }
            else if (input.Length - position < node.countMin)
                return -1;
            RegexNode<T> child = node.children;
            int curr = position;
            int matchLen = 0;   // total matched input length
            int step = -1;  // single matched input length, -1 for match failed
            switch (node.type)
            {
                case RegexNodeType.Empty:
                    return 0;
                case RegexNodeType.Head:
                    if ((option & RegexOption.Reverse) != 0)
                        return curr == input.Length ? 0 : -1;
                    else
                        return curr == 0 ? 0 : -1;
                case RegexNodeType.Tail:
                    if ((option & RegexOption.Reverse) != 0)
                        return curr == 0 ? 0 : -1;
                    else
                        return curr == input.Length ? 0 : -1;
                case RegexNodeType.All:
                    if ((option & RegexOption.AllContainNull) != 0)
                        return 1;
                    else
                        return GetObj(curr, option) == null ? -1 : 1;
                case RegexNodeType.Unit:
                    return comparer(node.strData, GetObj(curr, option)) ? 1 : -1;
                case RegexNodeType.Json:
                    T obj = GetObj(curr, option);
                    if (node.objData == null)
                        return obj == null ? 1 : -1;
                    else
                        return node.objData.Equals(obj) ? 1 : -1;
                case RegexNodeType.Repeat:
                    if (rollback != null)
                    {
                        if (rollback.regexNode != node)
                        {
                            RegexNode<T> target = rollback.regexNode;
                            while (target.parent != node)
                                target = target.parent;
                            Debug.Assert(target == child);
                            return MatchNode(child, rollback.position, rollback, option);
                        }
                        else
                        {
                            Debug.Assert(node.matchRepeat >= node.repeatMin);
                            Debug.Assert(node.repeatMax < 0 || node.matchRepeat < node.repeatMax);
                            if (node.greedy)
                            {
                                return 0;
                            }
                            else
                            {
                                step = MatchNode(child, rollback.position, null, option);
                                if (step > 0)   // will not match more when step == 0
                                {
                                    node.matchRepeat++;
                                    if (node.repeatMax < 0 || node.matchRepeat < node.repeatMax)
                                        PushStatus(new RollbackNode<T>(RollbackNodeType.NoGreedyMatch, node, curr));
                                }
                                return step;
                            }
                        }
                    }
                    while (true)
                    {
                        if (node.repeatMin == node.repeatMax)
                        {
                            step = MatchNode(child, curr, null, option);
                            if (step >= 0)
                            {
                                matchLen += step;
                                curr += step;
                                node.matchRepeat++;
                                if (node.matchRepeat >= node.repeatMin)
                                    return matchLen;
                            }
                            else
                                return -1;
                        }
                        else
                        {
                            if (node.matchRepeat < node.repeatMin)
                            {
                                step = MatchNode(child, curr, null, option);
                                if (step >= 0)
                                {
                                    matchLen += step;
                                    curr += step;
                                    node.matchRepeat++;
                                }
                                else
                                    return -1;
                            }
                            else
                            {
                                Debug.Assert(node.repeatMax < 0 || node.matchRepeat < node.repeatMax);
                                if (node.greedy)
                                {
                                    if (!node.noBack)
                                        PushStatus(new RollbackNode<T>(RollbackNodeType.GreedyMatch, node, curr));
                                    step = MatchNode(child, curr, null, option);
                                    if (step > 0)
                                    {
                                        matchLen += step;
                                        curr += step;
                                        node.matchRepeat++;
                                        if (node.repeatMax >= 0 && node.matchRepeat >= node.repeatMax)
                                            return matchLen;
                                    }
                                    else
                                    {
                                        if (!node.noBack)
                                            PopStatus(node);
                                        return matchLen;
                                    }
                                }
                                else
                                {
                                    PushStatus(new RollbackNode<T>(RollbackNodeType.NoGreedyMatch, node, curr));
                                    return matchLen;
                                }
                            }
                        }
                    }
                case RegexNodeType.Expression:
                    PushStatus(new RollbackNode<T>(RollbackNodeType.RecurMark, node, 0));
                    if (rollback != null)
                    {
                        RegexNode<T> target = rollback.regexNode;
                        while (target.parent != node)
                            target = target.parent;
                        curr = rollback.position;
                        child = target;
                    }
                    while (child != null)
                    {
                        step = MatchNode(child, curr, rollback, option);
                        if (step >= 0)
                        {
                            rollback = null;
                            matchLen += step;
                            curr += step;
                            child = child.next;
                        }
                        else
                        {
                            rollback = PopStatus(node);
                            if (rollback == null)
                                return -1;
                            RegexNode<T> target = rollback.regexNode;
                            while (target.parent != node)
                                target = target.parent;
                            curr = rollback.position;
                            matchLen = curr - position;
                            child = target;
                        }
                    }
                    return matchLen;
                case RegexNodeType.Alternation:
                    if (rollback != null)
                    {
                        if (rollback.regexNode == node)
                        {
                            child = rollback.matchChild.next;
                            rollback = null;
                        }
                        else
                        {
                            RegexNode<T> target = rollback.regexNode;
                            while (target.parent != node)
                                target = target.parent;
                            child = target;
                        }
                    }
                    while (child != null)
                    {
                        node.matchChild = child;
                        PushStatus(new RollbackNode<T>(RollbackNodeType.Alternation, node, curr));
                        step = MatchNode(child, curr, rollback, option);
                        if (step >= 0)
                            return step;
                        PopStatus(node);
                        child = child.next;
                    }
                    return -1;
                case RegexNodeType.Include:
                    while (child != null)
                    {
                        if (MatchNode(child, curr, null, option) > 0)
                            return 1;
                        child = child.next;
                    }
                    return -1;
                case RegexNodeType.Exclude:
                    while (child != null)
                    {
                        if (MatchNode(child, curr, null, option) > 0)
                            return -1;
                        child = child.next;
                    }
                    return 1;
                case RegexNodeType.PrevPosChk:
                    if (rollback != null)
                        return -1;
                    if ((option & RegexOption.Reverse) != 0)
                        step = MatchNode(child, curr, null, option);
                    else
                        step = MatchNode(child, input.Length - curr, null, option | RegexOption.Reverse);
                    return step >= 0 ? 0 : -1;
                case RegexNodeType.PrevNegChk:
                    if (rollback != null)
                        return -1;
                    if ((option & RegexOption.Reverse) != 0)
                        step = MatchNode(child, curr, null, option);
                    else
                        step = MatchNode(child, input.Length - curr, null, option | RegexOption.Reverse);
                    return step >= 0 ? -1 : 0;
                case RegexNodeType.NextPosChk:
                    if (rollback != null)
                        return -1;
                    if ((option & RegexOption.Reverse) != 0)
                        step = MatchNode(child, input.Length - curr, null, (RegexOption)(option - RegexOption.Reverse));
                    else
                        step = MatchNode(child, curr, null, option);
                    return step >= 0 ? 0 : -1;
                case RegexNodeType.NextNegChk:
                    if (rollback != null)
                        return -1;
                    if ((option & RegexOption.Reverse) != 0)
                        step = MatchNode(child, input.Length - curr, null, (RegexOption)(option - RegexOption.Reverse));
                    else
                        step = MatchNode(child, curr, null, option);
                    return step >= 0 ? -1 : 0;
                case RegexNodeType.CreateGroup:
                    step = MatchNode(node, curr, rollback, option);
                    if (step >= 0)
                    {
                        int start = (option & RegexOption.Reverse) != 0 ? input.Length - curr - step : curr;
                        Capture<T> capture = new Capture<T>(input, start, step);
                        PushStatus(new RollbackNode<T>(RollbackNodeType.CreateGroup, node, curr));
                        if (!string.IsNullOrEmpty(node.groupName))
                            match.PushGroup(node.groupName, capture);
                        if (!string.IsNullOrEmpty(node.closeGroup))
                            match.PopGroup(node.closeGroup);
                    }
                    return step;
                case RegexNodeType.MatchGroup:
                    Debug.Assert(rollback == null);
                    T[] mat = match.PeekGroup(node.groupName).Value.ToArray();
                    if ((option & RegexOption.Reverse) != 0)
                        curr = input.Length - curr - mat.Length;
                    if (curr < 0 || curr + mat.Length >= input.Length)
                        return -1;
                    for (int i = 0; i < mat.Length; i++)
                    {
                        if (input[curr + i] == null)
                        {
                            if (mat[i] != null)
                                return -1;
                        }
                        else
                        {
                            if (!input[curr + i].Equals(mat[i]))
                                return -1;
                        }
                    }
                    return mat.Length;
                case RegexNodeType.TestGroup:
                    Debug.Assert(rollback == null);
                    return match.PeekGroup(node.groupName) != null ? 0 : -1;
                default:
                    Debug.Assert(false);
                    return -1;
            }
        }

        public Match<T> Match(IEnumerable<T> input, Func<string, T, bool> comparer, RegexOption option)
        {
            this.input = input.ToArray();
            this.comparer = comparer;
            match = new Match<T>();
            status = new Stack<RollbackNode<T>>();
            int position = 0;
            while (position <= this.input.Length && !match.Success)
            {
                int length = MatchNode(root, position, null, option);
                if (length >= 0)
                {
                    int start = (option & RegexOption.Reverse) != 0 ? this.input.Length - position - length : position;
                    match.SetValue(input, start, length);
                }
                else
                    position++;
            }
            return match;
        }

        public Match<T>[] Matches(IEnumerable<T> input, Func<string, T, bool> comparer, RegexOption option)
        {
            this.input = input.ToArray();
            this.comparer = comparer;
            List<Match<T>> list = new List<Match<T>>();
            int position = 0;
            while (true)
            {
                match = new Match<T>();
                status = new Stack<RollbackNode<T>>();
                int length = 0;
                while (position <= this.input.Length && !match.Success)
                {
                    length = MatchNode(root, position, null, option);
                    if (length >= 0)
                    {
                        int start = (option & RegexOption.Reverse) != 0 ? this.input.Length - position - length : position;
                        match.SetValue(input, start, length);
                    }
                    else
                        position++;
                }
                if (match.Success)
                {
                    list.Add(match);
                    position += (length <= 0 ? 1 : length);
                }
                else
                    break;
            }
            return list.ToArray();
        }


        public string PrintPattern()
        {
            return string.Format("Original Pattern: {0}\nRegenerated Pattern: {1}\n\n{2}", pattern, PrintPattern(root), PrintTree(root, 0));
        }

        private string PrintPattern(RegexNode<T> node)
        {
            if (node == null)
                return null;
            string result = "";
            if (node.type != RegexNodeType.Alternation && node.children != null)
            {
                RegexNode<T> child = node.children;
                StringBuilder builder = new StringBuilder();
                while (child != null)
                {
                    builder.Append(PrintPattern(child));
                    child = child.next;
                }
                result = builder.ToString();
            }
            switch (node.type)
            {
                case RegexNodeType.Empty:
                    return "";
                case RegexNodeType.Head:
                    return "^";
                case RegexNodeType.Tail:
                    return "$";
                case RegexNodeType.All:
                    result = ".";
                    break;
                case RegexNodeType.Unit:
                    result = node.strData;
                    break;
                case RegexNodeType.Json:
                    //result = node.strData;
                    result = "/{" + node.objData.ToString() + "}";
                    break;
                case RegexNodeType.Repeat:
                    string quant;
                    if (node.repeatMin == 0 && node.repeatMax == 1)
                        quant = "?";
                    else if (node.repeatMin == 0 && node.repeatMax == -1)
                        quant = "*";
                    else if (node.repeatMin == 1 && node.repeatMax == -1)
                        quant = "+";
                    else if (node.repeatMin == node.repeatMax)
                        quant = "{" + node.repeatMin + "}";
                    else if (node.repeatMax == -1)
                        quant = "{" + node.repeatMin + ",}";
                    else
                        quant = "{" + node.repeatMin + "," + node.repeatMax + "}";
                    if (!node.greedy)
                        quant += "?";
                    if (node.type == RegexNodeType.Expression)
                        result = "(" + result + ")";
                    result += quant;
                    break;
                case RegexNodeType.Expression:
                    break;
                case RegexNodeType.Alternation:
                    RegexNode<T> child = node.children;
                    StringBuilder builder = new StringBuilder();
                    while (child != null)
                    {
                        builder.Append(PrintPattern(child));
                        child = child.next;
                        if (child != null)
                            builder.Append("|");
                    }
                    result = "(" + builder.ToString() + ")";
                    break;
                case RegexNodeType.Include:
                    result = "[" + result + "]";
                    break;
                case RegexNodeType.Exclude:
                    result = "[^" + result + "]";
                    break;
                case RegexNodeType.PrevPosChk:
                    result = "(?<=" + result + ")";
                    break;
                case RegexNodeType.PrevNegChk:
                    result = "(?<!" + result + ")";
                    break;
                case RegexNodeType.NextPosChk:
                    result = "(?>=" + result + ")";
                    break;
                case RegexNodeType.NextNegChk:
                    result = "(?>!" + result + ")";
                    break;
                case RegexNodeType.CreateGroup:
                    result = "(?'" + node.groupName + "'" + result + ")";
                    break;
                case RegexNodeType.MatchGroup:
                    result = "(?=" + node.groupName + ")";
                    break;
                case RegexNodeType.TestGroup:
                    result = "(??" + node.groupName + ")";
                    break;
                default:
                    return "!ERROR!";
            }
            return result;
        }

        private string PrintTree(RegexNode<T> node, int depth)
        {
            if (node == null)
                return null;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < depth; i++)
                builder.Append("  ");
            builder.AppendLine(node.ToString());
            RegexNode<T> child = node.children;
            while (child != null)
            {
                builder.Append(PrintTree(child, depth + 1));
                child = child.next;
            }
            return builder.ToString();
        }
    }
}
