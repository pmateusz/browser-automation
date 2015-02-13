using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace BrowserAuto.Install.Automation
{
    internal static class AutomationExtensions
    {
        private static readonly Dictionary<Type, Action<Condition, StringBuilder>> ConditionToStringStrategy;
        private static readonly Condition WindowCondition;

        static AutomationExtensions()
        {
            WindowCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);

            ConditionToStringStrategy = new Dictionary<Type, Action<Condition, StringBuilder>>
            {
                { typeof(AndCondition), (c,b) => b.AppendCondition((AndCondition) c) },
                { typeof(OrCondition),  (c,b) => b.AppendCondition((OrCondition) c) },
                { typeof(NotCondition), (c,b) => b.AppendCondition((NotCondition) c) },
                { typeof(PropertyCondition), (c,b) => b.AppendCondition((PropertyCondition) c) }
            };
        }

        public static Task<AutomationElement> FindApplicationWindow(Regex windowNamePattern, CancellationToken token)
        {
            Func<AutomationElement> stopFunction = () => ApplicationWindows.FirstOrDefault(w => windowNamePattern.IsMatch(w.GetName()));
            return Waiter.Start(stopFunction, token);
        }

        private static IEnumerable<AutomationElement> ApplicationWindows
        {
            get { return AutomationElement.RootElement.FindAll(TreeScope.Children, WindowCondition).OfType<AutomationElement>(); }
        }

        public static string GetName(this AutomationElement automationElement)
        {
            try
            {
                var rawValue = automationElement.GetCurrentPropertyValue(AutomationElement.NameProperty);
                
                if (rawValue != null)
                {
                    return rawValue.ToString();
                }
            }
            catch (ElementNotAvailableException)
            {
                // no-op
            }

            return string.Empty;
        }

        public static async Task<AutomationElement> FirstChild(this AutomationElement automationElement, Query query, CancellationToken token)
        {
            try
            {
                return await Waiter.Start(() => automationElement.FindFirst(TreeScope.Children, query), token);
            }
            catch (OperationCanceledException ex)
            {
                var msg = string.Format("AutomationElement ({0}) not found", query);
                throw new OperationCanceledException(msg, ex);
            }
        }

        public static void Click(this AutomationElement automationElement)
        {
            var invokePattern = automationElement.GetPattern<InvokePattern>(InvokePattern.Pattern);
            invokePattern.Invoke();
        }

        public static void Toggle(this AutomationElement automationElement)
        {
            var togglePattern = automationElement.GetPattern<TogglePattern>(TogglePattern.Pattern);
            togglePattern.Toggle();
        }

        public static void Select(this AutomationElement automationElement)
        {
            var selectionItemPattern = automationElement.GetPattern<SelectionItemPattern>(SelectionItemPattern.Pattern);
            selectionItemPattern.Select();
        }

        public static ToggleState GetState(this AutomationElement automationElement)
        {
            var togglePattern = automationElement.GetPattern<TogglePattern>(TogglePattern.Pattern);
            return togglePattern.Current.ToggleState;
        }

        public static T GetPattern<T>(this AutomationElement automationElement, AutomationPattern automationPattern) where T : BasePattern
        {
            try
            {
                return (T)automationElement.GetCurrentPattern(automationPattern);
            }
            catch (InvalidOperationException ex)
            {
                var msg = string.Format("AutomationElement does not support ({0})", automationPattern.ProgrammaticName);
                throw new InvalidOperationException(msg, ex);
            }
            catch (ElementNotAvailableException ex)
            {
                throw new InvalidOperationException("AutomationElement is not available", ex);
            }
        }

        public static string ToReadableString(this Condition condition)
        {
            var builder = new StringBuilder();
            builder.AppendCondition(condition);
            return builder.ToString();
        }

        private static void AppendCondition(this StringBuilder builder, Condition condition)
        {
            var conditionType = condition.GetType();

            if (!ConditionToStringStrategy.ContainsKey(conditionType))
            {
                var msg = string.Format("No AppendCondition method defined for ({0}) type", conditionType);
                throw new ArgumentException(msg);
            }

            var appendAction = ConditionToStringStrategy[conditionType];
            appendAction(condition, builder);
        }

        private static void AppendCondition(this StringBuilder builder, AndCondition condition)
        {
            builder.AppendConditons(" and ", condition.GetConditions());
        }

        private static void AppendCondition(this StringBuilder builder, OrCondition condition)
        {
            builder.AppendConditons(" or ", condition.GetConditions());
        }

        private static void AppendConditons(this StringBuilder builder, string separator, Condition[] conditions)
        {
            if (conditions.Length == 0)
            {
                return;
            }

            if (conditions.Length == 1)
            {
                builder.AppendCondition(conditions[0]);
                return;
            }

            builder.Append("( ");
            builder.AppendCondition(conditions[0]);
            foreach (var condition in conditions.Skip(1))
            {
                builder.Append(separator);
                builder.AppendCondition(condition);
            }
            builder.Append(" )");
        }

        private static void AppendCondition(this StringBuilder builder, NotCondition condition)
        {
            builder.Append("not ");
            builder.AppendCondition(condition.Condition);
        }

        private static void AppendCondition(this StringBuilder builder, PropertyCondition condition)
        {
            builder.Append(condition.Property.ProgrammaticName);
            builder.Append(" == ");
            builder.Append(condition.Value);
        }
    }
}
