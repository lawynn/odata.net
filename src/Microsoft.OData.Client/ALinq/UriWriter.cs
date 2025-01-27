//---------------------------------------------------------------------
// <copyright file="UriWriter.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Client
{
    #region Namespaces

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Microsoft.OData.Client.Metadata;
    using Microsoft.OData.UriParser.Aggregation;

    #endregion Namespaces

    /// <summary>
    /// Translates resource bound expression trees into URIs.
    /// </summary>
    internal class UriWriter : DataServiceALinqExpressionVisitor
    {
        /// <summary>Data context used to generate type names for types.</summary>
        private readonly DataServiceContext context;

        /// <summary>stringbuilder for constructed URI.</summary>
        private readonly StringBuilder uriBuilder = new StringBuilder();

        /// <summary>The dictionary to store the alias.</summary>
        private readonly Dictionary<string, string> alias = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>the request data service version for the uri.</summary>
        private Version uriVersion;

        /// <summary>
        /// For caching query options to be grouped
        /// </summary>
        private Dictionary<string, List<string>> cachedQueryOptions = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        /// <summary>
        /// Private constructor for creating UriWriter
        /// </summary>
        /// <param name='context'>Data context used to generate type names for types.</param>
        private UriWriter(DataServiceContext context)
        {
            Debug.Assert(context != null, "context != null");
            this.context = context;
            this.uriVersion = Util.ODataVersion4;
        }

        /// <summary>
        /// Translates resource bound expression tree to a URI.
        /// </summary>
        /// <param name='context'>Data context used to generate type names for types.</param>
        /// <param name="addTrailingParens">flag to indicate whether generated URI should include () if leaf is ResourceSet</param>
        /// <param name="e">The expression to translate</param>
        /// <param name="uri">uri</param>
        /// <param name="version">version for query</param>
        internal static void Translate(DataServiceContext context, bool addTrailingParens, Expression e, out Uri uri, out Version version)
        {
            var writer = new UriWriter(context);
            writer.Visit(e);
            string fullUri = writer.uriBuilder.ToString();

            if (writer.alias.Any())
            {
                if (fullUri.IndexOf(UriHelper.QUESTIONMARK, StringComparison.Ordinal) > -1)
                {
                    fullUri += UriHelper.AMPERSAND;
                }
                else
                {
                    fullUri += UriHelper.QUESTIONMARK;
                }

                foreach (var kv in writer.alias)
                {
                    fullUri += kv.Key;
                    fullUri += UriHelper.EQUALSSIGN;
                    fullUri += kv.Value;
                    fullUri += UriHelper.AMPERSAND;
                }

                fullUri = fullUri.Substring(0, fullUri.Length - 1);
            }


            uri = UriUtil.CreateUri(fullUri, UriKind.Absolute);
            version = writer.uriVersion;
        }

        /// <summary>
        /// MethodCallExpression visit method
        /// </summary>
        /// <param name="m">The MethodCallExpression expression to visit</param>
        /// <returns>The visited MethodCallExpression expression </returns>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            throw Error.MethodNotSupported(m);
        }

        /// <summary>
        /// UnaryExpression visit method
        /// </summary>
        /// <param name="u">The UnaryExpression expression to visit</param>
        /// <returns>The visited UnaryExpression expression </returns>
        internal override Expression VisitUnary(UnaryExpression u)
        {
            throw new NotSupportedException(Error.Format(SRResources.ALinq_UnaryNotSupported, u.NodeType.ToString()));
        }

        /// <summary>
        /// BinaryExpression visit method
        /// </summary>
        /// <param name="b">The BinaryExpression expression to visit</param>
        /// <returns>The visited BinaryExpression expression </returns>
        internal override Expression VisitBinary(BinaryExpression b)
        {
            throw new NotSupportedException(Error.Format(SRResources.ALinq_BinaryNotSupported, b.NodeType.ToString()));
        }

        /// <summary>
        /// ConstantExpression visit method
        /// </summary>
        /// <param name="c">The ConstantExpression expression to visit</param>
        /// <returns>The visited ConstantExpression expression </returns>
        internal override Expression VisitConstant(ConstantExpression c)
        {
            throw new NotSupportedException(Error.Format(SRResources.ALinq_ConstantNotSupported, c.Value));
        }

        /// <summary>
        /// TypeBinaryExpression visit method
        /// </summary>
        /// <param name="b">The TypeBinaryExpression expression to visit</param>
        /// <returns>The visited TypeBinaryExpression expression </returns>
        internal override Expression VisitTypeIs(TypeBinaryExpression b)
        {
            throw new NotSupportedException(SRResources.ALinq_TypeBinaryNotSupported);
        }

        /// <summary>
        /// ConditionalExpression visit method
        /// </summary>
        /// <param name="c">The ConditionalExpression expression to visit</param>
        /// <returns>The visited ConditionalExpression expression </returns>
        internal override Expression VisitConditional(ConditionalExpression c)
        {
            throw new NotSupportedException(SRResources.ALinq_ConditionalNotSupported);
        }

        /// <summary>
        /// ParameterExpression visit method
        /// </summary>
        /// <param name="p">The ParameterExpression expression to visit</param>
        /// <returns>The visited ParameterExpression expression </returns>
        internal override Expression VisitParameter(ParameterExpression p)
        {
            throw new NotSupportedException(SRResources.ALinq_ParameterNotSupported);
        }

        /// <summary>
        /// MemberExpression visit method
        /// </summary>
        /// <param name="m">The MemberExpression expression to visit</param>
        /// <returns>The visited MemberExpression expression </returns>
        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            throw new NotSupportedException(Error.Format(SRResources.ALinq_MemberAccessNotSupported, m.Member.Name));
        }

        /// <summary>
        /// LambdaExpression visit method
        /// </summary>
        /// <param name="lambda">The LambdaExpression to visit</param>
        /// <returns>The visited LambdaExpression</returns>
        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            throw new NotSupportedException(SRResources.ALinq_LambdaNotSupported);
        }

        /// <summary>
        /// NewExpression visit method
        /// </summary>
        /// <param name="nex">The NewExpression to visit</param>
        /// <returns>The visited NewExpression</returns>
        internal override NewExpression VisitNew(NewExpression nex)
        {
            throw new NotSupportedException(SRResources.ALinq_NewNotSupported);
        }

        /// <summary>
        /// MemberInitExpression visit method
        /// </summary>
        /// <param name="init">The MemberInitExpression to visit</param>
        /// <returns>The visited MemberInitExpression</returns>
        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            throw new NotSupportedException(SRResources.ALinq_MemberInitNotSupported);
        }

        /// <summary>
        /// ListInitExpression visit method
        /// </summary>
        /// <param name="init">The ListInitExpression to visit</param>
        /// <returns>The visited ListInitExpression</returns>
        internal override Expression VisitListInit(ListInitExpression init)
        {
            throw new NotSupportedException(SRResources.ALinq_ListInitNotSupported);
        }

        /// <summary>
        /// NewArrayExpression visit method
        /// </summary>
        /// <param name="na">The NewArrayExpression to visit</param>
        /// <returns>The visited NewArrayExpression</returns>
        internal override Expression VisitNewArray(NewArrayExpression na)
        {
            throw new NotSupportedException(SRResources.ALinq_NewArrayNotSupported);
        }

        /// <summary>
        /// InvocationExpression visit method
        /// </summary>
        /// <param name="iv">The InvocationExpression to visit</param>
        /// <returns>The visited InvocationExpression</returns>
        internal override Expression VisitInvocation(InvocationExpression iv)
        {
            throw new NotSupportedException(SRResources.ALinq_InvocationNotSupported);
        }

        /// <summary>
        /// NavigationPropertySingletonExpression visit method.
        /// </summary>
        /// <param name="npse">NavigationPropertySingletonExpression expression to visit</param>
        /// <returns>Visited NavigationPropertySingletonExpression expression</returns>
        internal override Expression VisitNavigationPropertySingletonExpression(NavigationPropertySingletonExpression npse)
        {
            this.Visit(npse.Source);
            this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(this.ExpressionToString(npse.MemberExpression, /*inPath*/ true));
            this.VisitQueryOptions(npse);
            return npse;
        }

        /// <summary>
        /// QueryableResourceExpression visit method.
        /// </summary>
        /// <param name="rse">QueryableResourceExpression expression to visit</param>
        /// <returns>Visited QueryableResourceExpression expression</returns>
        internal override Expression VisitQueryableResourceExpression(QueryableResourceExpression rse)
        {
            if ((ResourceExpressionType)rse.NodeType == ResourceExpressionType.ResourceNavigationProperty)
            {
                if (rse.IsOperationInvocation && !(rse.Source is QueryableResourceExpression))
                {
                    var normalizerRewrites = new Dictionary<Expression, Expression>(ReferenceEqualityComparer<Expression>.Instance);
                    var e = Evaluator.PartialEval(rse.Source);
                    e = ExpressionNormalizer.Normalize(e, normalizerRewrites);
                    e = ResourceBinder.Bind(e, this.context);
                    this.Visit(e);
                }
                else
                {
                    this.Visit(rse.Source);
                }

                this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(this.ExpressionToString(rse.MemberExpression, /*inPath*/ true));
            }
            else if (rse.MemberExpression != null)
            {
                // this is a resource set expression
                // we should be at the very beginning of
                // the URI
                Debug.Assert(this.uriBuilder.Length == 0, "The builder is not empty while we are adding a resourceset");
                string entitySetName = (String)((ConstantExpression)rse.MemberExpression).Value;
                this.uriBuilder.Append(this.context.BaseUriResolver.GetEntitySetUri(entitySetName));
            }
            else
            {
                this.uriBuilder.Append(this.context.BaseUriResolver.BaseUriOrNull);
            }

            WebUtil.RaiseVersion(ref this.uriVersion, rse.UriVersion);

            if (rse.ResourceTypeAs != null)
            {
                this.uriBuilder.Append(UriHelper.FORWARDSLASH);
                UriHelper.AppendTypeSegment(this.uriBuilder, rse.ResourceTypeAs, this.context, /*inPath*/ true, ref this.uriVersion);
            }

            if (rse.KeyPredicateConjuncts.Count > 0)
            {
                this.context.UrlKeyDelimiter.AppendKeyExpression(rse.GetKeyProperties(), kvp => ClientTypeUtil.GetServerDefinedName(kvp.Key), kvp => kvp.Value.Value, this.uriBuilder);
            }

            if (rse.IsOperationInvocation)
            {
                this.VisitOperationInvocation(rse);
            }

            if (rse.CountOption == CountOption.CountSegment)
            {
                // append $count segment: /$count
                this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(UriHelper.DOLLARSIGN).Append(UriHelper.COUNT);
            }

            this.VisitQueryOptions(rse);
            return rse;
        }


        /// <summary>
        /// Visit Function Invocation
        /// </summary>
        /// <param name="rse">Resource Expression with function invocation</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        internal void VisitOperationInvocation(QueryableResourceExpression rse)
        {
            if (!this.uriBuilder.ToString().EndsWith(UriHelper.FORWARDSLASH.ToString(), StringComparison.Ordinal))
            {
                this.uriBuilder.Append(UriHelper.FORWARDSLASH);
            }

            if (rse.IsOperationInvocation)
            {
                this.uriBuilder.Append(rse.OperationName);
                if (rse.IsAction)
                {
                    return;
                }

                this.uriBuilder.Append(UriHelper.LEFTPAREN);
                bool needComma = false;
                KeyValuePair<string, string>[] parameters = rse.OperationParameters.ToArray();
                for (int i = 0; i < parameters.Length; ++i)
                {
                    KeyValuePair<string, string> param = parameters[i];
                    if (needComma)
                    {
                        this.uriBuilder.Append(UriHelper.COMMA);
                    }

                    this.uriBuilder.Append(param.Key);
                    this.uriBuilder.Append(UriHelper.EQUALSSIGN);

                    // non-primitive value, use alias.
                    if (!UriHelper.IsPrimitiveValue(param.Value))
                    {
                        string aliasName = UriHelper.ATSIGN + param.Key;
                        int count = 1;
                        while (this.alias.ContainsKey(aliasName))
                        {
                            aliasName = UriHelper.ATSIGN + param.Key + count.ToString(CultureInfo.InvariantCulture);
                            count++;
                        }

                        this.uriBuilder.Append(aliasName);

                        this.alias.Add(aliasName, param.Value);
                    }
                    else
                    {
                        // primitive value, do not use alias.
                        this.uriBuilder.Append(param.Value);
                    }

                    needComma = true;
                }

                this.uriBuilder.Append(UriHelper.RIGHTPAREN);
            }
        }

        /// <summary>
        /// Visit Query options for Resource
        /// </summary>
        /// <param name="re">Resource Expression with query options</param>
        internal void VisitQueryOptions(ResourceExpression re)
        {
            if (re.HasQueryOptions)
            {
                this.uriBuilder.Append(UriHelper.QUESTIONMARK);

                QueryableResourceExpression rse = re as QueryableResourceExpression;
                if (rse != null)
                {
                    IEnumerator options = rse.SequenceQueryOptions.GetEnumerator();
                    while (options.MoveNext())
                    {
                        Expression e = ((Expression)options.Current);
                        ResourceExpressionType et = (ResourceExpressionType)e.NodeType;
                        switch (et)
                        {
                            case ResourceExpressionType.SkipQueryOption:
                                this.VisitQueryOptionExpression((SkipQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.TakeQueryOption:
                                this.VisitQueryOptionExpression((TakeQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.OrderByQueryOption:
                                this.VisitQueryOptionExpression((OrderByQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.FilterQueryOption:
                                this.VisitQueryOptionExpression((FilterQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.ApplyQueryOption:
                                this.VisitQueryOptionExpression((ApplyQueryOptionExpression)e);
                                break;
                            default:
                                Debug.Assert(false, "Unexpected expression type " + ((int)et).ToString(CultureInfo.InvariantCulture));
                                break;
                        }
                    }
                }

                if (re.ExpandPaths.Count > 0)
                {
                    this.VisitExpandOptions(re.ExpandPaths);
                }

                if (re.Projection != null && re.Projection.Paths.Count > 0)
                {
                    this.VisitProjectionPaths(re.Projection.Paths);
                }

                if (re.CountOption == CountOption.CountQueryTrue)
                {
                    this.VisitCountQueryOptions(true);
                }

                if (re.CountOption == CountOption.CountQueryFalse)
                {
                    this.VisitCountQueryOptions(false);
                }

                if (re.CustomQueryOptions.Count > 0)
                {
                    this.VisitCustomQueryOptions(re.CustomQueryOptions);
                }

                this.AppendCachedQueryOptionsToUriBuilder();
            }
        }

        /// <summary>
        /// SkipQueryOptionExpression visit method.
        /// </summary>
        /// <param name="sqoe">SkipQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(SkipQueryOptionExpression sqoe)
        {
            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONSKIP, this.ExpressionToString(sqoe.SkipAmount, /*inPath*/ false));
        }

        /// <summary>
        /// TakeQueryOptionExpression visit method.
        /// </summary>
        /// <param name="tqoe">TakeQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(TakeQueryOptionExpression tqoe)
        {
            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONTOP, this.ExpressionToString(tqoe.TakeAmount, /*inPath*/ false));
        }

        /// <summary>
        /// FilterQueryOptionExpression visit method.
        /// </summary>
        /// <param name="fqoe">FilterQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(FilterQueryOptionExpression fqoe)
        {
            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONFILTER, this.ExpressionToString(fqoe.GetPredicate(), /*inPath*/ false));
        }

        /// <summary>
        /// OrderByQueryOptionExpression visit method.
        /// </summary>
        /// <param name="oboe">OrderByQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(OrderByQueryOptionExpression oboe)
        {
            StringBuilder tmpBuilder = new StringBuilder();
            int ii = 0;
            while (true)
            {
                var selector = oboe.Selectors[ii];

                tmpBuilder.Append(this.ExpressionToString(selector.Expression, /*inPath*/ false));
                if (selector.Descending)
                {
                    tmpBuilder.Append(UriHelper.SPACE);
                    tmpBuilder.Append(UriHelper.OPTIONDESC);
                }

                if (++ii == oboe.Selectors.Count)
                {
                    break;
                }

                tmpBuilder.Append(UriHelper.COMMA);
            }

            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONORDERBY, tmpBuilder.ToString());
        }

        /// <summary>
        /// VisitExpandOptions visit method.
        /// </summary>
        /// <param name="paths">Expand Paths</param>
        internal void VisitExpandOptions(List<string> paths)
        {
            StringBuilder tmpBuilder = new StringBuilder();
            int ii = 0;
            while (true)
            {
                tmpBuilder.Append(paths[ii]);

                if (++ii == paths.Count)
                {
                    break;
                }

                tmpBuilder.Append(UriHelper.COMMA);
            }

            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONEXPAND, tmpBuilder.ToString());
        }

        /// <summary>
        /// ProjectionPaths visit method.
        /// </summary>
        /// <param name="paths">Projection Paths</param>
        internal void VisitProjectionPaths(List<string> paths)
        {
            StringBuilder tmpBuilder = new StringBuilder();
            int ii = 0;
            while (true)
            {
                string path = paths[ii];

                tmpBuilder.Append(path);

                if (++ii == paths.Count)
                {
                    break;
                }

                tmpBuilder.Append(UriHelper.COMMA);
            }

            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONSELECT, tmpBuilder.ToString());
        }

        /// <summary>
        /// VisitCountQueryOptions visit method.
        /// </summary>
        /// <param name = "countQueryOption" > Count query option, either true or false</param>
        internal void VisitCountQueryOptions(bool countQueryOption)
        {
            if (countQueryOption)
            {
                this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONCOUNT, UriHelper.COUNTTRUE);
            }
            else
            {
                this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONCOUNT, UriHelper.COUNTFALSE);
            }
        }

        /// <summary>
        /// VisitCustomQueryOptions visit method.
        /// </summary>
        /// <param name="options">Custom query options</param>
        internal void VisitCustomQueryOptions(Dictionary<ConstantExpression, ConstantExpression> options)
        {
            List<ConstantExpression> keys = options.Keys.ToList();
            List<ConstantExpression> values = options.Values.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                string k = keys[i].Value + "";
                string v = values[i].Value + "";
                this.AddAsCachedQueryOption(k, v);
            }
        }

        /// <summary>
        /// ApplyQueryOptionExpression visit method.
        /// </summary>
        /// <param name="applyQueryOptionExpr">ApplyQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(ApplyQueryOptionExpression applyQueryOptionExpr)
        {
            // GroupBy with no aggregations is supported e.g. /Customers?$apply=groupby((Name))
            if (applyQueryOptionExpr.Aggregations.Count == 0 && applyQueryOptionExpr.GroupingExpressions.Count == 0)
            {
                return;
            }

            StringBuilder applyOptionBuilder = new StringBuilder();
            string aggregateTransformation = string.Empty;
            // E.g. filter(Amount gt 1)
            string filterTransformation = ConstructFilterTransformation(applyQueryOptionExpr);

            if (!string.IsNullOrEmpty(filterTransformation))
            {
                applyOptionBuilder.Append(filterTransformation);
                applyOptionBuilder.Append("/");
            }

            if (applyQueryOptionExpr.Aggregations.Count > 0)
            {
                // E.g. aggregate(Prop with sum as SumProp, Prop with average as AverageProp)
                aggregateTransformation = ConstructAggregateTransformation(applyQueryOptionExpr.Aggregations);
            }

            if (applyQueryOptionExpr.GroupingExpressions.Count == 0)
            {
                applyOptionBuilder.Append(aggregateTransformation);

                // E.g. $apply=aggregate(Prop with sum as SumProp, Prop with average as AverageProp)
                // Or $apply=filter(Amount gt 1)/aggregate(Prop with sum as SumProp, Prop with average as AverageProp)
                this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONAPPLY, applyOptionBuilder.ToString());
            }
            else
            {
                // E.g (Prop1, Prop2, ..., PropN)
                string groupingPropertiesExpr = ConstructGroupingExpression(applyQueryOptionExpr.GroupingExpressions);

                StringBuilder groupByBuilder = new StringBuilder();
                groupByBuilder.Append(applyOptionBuilder.ToString()); // This should add filter transformation if any
                groupByBuilder.Append(UriHelper.GROUPBY);
                groupByBuilder.Append(UriHelper.LEFTPAREN);
                groupByBuilder.Append(groupingPropertiesExpr);

                if (!string.IsNullOrEmpty(aggregateTransformation))
                {
                    // Scenario: GroupBy(d1 => d1.Prop, (d1, d2) => new { Prop = d1 })
                    groupByBuilder.Append(UriHelper.COMMA);
                    groupByBuilder.Append(aggregateTransformation);
                }

                groupByBuilder.Append(UriHelper.RIGHTPAREN);

                // E.g. $apply=groupby((Category),aggregate(Prop with sum as SumProp, Prop with average as AverageProp))
                // Or $apply=filter(Amount gt 1)/groupby((Category),aggregate(Prop with sum as SumProp, Prop with average as AverageProp))
                this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONAPPLY, groupByBuilder.ToString());
            }
        }

        /// <summary>
        /// Constructs a $apply filter transformation.
        /// E.g. $apply=filter(Amount gt 1)
        /// </summary>
        /// <param name="applyQueryOptionExpr">ApplyQueryOptionExpression expression</param>
        /// <returns>A filter transformation</returns>
        private string ConstructFilterTransformation(ApplyQueryOptionExpression applyQueryOptionExpr)
        {
            if (applyQueryOptionExpr.PredicateConjuncts.Count == 0)
            {
                return string.Empty;
            }

            return "filter(" + this.ExpressionToString(applyQueryOptionExpr.GetPredicate(), /*inPath*/ false) + ")";
        }

        /// <summary>
        /// Constructs a $apply aggregate transformation.
        /// E.g. $apply=aggregate(Prop with sum as SumProp, Prop with average as AverageProp)
        /// </summary>
        /// <param name="aggregations">List of aggregations.</param>
        /// <returns>The aggregate tranformation.</returns>
        private string ConstructAggregateTransformation(IList<ApplyQueryOptionExpression.Aggregation> aggregations)
        {
            StringBuilder aggregateBuilder = new StringBuilder();

            aggregateBuilder.Append(UriHelper.AGGREGATE);
            aggregateBuilder.Append(UriHelper.LEFTPAREN);
            int i = 0;

            while (true)
            {
                ApplyQueryOptionExpression.Aggregation aggregation = aggregations[i];
                AggregationMethod aggregationMethod = aggregation.AggregationMethod;
                string aggregationAlias = aggregation.AggregationAlias;

                string aggregationUriEquivalent;
                if (!TypeSystem.TryGetUriEquivalent(aggregationMethod, out aggregationUriEquivalent))
                {
                    // This would happen if an aggregation method was added to the enum with no
                    // relevant update to map it to the URI equivalent 
                    throw new NotSupportedException(Error.Format(SRResources.ALinq_AggregationMethodNotSupported, aggregationMethod.ToString()));
                }

                string aggregationProperty = string.Empty;

                // E.g. Amount with sum as SumAmount (For $count aggregation: $count as Count)
                if (aggregationMethod != AggregationMethod.VirtualPropertyCount)
                {
                    aggregationProperty = this.ExpressionToString(aggregation.Expression, /*inPath*/ false);

                    aggregateBuilder.Append(aggregationProperty);
                    aggregateBuilder.Append(UriHelper.SPACE);
                    aggregateBuilder.Append(UriHelper.WITH);
                    aggregateBuilder.Append(UriHelper.SPACE);
                }

                aggregateBuilder.Append(aggregationUriEquivalent);
                aggregateBuilder.Append(UriHelper.SPACE);
                aggregateBuilder.Append(UriHelper.AS);
                aggregateBuilder.Append(UriHelper.SPACE);
                // MUST define an alias for the resulting aggregate value
                // Concatenate aggregation method with aggregation property to generate a simple identifier/alias
                // OASIS Standard: The alias MUST NOT collide with names of declared properties, custom aggregates, or other aliases in that type
                // TODO: Strategy to avoid name collision - Append a Guid?
                if (string.IsNullOrEmpty(aggregationAlias))
                {
                    aggregationAlias = aggregationMethod.ToString() + aggregationProperty.Replace('/', '_');
                }

                aggregateBuilder.Append(aggregationAlias);

                if (++i == aggregations.Count)
                {
                    break;
                }

                aggregateBuilder.Append(UriHelper.COMMA);
            }

            aggregateBuilder.Append(UriHelper.RIGHTPAREN);

            return aggregateBuilder.ToString();
        }

        /// <summary>
        /// Constructs a $apply grouping expression.
        /// E.g. (Prop1, Prop2, ..., PropN)
        /// </summary>
        /// <param name="groupingExpressions">List of grouping expressions.</param>
        /// <returns>The grouping expression.</returns>
        private string ConstructGroupingExpression(IList<Expression> groupingExpressions)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(UriHelper.LEFTPAREN);
            int i = 0;

            while (true)
            {
                Expression groupingExpression = groupingExpressions[i];
                builder.Append(this.ExpressionToString(groupingExpression, /*inPath*/ false));

                if (++i == groupingExpressions.Count)
                {
                    break;
                }
                builder.Append(UriHelper.COMMA);
            }

            builder.Append(UriHelper.RIGHTPAREN);

            return builder.ToString();
        }

        /// <summary>
        /// Caches query option to be grouped
        /// </summary>
        /// <param name="optionKey">The key.</param>
        /// <param name="optionValue">The value</param>
        private void AddAsCachedQueryOption(string optionKey, string optionValue)
        {
            List<string> tmp;
            if (!this.cachedQueryOptions.TryGetValue(optionKey, out tmp))
            {
                tmp = new List<string>();
                this.cachedQueryOptions.Add(optionKey, tmp);
            }

            tmp.Add(optionValue);
        }

        /// <summary>
        /// Append all cached query options to uri.
        /// </summary>
        private void AppendCachedQueryOptionsToUriBuilder()
        {
            int i = 0;
            foreach (var queryOption in this.cachedQueryOptions)
            {
                if (i++ != 0)
                {
                    this.uriBuilder.Append(UriHelper.AMPERSAND);
                }

                string keyStr = queryOption.Key;
                string valueStr = string.Join(",", queryOption.Value);
                this.uriBuilder.Append(keyStr);
                this.uriBuilder.Append(UriHelper.EQUALSSIGN);
                this.uriBuilder.Append(valueStr);
            }
        }

        /// <summary>Serializes an expression to a string.</summary>
        /// <param name="expression">Expression to serialize</param>
        /// <param name='inPath'>Whether or not the expression being written is part of the path of the URI.</param>
        /// <returns>The serialized expression.</returns>
        private string ExpressionToString(Expression expression, bool inPath)
        {
            return ExpressionWriter.ExpressionToString(this.context, expression, inPath, ref this.uriVersion);
        }
    }
}
