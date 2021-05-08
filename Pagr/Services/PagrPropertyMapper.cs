using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pagr.Models;

namespace Pagr.Services
{
    public class PagrPropertyMapper
    {
        private readonly Dictionary<Type, ICollection<KeyValuePair<PropertyInfo, IPagrPropertyMetadata>>> _map
            = new Dictionary<Type, ICollection<KeyValuePair<PropertyInfo, IPagrPropertyMetadata>>>();

        public PropertyFluentApi<TEntity> Property<TEntity>(Expression<Func<TEntity, object>> expression)
        {
            if (!_map.ContainsKey(typeof(TEntity)))
            {
                _map.Add(typeof(TEntity), new List<KeyValuePair<PropertyInfo, IPagrPropertyMetadata>>());
            }

            return new PropertyFluentApi<TEntity>(this, expression);
        }

        public class PropertyFluentApi<TEntity>
        {
            private readonly PagrPropertyMapper _pagrPropertyMapper;
            private readonly PropertyInfo _property;

            public PropertyFluentApi(PagrPropertyMapper pagrPropertyMapper, Expression<Func<TEntity, object>> expression)
            {
                _pagrPropertyMapper = pagrPropertyMapper;
                (_fullName, _property) = GetPropertyInfo(expression);
                _name = _fullName;
                _canFilter = false;
                _canSort = false;
            }

            private string _name;
            private readonly string _fullName;
            private bool _canFilter;
            private bool _canSort;

            public PropertyFluentApi<TEntity> CanFilter()
            {
                _canFilter = true;
                UpdateMap();
                return this;
            }

            public PropertyFluentApi<TEntity> CanSort()
            {
                _canSort = true;
                UpdateMap();
                return this;
            }

            public PropertyFluentApi<TEntity> HasName(string name)
            {
                _name = name;
                UpdateMap();
                return this;
            }

            private void UpdateMap()
            {
                var metadata = new PagrPropertyMetadata
                {
                    Name = _name,
                    FullName = _fullName,
                    CanFilter = _canFilter,
                    CanSort = _canSort
                };
                var pair = new KeyValuePair<PropertyInfo, IPagrPropertyMetadata>(_property, metadata);

                _pagrPropertyMapper._map[typeof(TEntity)].Add(pair);
            }

            private static (string, PropertyInfo) GetPropertyInfo(Expression<Func<TEntity, object>> exp)
            {
                if (!(exp.Body is MemberExpression body))
                {
                    var unaryExprBody = (UnaryExpression)exp.Body;
                    body = unaryExprBody.Operand as MemberExpression;
                }

                var member = body?.Member as PropertyInfo;
                var stack = new Stack<string>();
                while (body != null)
                {
                    stack.Push(body.Member.Name);
                    body = body.Expression as MemberExpression;
                }

                return (string.Join(".", stack.ToArray()), member);
            }
        }

        public (string, PropertyInfo) FindProperty<TEntity>(bool canSortRequired, bool canFilterRequired, string name, bool isCaseSensitive)
        {
            try
            {
                var (propertyInfo, metadata) = _map[typeof(TEntity)]
                    .FirstOrDefault(kv =>
                        kv.Value.Name.Equals(name, isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)
                        && (!canSortRequired || kv.Value.CanSort)
                        && (!canFilterRequired || kv.Value.CanFilter));

                return (metadata?.FullName, propertyInfo);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentNullException)
            {
                return (null, null);
            }
        }
    }
}
