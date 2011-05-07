using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using log4net;
using Propaganda.Core.Util;
using Propaganda.Core.Interfaces.Database;

namespace Propaganda.DBService
{
    /// <summary>
    /// IService providing the infrastructure for interacting with db4o
    /// </summary>
    public class Db4oService : IDatabaseService
    {
        /// <summary>
        /// A logger for this class
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof (Db4oService));

        /// <summary>
        /// Main database file for <b>Propaganda</b>
        /// </summary>
        private IObjectServer _theDatabase;

        public Db4oService() { }

        #region IService Members

        public void Initialise()
        {
            _log.Debug("Loading " + Name);

            try
            {
                IConfiguration config = Db4oFactory.CloneConfiguration();
                config.LockDatabaseFile(false);
                config.UpdateDepth(2);

                // open the database connection
                _theDatabase = Db4oFactory.OpenServer(config, Db4oServiceConstants.DB_FILE_NAME, 0);
            }
            catch (Db4oException e)
            {
                throw new InitialisationException("Problem loading " + Name, e);
            }
        }

        public void Dispose()
        {
            if (null != _theDatabase)
            {
                _theDatabase.Close();
            }
        }

        #endregion

        /// <summary>
        /// Retrieve an IObjectContainer which is the key into the database
        /// </summary>
        /// <returns></returns>
        public IObjectContainer GetClient()
        {
            _log.Debug("Opening connection to DB");
            return _theDatabase.OpenClient();
        }

        #region IComponent Members


        public string Name
        {
            get { return "db4o Database Service"; }
        }

        #endregion
    }
}