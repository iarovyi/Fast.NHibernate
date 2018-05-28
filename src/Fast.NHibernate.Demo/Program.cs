using NHibernate;
namespace Fast.NHibernate.Demo
{
    using Fast.NHibernate.Specs;
    using static System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            using (var sessionFactory = new InMemoryDbSessionFactory<CarClassMap>())
            using (ISession session = sessionFactory.OpenSession())
            {
                WriteLine("Updating single or multiple database records using NHibernate with single database request");
                session.SingleRequestUpdate<Car>()
                    .SetProperty(c => c.Year, 2000)
                    .Where(c => c.Id, 46)
                    .Execute();

                WriteLine("Deleting single or multiple database records using NHibernate with single database request");
                session.SingleRequestDeletion<Car>()
                    .Where(c => c.Id, 46)
                    .Execute();
            }

            WriteLine("Finished successfully");
            ReadKey();
        }
    }
}
