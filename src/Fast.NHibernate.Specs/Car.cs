namespace Fast.NHibernate.Specs
{
    using FluentNHibernate.Mapping;

    public class Car
    {
        public virtual int Id { get; set; }
        public virtual int Year { get; set; }
        public virtual string Name { get; set; }
    }

    public class CarClassMap : ClassMap<Car>
    {
        public CarClassMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Year);
        }
    }
}
