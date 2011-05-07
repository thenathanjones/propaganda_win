using Propaganda.Core.Interfaces.Database;
using Db4objects.Db4o;

namespace Propaganda.DBService
{
    /// <summary>
    /// Db4o specific client used for interacting with db4o databases
    /// </summary>
    public class Db4oClient : IDBClient
    {
        /// <summary>
        /// Underlying client container
        /// </summary>
        public IObjectContainer Client { get; set; }
        
        #region IDBClient Members

        public void Close()
        {
            if (Client != null)
                Client.Close();
        }

        #endregion

        #region IDBClient Members


        public void Commit()
        {
            if (Client != null)
                Client.Commit();
        }

        #endregion
    }
}


