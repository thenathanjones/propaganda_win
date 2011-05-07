namespace Propaganda.Core.Interfaces.Database
{
    public interface IDatabase
    {
        /// <summary>
        /// Get a client accessing this DB
        /// </summary>
        /// <returns></returns>
        IDBClient RetrieveClient();
    }
}