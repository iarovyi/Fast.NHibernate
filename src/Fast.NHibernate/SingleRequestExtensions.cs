using NHibernate;
namespace Fast.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using static System.Environment;

    public static class SingleRequestExtensions
    {
        public static SingleRequestDeletionOperation<TEntity> SingleRequestDeletion<TEntity>(this ISession session) =>
            new SingleRequestDeletionOperation<TEntity>(session);

        public class SingleRequestDeletionOperation<TEntity>
        {
            private readonly ISession session;
            private readonly List<KeyValuePair<string, object>> filters = new List<KeyValuePair<string, object>>();

            internal SingleRequestDeletionOperation(ISession session)
            {
                this.session = session;
            }

            public SingleRequestDeletionOperation<TEntity> Where(Expression<Func<TEntity, object>> property, object propertyValue)
            {
                filters.Add(new KeyValuePair<string, object>(property.GetMemberName(), propertyValue));
                return this;
            }

            /// <summary>
            /// Delete multiple row from table with single database request. Results in flushed and cleaned session.
            /// Session is flushed and cleaned.
            /// </summary>
            /// <returns>The count of deleted entities.</returns>
            /// <example>
            /// <code>
            /// <![CDATA[
            /// session
            ///    .SingleRequestDeletion<Car>()
            ///    .Where(c => c.Id, 46)
            ///    .Execute();
            /// ]]>
            /// </code>
            /// </example>
            public int Execute()
            {
                session.Flush();
                session.Clear();

                var whereFilters = filters.Select((p, i) => $"entity.{p.Key} = :filter{i}");
                string where = $"WHERE {string.Join($" AND{NewLine}", whereFilters)}";
                var query = session
                    .CreateQuery($@"DELETE {typeof(TEntity).Name} entity
                                  {(filters.Any() ? where : "")}");

                for (int i = 0; i < filters.Count; i++)
                {
                    query = query.SetParameter($"filter{i}", filters[i].Value);
                }

                return query.ExecuteUpdate();
            }
        }

        public static SingleRequestUpdateOperation<TEntity> SingleRequestUpdate<TEntity>(this ISession session) =>
            new SingleRequestUpdateOperation<TEntity>(session);

        public class SingleRequestUpdateOperation<TEntity>
        {
            private readonly ISession session;
            private readonly List<KeyValuePair<string, object>> filters = new List<KeyValuePair<string, object>>();
            private readonly List<KeyValuePair<string, object>> updates = new List<KeyValuePair<string, object>>();

            internal SingleRequestUpdateOperation(ISession session)
            {
                this.session = session;
            }

            public SingleRequestUpdateOperation<TEntity> Where(Expression<Func<TEntity, object>> property, object propertyValue)
            {
                filters.Add(new KeyValuePair<string, object>(property.GetMemberName(), propertyValue));
                return this;
            }

            public SingleRequestUpdateOperation<TEntity> SetProperty(Expression<Func<TEntity, object>> property, object propertyValue)
            {
                updates.Add(new KeyValuePair<string, object>(property.GetMemberName(), propertyValue));
                return this;
            }

            /// <summary>
            /// Update multiple rows on table with single database request. Results in flushed and cleaned session
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown when no column were provided for update</exception>
            /// <returns>The number of entities updated</returns>
            /// <example>
            /// <code>
            /// <![CDATA[
            ///    session
            ///     .SingleRequestUpdate<Cars>()
            ///     .SetProperty(c => c.Year, 2000)
            ///     .SetProperty(c => c.Country, "USA")
            ///     .Where(c => c.Type, "Old american car")
            ///     .Execute();
            /// ]]>
            /// </code>
            /// </example>
            public int Execute()
            {
                if (!updates.Any())
                {
                    throw new InvalidOperationException("At least one column needs to be changed within update operation");
                }

                session.Flush();
                session.Clear();

                var whereFilters = filters.Select((p, i) => $"entity.{p.Key} = :filter{i}");
                var columnUpdates = updates.Select((p, i) => $"entity.{p.Key} = :arg{i}");
                string where = $"WHERE {string.Join($" AND{NewLine}", whereFilters)}";
                var query = session
                    .CreateQuery($@"UPDATE {typeof(TEntity).Name} entity
                                    SET   {string.Join($",{NewLine}", columnUpdates)}
                                    {(filters.Any() ? where : "")}");

                for (int i = 0; i < updates.Count; i++)
                {
                    query = query.SetParameter($"arg{i}", updates[i].Value);
                }

                for (int i = 0; i < filters.Count; i++)
                {
                    query = query.SetParameter($"filter{i}", filters[i].Value);
                }

                return query.ExecuteUpdate();
            }
        }
    }

    internal static class ExpressionExtensions
    {
        public static string GetMemberName<TObj>(this Expression<Func<TObj, object>> memberSelector)
            => GetMemberName(memberSelector.Body);

        private static string GetMemberName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return ((ParameterExpression)expression).Name;
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).Method.Name;
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return GetMemberName(((UnaryExpression)expression).Operand);
                case ExpressionType.Invoke:
                    return GetMemberName(((InvocationExpression)expression).Expression);
                case ExpressionType.ArrayLength:
                    return nameof(Array.Length);
                default:
                    throw new Exception("not a proper member selector");
            }
        }
    }
}
