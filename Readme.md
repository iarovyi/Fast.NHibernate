# Fast.NHinernate

NHinernate extention for updating and deleting records from single table with single database request

```
session.SingleRequestUpdate<Car>()
	.SetProperty(c => c.Year, 2000)
	.SetProperty(c => c.Cathergory, "New")
	.Where(c => c.Id, 46)
	.Execute();

session.SingleRequestDeletion<Car>()
	.Where(c => c.Cathergory, "New")
	.Where(c => c.Year, 2000)
	.Execute();

```