#region Licence
// -----------------------------------------------------------------------
// <copyright file="LocalCollectionExpander.cs" company="James South">
//     Copyright (c) 2012,  James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace EFBootstrap.Caching
{
    #region Using
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using EFBootstrap.Extensions;
    #endregion

    /// <summary>
    /// Enables cache key support for local collection values.
    /// http://petemontgomery.wordpress.com
    /// http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ 
    /// </summary>
    public class LocalCollectionExpander : ExpressionVisitor
    {
        #region Methods
        /// <summary>
        /// Returns a re-written Expression. 
        /// </summary>
        /// <param name="expression">The expression to rewrite.</param>
        /// <returns>A re-written Expression. </returns>
        public static Expression Rewrite(Expression expression)
        {
            return new LocalCollectionExpander().Visit(expression);
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise,
        /// returns the original expression.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // Pair the method's parameter types with its arguments
            var map = node.Method.GetParameters()
                .Zip(node.Arguments, (p, a) => new { Param = p.ParameterType, Arg = a })
                .ToLinkedList();

            // deal with instance methods
            Type instanceType = node.Object == null ? null : node.Object.Type;
            map.AddFirst(new { Param = instanceType, Arg = node.Object });

            // for any local collection parameters in the method, make a
            // replacement argument which will print its elements
            var replacements = (from x in map
                                where x.Param != null && x.Param.IsGenericType
                                let g = x.Param.GetGenericTypeDefinition()
                                where g == typeof(IEnumerable<>) || g == typeof(List<>)
                                where x.Arg.NodeType == ExpressionType.Constant
                                let elementType = x.Param.GetGenericArguments().Single()
                                let printer = LocalCollectionExpander.MakePrinter((ConstantExpression)x.Arg, elementType)
                                select new { x.Arg, Replacement = printer }).ToList();

            if (replacements.Any())
            {
                var args = map.Select(x => replacements.Where(r => r.Arg == x.Arg).Select(r => r.Replacement).SingleOrDefault() ?? x.Arg).ToList();

                node = node.Update(args.First(), args.Skip(1));
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Linq.Expressions.ConstantExpression">ConstantExpression</see> that has the 
        /// ConstantExpression.Value property set to the specified value.
        /// </summary>
        /// <param name="enumerable">The ConstantExpression to manipulate.</param>
        /// <param name="elementType">The element type to create a ConstantExpression for.</param>
        /// <returns>A <see cref="T:System.Linq.Expressions.ConstantExpression">ConstantExpression</see> that has the 
        /// ConstantExpression.Value property set to the specified value.</returns>
        private static ConstantExpression MakePrinter(ConstantExpression enumerable, Type elementType)
        {
            IEnumerable value = (IEnumerable)enumerable.Value;
            Type printerType = typeof(Printer<>).MakeGenericType(elementType);
            object printer = Activator.CreateInstance(printerType, value);

            return Expression.Constant(printer);
        }
        #endregion

        /// <summary>
        /// Overrides ToString to print each element of a collection.
        /// </summary>
        /// <remarks>
        /// Inherits List in order to support List.Contains instance method as well
        /// as standard Enumerable.Contains/Any extension methods.
        /// </remarks>
        /// <typeparam name="T">The type of object to provide the methods for.</typeparam>
        private class Printer<T> : List<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:EFBootstrap.Caching.LocalCollectionExpander.Printer`1">Printer</see> class. 
            /// </summary>
            /// <param name="collection">The collection of objects to print.</param>
            public Printer(IEnumerable collection)
            {
                this.AddRange(collection.Cast<T>());
            }

            /// <summary>
            /// Returns a <see cref="T:System.String">String</see> that represents the current <see cref="T:System.Object">Object.</see>
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String">String</see> that represents the current <see cref="T:System.Object">Object.</see>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "{{{0}}}", this.ToConcatenatedString(t => t.ToString(), "|"));
            }
        }
    }
}
