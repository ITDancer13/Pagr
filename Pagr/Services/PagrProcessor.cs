using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using Pagr.Attributes;
using Pagr.Exceptions;
using Pagr.Extensions;
using Pagr.Models;

namespace Pagr.Services
{
    public class PagrProcessor : PagrProcessor<PagrModel, FilterTerm, SortTerm>, IPagrProcessor
    {
        public PagrProcessor(IOptions<PagrOptions> options) : base(options)
        {
        }

        public PagrProcessor(IOptions<PagrOptions> options, IPagrCustomSortMethods customSortMethods) : base(options, customSortMethods)
        {
        }

        public PagrProcessor(IOptions<PagrOptions> options, IPagrCustomFilterMethods customFilterMethods) : base(options, customFilterMethods)
        {
        }

        public PagrProcessor(IOptions<PagrOptions> options, IPagrCustomSortMethods customSortMethods, IPagrCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }
    }

    public class PagrProcessor<TFilterTerm, TSortTerm> : PagrProcessor<PagrModel<TFilterTerm, TSortTerm>, TFilterTerm, TSortTerm>, IPagrProcessor<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        public PagrProcessor(IOptions<PagrOptions> options) : base(options)
        {
        }

        public PagrProcessor(IOptions<PagrOptions> options, IPagrCustomSortMethods customSortMethods) : base(options, customSortMethods)
        {
        }

        public PagrProcessor(IOptions<PagrOptions> options, IPagrCustomFilterMethods customFilterMethods) : base(options, customFilterMethods)
        {
        }

