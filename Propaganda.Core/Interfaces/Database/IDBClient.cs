namespace Propaganda.Core.Interfaces.Database
{
    public interface IDBClient
    {
        /// <summary>
        /// Close this client connection to the DB
        /// </summary>
        void Close();

        /// <summary>
        /// Commit the pending changes to the DB
        /// </summary>
        void Commit();
    }
}