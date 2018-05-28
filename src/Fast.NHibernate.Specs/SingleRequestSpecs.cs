using NHibernate;
namespace Fast.NHibernate.Specs
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class SingleRequestSpecs
    {
        [Fact]
        public async Task Should_be_able_update_record()
        {
            using (var sessionFactory = new InMemoryDbSessionFactory<CarClassMap>())
            using (ISession session = sessionFactory.OpenSession())
            {
                await session.SaveAsync(new Car() { Name = "BMW", Year = 2000 });

                session.SingleRequestUpdate<Car>()
                    .SetProperty(c => c.Year, 2001)
                    .Where(c => c.Id, 1)
                    .Execute();

                var updatedCar = await session.GetAsync<Car>(1);
                updatedCar.Year.Should().Be(2001);
            }
        }

        [Fact]
        public async Task Should_be_able_to_delete_record()
        {
            using (var sessionFactory = new InMemoryDbSessionFactory<CarClassMap>())
            using (ISession session = sessionFactory.OpenSession())
            {
                await session.SaveAsync(new Car() { Name = "BMW", Year = 2000 });
                await session.SaveAsync(new Car() { Name = "Porsche", Year = 2000 });

                session.SingleRequestDeletion<Car>()
                    .Where(c => c.Name, "BMW")
                    .Execute();

                var deletedCar = await session.GetAsync<Car>(1);
                var remainedCar = await session.GetAsync<Car>(2);
                deletedCar.Should().BeNull();
                remainedCar.Should().NotBeNull();
            }
        }
    }
}