        public PagrProcessor(IOptions<PagrOptions> options, IPagrCustomSortMethods customSortMethods, IPagrCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }
    }

    public class PagrProcessor<TPagrModel, TFilterTerm, TSortTerm> : IPagrProcessor<TPagrModel, TFilterTerm, TSortTerm>
        where TPagrModel : class, IPagrModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        private const string nullFilterValue = "null";
        private readonly IOptions<PagrOptions> _options;
        private readonly IPagrCustomSortMethods _customSortMethods;
        private readonly IPagrCustomFilterMethods _customFilterMethods;
        private readonly PagrPropertyMapper mapper = new PagrPropertyMapper();

        public PagrProcessor(IOptions<PagrOptions> options,
            IPagrCustomSortMethods customSortMethods,
            IPagrCustomFilterMethods customFilterMethods)
        {
            mapper = MapProperties(mapper);
            _options = options;
            _customSortMethods = customSortMethods;
            _customFilterMethods = customFilterMethods;
        }

        public PagrProcessor(IOptions<PagrOptions> options,
            IPagrCustomSortMethods customSortMethods)
        {
            mapper = MapProperties(mapper);
            _options = options;
            _customSortMethods = customSortMethods;
        }

        public PagrProcessor(IOptions<PagrOptions> options,
            IPagrCustomFilterMethods customFilterMethods)
        {
            mapper = MapProperties(mapper);
            _options = options;
            _customFilterMethods = customFilterMethods;
        }

        public PagrProcessor(IOptions<PagrOptions> options)
        {
            mapper = MapProperties(mapper);
            _options = options;
        }

        /// <summary>
        /// Apply filtering, sorting, and pagination parameters found in `model` to `source`
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model">An instance of IPagrModel</param>
        /// <param name="source">Data source</param>
        /// <param name="dataForCustomMethods">Additional data that will be passed down to custom methods</param>
        /// <param name="applyFiltering">Should the data be filtered? Defaults to true.</param>
        /// <param name="applySorting">Should the data be sorted? Defaults to true.</param>
        /// <param name="applyPagination">Should the data be paginated? Defaults to true.</param>
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> Apply<TEntity>(
            TPagrModel model,
            IQueryable<TEntity> source,
            object[] dataForCustomMethods = null,
            bool applyFiltering = true,
            bool applySorting = true,
            bool applyPagination = true)
        {
            var result = source;

            if (model == null)
            {
                return result;
            }

            try
            {
                // Filter
                if (applyFiltering)
                {
                    result = ApplyFiltering(model, result, dataForCustomMethods);
                }

                // Sort
                if (applySorting)
                {
                    result = ApplySorting(model, result, dataForCustomMethods);
                }

                // Paginate
                if (applyPagination)
                {
                    result = ApplyPagination(model, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (_options.Value.ThrowExceptions)
                {
                    if (ex is PagrException)
                    {
                        throw;
                    }

                    throw new PagrException(ex.Message, ex);
                }
                else
                {
                    return result;
                }
            }
        }

        private IQueryable<TEntity> ApplyFiltering<TEntity>(
            TPagrModel model,
            IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.GetFiltersParsed() == null)
            {
                return result;
            }

            Expression outerExpression = null;
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            foreach (var filterTerm in model.GetFiltersParsed())
            {
                Expression innerExpression = null;
                foreach (var filterTermName in filterTerm.Names)
                {
                    var (fullPropertyName, property) = GetPagrProperty<TEntity>(false, true, filterTermName);
                    if (property != null)
                    {
                        Expression propertyValue = parameter;
                        Expression nullCheck = null;
                        var names = fullPropertyName.Split('.');
                        for (var i = 0; i < names.Length; i++)
                        {
                            propertyValue = Expression.PropertyOrField(propertyValue, names[i]);

                            if (i != names.Length - 1 && propertyValue.Type.IsNullable())
                            {
                                nullCheck = GenerateFilterNullCheckExpression(propertyValue, nullCheck);
                            }
                        }

                        if (filterTerm.Values == null) continue;

                        var converter = TypeDescriptor.GetConverter(property.PropertyType);
                        foreach (var filterTermValue in filterTerm.Values)
                        {
                            var isFilterTermValueNull = filterTermValue.ToLower() == nullFilterValue;
                            var filterValue = isFilterTermValueNull
                                ? Expression.Constant(null, property.PropertyType)
                                : ConvertStringValueToConstantExpression(filterTermValue, property, converter);

                            if (filterTerm.OperatorIsCaseInsensitive)
                            {
                                propertyValue = Expression.Call(propertyValue,
                                    typeof(string).GetMethods()
                                    .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));

                                filterValue = Expression.Call(filterValue,
                                    typeof(string).GetMethods()
                                    .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));
                            }

                            var expression = GetExpression(filterTerm, filterValue, propertyValue);

                            if (filterTerm.OperatorIsNegated)
                            {
                                expression = Expression.Not(expression);
                            }

                            var filterValueNullCheck = !isFilterTermValueNull && propertyValue.Type.IsNullable()
                                ? GenerateFilterNullCheckExpression(propertyValue, nullCheck)
                                : nullCheck;

                            if (filterValueNullCheck != null)
                            {
                                expression = Expression.AndAlso(filterValueNullCheck, expression);
                            }

                            if (innerExpression == null)
                            {
                                innerExpression = expression;
                            }
                            else
                            {
                                innerExpression = Expression.OrElse(innerExpression, expression);
                            }
                        }
                    }
                    else
                    {
                        result = ApplyCustomMethod(result, filterTermName, _customFilterMethods,
                            new object[] {
                                            result,
                                            filterTerm.Operator,
                                            filterTerm.Values
                            }, dataForCustomMethods);

                    }
                }
                if (outerExpression == null)
                {
                    outerExpression = innerExpression;
                    continue;
                }
                if (innerExpression == null)
                {
                    continue;
                }
                outerExpression = Expression.AndAlso(outerExpression, innerExpression);
            }
            return outerExpression == null
                ? result
                : result.Where(Expression.Lambda<Func<TEntity, bool>>(outerExpression, parameter));
        }

        private static Expression GenerateFilterNullCheckExpression(Expression propertyValue, Expression nullCheckExpression)
        {
            return nullCheckExpression == null
                ? Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type))
                : Expression.AndAlso(nullCheckExpression, Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type)));
        }

        private Expression ConvertStringValueToConstantExpression(string value, PropertyInfo property, TypeConverter converter)
        {
            dynamic constantVal = converter.CanConvertFrom(typeof(string))
                ? converter.ConvertFrom(value)
                : Convert.ChangeType(value, property.PropertyType);

            return GetClosureOverConstant(constantVal, property.PropertyType);
        }

        private static Expression GetExpression(TFilterTerm filterTerm, dynamic filterValue, dynamic propertyValue)
        {
            switch (filterTerm.OperatorParsed)
            {
                case FilterOperator.Equals:
                    return Expression.Equal(propertyValue, filterValue);
                case FilterOperator.NotEquals:
                    return Expression.NotEqual(propertyValue, filterValue);
                case FilterOperator.GreaterThan:
                    return Expression.GreaterThan(propertyValue, filterValue);
                case FilterOperator.LessThan:
                    return Expression.LessThan(propertyValue, filterValue);
                case FilterOperator.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(propertyValue, filterValue);
                case FilterOperator.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(propertyValue, filterValue);
                case FilterOperator.Contains:
                    return Expression.Call(propertyValue,
                        typeof(string).GetMethods()
                        .First(m => m.Name == "Contains" && m.GetParameters().Length == 1),
                        filterValue);
                case FilterOperator.StartsWith:
                    return Expression.Call(propertyValue,
                        typeof(string).GetMethods()
                        .First(m => m.Name == "StartsWith" && m.GetParameters().Length == 1),
                        filterValue);
                default:
                    return Expression.Equal(propertyValue, filterValue);
            }
        }

        // Workaround to ensure that the filter value gets passed as a parameter in generated SQL from EF Core
        // See https://github.com/aspnet/EntityFrameworkCore/issues/3361
        // Expression.Constant passed the target type to allow Nullable comparison
        // See http://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
        private Expression GetClosureOverConstant<T>(T constant, Type targetType)
        {
            return Expression.Constant(constant, targetType);
        }

        private IQueryable<TEntity> ApplySorting<TEntity>(
            TPagrModel model,
            IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.GetSortsParsed() == null)
            {
                return result;
            }

            var useThenBy = false;
            foreach (var sortTerm in model.GetSortsParsed())
            {
                var (fullName, property) = GetPagrProperty<TEntity>(true, false, sortTerm.Name);

                if (property != null)
                {
                    result = result.OrderByDynamic(fullName, property, sortTerm.Descending, useThenBy);
                }
                else
                {
                    result = ApplyCustomMethod(result, sortTerm.Name, _customSortMethods,
                        new object[]
                        {
                        result,
                        useThenBy,
                        sortTerm.Descending
                        }, dataForCustomMethods);
                }
                useThenBy = true;
            }

            return result;
        }

        private IQueryable<TEntity> ApplyPagination<TEntity>(
            TPagrModel model,
            IQueryable<TEntity> result)
        {
            var page = model?.Page ?? 1;
            var pageSize = model?.PageSize ?? _options.Value.DefaultPageSize;
            var maxPageSize = _options.Value.MaxPageSize > 0 ? _options.Value.MaxPageSize : pageSize;

            if (pageSize > 0)
            {
                result = result.Skip((page - 1) * pageSize);
                result = result.Take(Math.Min(pageSize, maxPageSize));
            }

            return result;
        }

        protected virtual PagrPropertyMapper MapProperties(PagrPropertyMapper mapper)
        {
            return mapper;
        }

        private (string, PropertyInfo) GetPagrProperty<TEntity>(
            bool canSortRequired,
            bool canFilterRequired,
            string name)
        {
            var property = mapper.FindProperty<TEntity>(canSortRequired, canFilterRequired, name, _options.Value.CaseSensitive);
            if (property.Item1 == null)
            {
                var prop = FindPropertyByPagrAttribute<TEntity>(canSortRequired, canFilterRequired, name, _options.Value.CaseSensitive);
                return (prop?.Name, prop);
            }
            return property;

        }

        private PropertyInfo FindPropertyByPagrAttribute<TEntity>(
            bool canSortRequired,
            bool canFilterRequired,
            string name,
            bool isCaseSensitive)
        {
            return Array.Find(typeof(TEntity).GetProperties(), p =>
            {
                return p.GetCustomAttribute(typeof(PagrAttribute)) is PagrAttribute pagrAttribute
                && (!canSortRequired || pagrAttribute.CanSort)
                && (!canFilterRequired || pagrAttribute.CanFilter)
                && (pagrAttribute.Name ?? p.Name).Equals(name, isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            });
        }

        private IQueryable<TEntity> ApplyCustomMethod<TEntity>(IQueryable<TEntity> result, string name, object parent, object[] parameters, object[] optionalParameters = null)
        {
            var customMethod = parent?.GetType()
                .GetMethodExt(name,
                _options.Value.CaseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                typeof(IQueryable<TEntity>));


            if (customMethod == null)
            {
                // Find generic methods `public IQueryable<T> Filter<T>(IQueryable<T> source, ...)`
                var genericCustomMethod = parent?.GetType()
                .GetMethodExt(name,
                _options.Value.CaseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                typeof(IQueryable<>));

                if (genericCustomMethod != null &&
                    genericCustomMethod.ReturnType.IsGenericType &&
                    genericCustomMethod.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    var genericBaseType = genericCustomMethod.ReturnType.GenericTypeArguments[0];
                    var constraints = genericBaseType.GetGenericParameterConstraints();
                    if (constraints == null || constraints.Length == 0 || constraints.All((t) => t.IsAssignableFrom(typeof(TEntity))))
                    {
                        customMethod = genericCustomMethod.MakeGenericMethod(typeof(TEntity));
                    }
                }
            }

            if (customMethod != null)
            {
                try
                {
                    result = customMethod.Invoke(parent, parameters)
                        as IQueryable<TEntity>;
                }
                catch (TargetParameterCountException)
                {
                    if (optionalParameters != null)
                    {
                        result = customMethod.Invoke(parent, parameters.Concat(optionalParameters).ToArray())
                            as IQueryable<TEntity>;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                var incompatibleCustomMethods = parent?
                                                    .GetType()
                                                    .GetMethods
                                                    (
                                                        _options.Value.CaseSensitive
                                                            ? BindingFlags.Default
                                                            : BindingFlags.IgnoreCase | BindingFlags.Public |
                                                              BindingFlags.Instance
                                                    )
                                                    .Where(method => string.Equals(method.Name, name,
                                                        _options.Value.CaseSensitive
                                                            ? StringComparison.InvariantCulture
                                                            : StringComparison.InvariantCultureIgnoreCase))
                                                    .ToList()
                                                ??
                                                new List<MethodInfo>();

                if (incompatibleCustomMethods.Any())
                {
                    var incompatibles =
                        from incompatibleCustomMethod in incompatibleCustomMethods
                        let expected = typeof(IQueryable<TEntity>)
                        let actual = incompatibleCustomMethod.ReturnType
                        select new PagrIncompatibleMethodException(name, expected, actual,
                            $"{name} failed. Expected a custom method for type {expected} but only found for type {actual}");

                    var aggregate = new AggregateException(incompatibles);

                    throw new PagrIncompatibleMethodException(aggregate.Message, aggregate);
                }
                else
                {
                    throw new PagrMethodNotFoundException(name, $"{name} not found.");
                }
            }

            return result;
        }
    }
}
