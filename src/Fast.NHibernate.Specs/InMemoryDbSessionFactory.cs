using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Cfg;

namespace Fast.NHibernate.Specs
{
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using System;

    public class InMemoryDbSessionFactory<TClassMap> : IDisposable
    {
        private ISessionFactory sessionFactory;
        private Configuration configuration;

        public InMemoryDbSessionFactory()
        {
            sessionFactory = CreateSessionFactory();
            CreateSchema();
        }

        public ISession OpenSession() => sessionFactory.OpenSession();

        private ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().UsingFile($"{Guid.NewGuid()}.db").ShowSql())
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<TClassMap>())
                    .ExposeConfiguration(cfg => configuration = cfg)
                    .BuildSessionFactory();
        }

        private void CreateSchema()
        {
            using(ISession session = sessionFactory.OpenSession())
            {
                var export = new SchemaExport(configuration);
                export.Execute(true, true, false, session.Connection, null);
            }
        }

        public void Dispose() => sessionFactory?.Dispose();
    }
}
