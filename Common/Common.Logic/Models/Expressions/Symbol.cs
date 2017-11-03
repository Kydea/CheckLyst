using System;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.AggregationFunctions;
using CheckListManager.Common.Logic.Models.Expressions.ElementProperties;
using CheckListManager.Common.Logic.Models.Expressions.LogicOperators;
using Newtonsoft.Json;
using LinqExpression = System.Linq.Expressions.Expression;
// ReSharper disable TailRecursiveCall

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class Symbol
    {
        private Expression _expression;
        [JsonIgnore]
        public virtual Expression Expression
        {
            get => _expression;
	        set
            {
                _expression = value;
                _expression.Symbols.Add(this);
            }
        }

        public abstract TypedValue GetValue(IElementsBranch elementsBranch);

        public abstract void Check(Prototype prototype);

        public DataType DataType { get; protected set; }

        public static Symbol Parse(LinqExpression expression, ICheckList checkList)
        {
			// ReSharper disable once SwitchStatementMissingSomeCases
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    return Parse(((LambdaExpression)expression).Body, checkList);
				case ExpressionType.Convert:
					return Parse(((UnaryExpression)expression).Operand, checkList);
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return BinaryOperator.Parse((BinaryExpression)expression, checkList);
                case ExpressionType.Not:
                    return new Not { Argument = Parse(((UnaryExpression)expression).Operand, checkList) };
                case ExpressionType.Parameter:
                    var name = ((ParameterExpression)expression).Name;
                    return checkList.Expressions.ContainsKey(name)
                        ? new ExpressionValue(checkList.Expressions[name], checkList)
                        : (Symbol)new FieldProperty(name);
                case ExpressionType.Call:
                    return AggregateFunction.Parse((MethodCallExpression)expression, checkList);
                case ExpressionType.Constant:
                    var constant = (ConstantExpression)expression;
                    return constant.Type == typeof(TimeSpan)
						? new Constant(new TypedValue(DataType.Number, Convert.ToDecimal(((TimeSpan)constant.Value).TotalSeconds)))
						: new Constant(new TypedValue(constant.Type, constant.Value));
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    var parameterExpression = (ParameterExpression)memberExpression.Expression;
                    var type = parameterExpression.Type;

					if (memberExpression.Member.Name == ElementProperty.ElementProperties.Valid.ToString())
						return new ElementProperty(parameterExpression.Name, (ElementProperty.ElementProperties)Enum.Parse(typeof(ElementProperty.ElementProperties), memberExpression.Member.Name));
					if (typeof(Field).IsAssignableFrom(type))
                        return new FieldProperty(parameterExpression.Name);
                    else if (typeof(CheckLists.Signature).IsAssignableFrom(type))
                        return new SignatureProperty(parameterExpression.Name, (SignatureProperty.SignatureProperties)Enum.Parse(typeof(SignatureProperty.SignatureProperties), memberExpression.Member.Name));
						
					break;
                case ExpressionType.Conditional:
                    var conditionalExpression = (ConditionalExpression)expression;
                    return new If { Condition = Parse(conditionalExpression.Test, checkList), TruePart = Parse(conditionalExpression.IfTrue, checkList), FalsePart = Parse(conditionalExpression.IfFalse, checkList) };
            }

            throw new Exception($"Node Type {expression.NodeType.ToString()} not managed");
        }
    }
}
