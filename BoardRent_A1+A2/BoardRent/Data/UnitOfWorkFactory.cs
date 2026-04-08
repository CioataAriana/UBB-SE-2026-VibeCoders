namespace BoardRent.Data
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly AppDbContext _dbContext;

        public UnitOfWorkFactory(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IUnitOfWork Create()
        {
            return new UnitOfWork(_dbContext);
        }
    }
}
