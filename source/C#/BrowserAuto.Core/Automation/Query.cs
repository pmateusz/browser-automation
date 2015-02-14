using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;

namespace BrowserAuto.Core.Automation
{
    public sealed class Query
    {
        private readonly List<Condition> conditions;

        private Query(Condition condition)
        {
            this.conditions = new List<Condition> { condition };
        }

        public Query ClassName(string name)
        {
            var condition = CreateClassNameCondition(name);
            return this.And(condition);
        }

        public Query Name(params string[] names)
        {
            var condition = CreateNameCondition(names);
            return this.And(condition);
        }

        public Query ControlType(ControlType type)
        {
            var condition = CreateControlTypeCondition(type);
            return this.And(condition);
        }

        internal Query And(Condition condition)
        {
            this.conditions.Add(condition);
            return this;
        }

        public static Query OfClassName(string name)
        {
            return CreateClassNameCondition(name);
        }

        public static Query OfName(params string[] names)
        {
            return CreateNameCondition(names);
        }

        public static Query OfControlType(ControlType type)
        {
            return CreateControlTypeCondition(type);
        }

        internal static PropertyCondition CreateControlTypeCondition(ControlType type)
        {
            return new PropertyCondition(AutomationElement.ControlTypeProperty, type);
        }

        internal static PropertyCondition CreateNameCondition(string name)
        {
            return new PropertyCondition(AutomationElement.NameProperty, name, PropertyConditionFlags.IgnoreCase);
        }

        internal static Condition CreateNameCondition(params string[] names)
        {
            if (names.Length == 1)
            {
                return CreateNameCondition(names[0]);
            }

            var nameConditions = names.Select(n => CreateNameCondition(n)).ToArray();
            return new OrCondition(nameConditions);
        }

        internal static PropertyCondition CreateClassNameCondition(string className)
        {
            return new PropertyCondition(AutomationElement.ClassNameProperty, className, PropertyConditionFlags.IgnoreCase);
        }

        public override string ToString()
        {
            var condition = (Condition)this;
            return condition.ToReadableString();
        }

        public static implicit operator Condition(Query query)
        {
            var conditions = query.conditions.ToArray();

            if (conditions.Length == 1)
            {
                return conditions[0];
            }

            return new AndCondition(conditions);
        }

        public static implicit operator Query(Condition condition)
        {
            return new Query(condition);
        }
    }
}
