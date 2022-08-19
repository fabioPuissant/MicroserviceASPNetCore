using System.Linq.Expressions;
using MongoDB.Driver;


namespace Play.Common.MongoDB
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {

        // Holds the mongo db collection (data)
        private readonly IMongoCollection<T> _dbCollection;

        // To build filters that allow for querying entries in the collection
        private readonly FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;


        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            // setup
            System.Diagnostics.Debug.WriteLine($"FABIO-MongoRepo:COLLECTION_NAME={collectionName}");
          
            _dbCollection = database.GetCollection<T>(collectionName);

        }


        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await _dbCollection.Find(filter: _filterBuilder.Empty).ToListAsync();
        }


        public async Task<T> GetByIdAsync(Guid id)
        {
            FilterDefinition<T> filter = _filterBuilder.Eq(existingEntity => existingEntity.Id, id);
            return await _dbCollection.Find(filter).FirstOrDefaultAsync();
        }


        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await _dbCollection.InsertOneAsync(entity);
        }


        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            FilterDefinition<T> filter = _filterBuilder.Eq(existingEntity => existingEntity.Id, entity.Id);
            await _dbCollection.ReplaceOneAsync(filter, entity);
        }


        public async Task RemoveAsync(Guid id)
        {
            FilterDefinition<T> filter = _filterBuilder.Eq(existingEntity => existingEntity.Id, id);
            await _dbCollection.DeleteOneAsync(filter);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbCollection.Find(filter).ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbCollection.Find(filter).FirstOrDefaultAsync();

        }
    }
}

