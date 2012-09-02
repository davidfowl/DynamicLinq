namespace DynamicLINQ
{
    using System;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class DynamicExpressionBuilder : DynamicObject
    {
        private Expression _expression;
        public DynamicExpressionBuilder(Expression expression)
        {
            _expression = expression;
        }

        public Expression Expression
        {
            get
            {
                return _expression;
            }
        }

        public dynamic this[string name]
        {
            get
            {
                return new DynamicExpressionBuilder(Expression.Property(Expression, name));
            }
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // REVIEW: Is is possible to make this work?
            throw new InvalidOperationException("Dynamic expressions don't support casts");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicExpressionBuilder(Expression.Property(Expression, binder.Name));
            return true;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            result = MakeBinaryBuilder(Expression, binder.Operation, arg);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            // TODO: As a first attempt we can use the member names of the parameters to try and resolve the overload

            MethodInfo methodInfo = null;
            if (args.Any(a => a == null))
            {
                methodInfo = Expression.Type.GetMethod(binder.Name);
            }
            else
            {
                var types = args.Select(arg => arg.GetType()).ToArray();
                methodInfo = Expression.Type.GetMethod(binder.Name, types);
            }

            if (methodInfo != null)
            {
                var expression = Expression.Call(Expression, methodInfo, args.Select(Expression.Constant));
                result = new DynamicExpressionBuilder(expression);
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            if (binder.Operation == ExpressionType.IsFalse || binder.Operation == ExpressionType.IsTrue)
            {
                result = false;
                return true;
            }

            result = new DynamicExpressionBuilder(Expression.MakeUnary(binder.Operation, Expression, binder.ReturnType));
            return true;
        }

        public static DynamicExpressionBuilder operator ==(object arg, DynamicExpressionBuilder expr)
        {
            return MakeBinaryBuilder(expr.Expression, ExpressionType.Equal, arg);
        }

        public static DynamicExpressionBuilder operator !=(object arg, DynamicExpressionBuilder expr)
        {
            return MakeBinaryBuilder(expr.Expression, ExpressionType.NotEqual, arg);
        }

        public static DynamicExpressionBuilder operator >(object arg, DynamicExpressionBuilder expr)
        {
            return MakeBinaryBuilder(expr.Expression, ExpressionType.LessThan, arg);
        }

        public static DynamicExpressionBuilder operator <(object arg, DynamicExpressionBuilder expr)
        {
            return MakeBinaryBuilder(expr.Expression, ExpressionType.GreaterThan, arg);
        }

        public static DynamicExpressionBuilder operator >=(object arg, DynamicExpressionBuilder expr)
        {
            return MakeBinaryBuilder(expr.Expression, ExpressionType.LessThanOrEqual, arg);
        }

        public static DynamicExpressionBuilder operator <=(object arg, DynamicExpressionBuilder expr)
        {
            return MakeBinaryBuilder(expr.Expression, ExpressionType.GreaterThanOrEqual, arg);
        }

        private static DynamicExpressionBuilder MakeBinaryBuilder(Expression left, ExpressionType operation, object arg)
        {
            Expression right = null;
            if (arg is DynamicExpressionBuilder)
            {
                right = ((DynamicExpressionBuilder)arg).Expression;
            }
            else
            {
                right = Expression.Constant(arg);
            }

            var expression = MakeBinary(left, right, operation);
            return new DynamicExpressionBuilder(expression);
        }

        private static BinaryExpression MakeBinary(Expression left, Expression right, ExpressionType type)
        {
            ConvertIfNecessary(ref left, ref right);
            return Expression.MakeBinary(type, left, right);
        }

        private static void ConvertIfNecessary(ref Expression left, ref Expression right)
        {
            if (right.Type.IsAssignableFrom(left.Type) ||
                left.Type.IsAssignableFrom(right.Type))
            {
                return;
            }

            var leftConverter = TypeDescriptor.GetConverter(left.Type);
            if (leftConverter.CanConvertTo(right.Type))
            {
                left = Expression.Convert(left, right.Type);
            }
            else
            {
                var rightConverter = TypeDescriptor.GetConverter(right.Type);
                if (rightConverter.CanConvertTo(left.Type))
                {
                    right = Expression.Convert(right, left.Type);
                }
            }
        }

        public override string ToString()
        {
            return Expression.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}