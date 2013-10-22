// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Evaluator.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Enables the partial evaluation of queries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Caching
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    #endregion

    /// <summary>
    /// Enables the partial evaluation of queries.
    /// </summary>
    /// <remarks>
    /// <see cref="http://msdn.microsoft.com/en-us/library/bb546158.aspx"/> 
    /// <see cref="http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ "/> 
    /// Copyright notice  <see cref="http://msdn.microsoft.com/en-gb/cc300389.aspx#O"/> 
    /// </remarks>
    public static class Evaluator
    {
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="functionCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> functionCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(functionCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, CanBeEvaluatedLocally);
        }

        /// <summary>
        /// Returns a value indicating whether the expression can be evaluated locally.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>A value indicating whether the expression can be evaluated locally.</returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        private class SubtreeEvaluator : ExpressionVisitor
        {
            #region Fields
            /// <summary>
            /// The candidate expressions.
            /// </summary>
            private readonly HashSet<Expression> candidates;
            #endregion

            #region Constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="T:EFBootstrap.Caching.Evaluator.SubtreeEvaluator"/> class. 
            /// </summary>
            /// <param name="candidates">
            /// The candidates for evaluation.
            /// </param>
            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Dispatches the expression to one of the more specialized visit methods in this
            /// class.
            /// </summary>
            /// <param name="node">The expression to visit.</param>
            /// <returns>
            /// The modified expression, if it or any sub-expression was modified; otherwise,
            /// returns the original expression.
            /// </returns>
            public override Expression Visit(Expression node)
            {
                if (node == null)
                {
                    return null;
                }

                if (this.candidates.Contains(node))
                {
                    return Evaluate(node);
                }

                return base.Visit(node);
            }

            /// <summary>
            /// Returns an evaluated Linq.Expression.
            /// </summary>
            /// <param name="expression">The expression to evaluate.</param>
            /// <returns>
            /// The modified expression, if it or any sub-expression was modified; otherwise,
            /// returns the original expression.
            /// </returns>
            internal Expression Eval(Expression expression)
            {
                return this.Visit(expression);
            }

            /// <summary>
            /// Returns an evaluated Linq.Expression.
            /// </summary>
            /// <param name="expression">The expression to evaluate.</param>
            /// <returns>
            /// The evaluated expression
            /// </returns>
            private static Expression Evaluate(Expression expression)
            {
                if (expression.NodeType == ExpressionType.Constant)
                {
                    return expression;
                }

                LambdaExpression lambda = Expression.Lambda(expression);
                Delegate function = lambda.Compile();
                return Expression.Constant(function.DynamicInvoke(null), expression.Type);
            }
        }
            #endregion

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        private class Nominator : ExpressionVisitor
        {
            #region Fields
            /// <summary>
            /// A function that decides whether a given expression node can be part of the local function.
            /// </summary>
            private readonly Func<Expression, bool> functionCanBeEvaluated;

            /// <summary>
            /// The candidates for evaluation.
            /// </summary>
            private HashSet<Expression> candidates;

            /// <summary>
            /// Whether the function can be evaluated.
            /// </summary>
            private bool cannotBeEvaluated;
            #endregion

            #region Constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="T:EFBootstrap.Caching.Evaluator.Nominator">Nominator</see> class. 
            /// </summary>
            /// <param name="functionCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
            internal Nominator(Func<Expression, bool> functionCanBeEvaluated)
            {
                this.functionCanBeEvaluated = functionCanBeEvaluated;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Dispatches the expression to one of the more specialized visit methods in this
            /// class.
            /// </summary>
            /// <param name="node">The expression to visit.</param>
            /// <returns>
            /// The modified expression, if it or any sub-expression was modified; otherwise,
            /// returns the original expression.
            /// </returns>
            public override Expression Visit(Expression node)
            {
                if (node != null)
                {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(node);
                    if (!this.cannotBeEvaluated)
                    {
                        if (this.functionCanBeEvaluated(node))
                        {
                            this.candidates.Add(node);
                        }
                        else
                        {
                            this.cannotBeEvaluated = true;
                        }
                    }

                    this.cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return node;
            }

            /// <summary>
            /// Returns an adjusted collection of expressions nominated for evaluation.
            /// </summary>
            /// <param name="expression">The expression to visit.</param>
            /// <returns>A an adjusted collection of expressions nominated for evaluation.</returns>
            internal HashSet<Expression> Nominate(Expression expression)
            {
                this.candidates = new HashSet<Expression>();
                this.Visit(expression);
                return this.candidates;
            }
            #endregion
        }
    }
}
