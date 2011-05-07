using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using log4net;
using Propaganda.Core;
using Propaganda.Core.Interfaces;
using Propaganda.Core.Util;

namespace Propaganda.DBService
{
    /// <summary>
    /// <code>IService</code> providing the infrastructure for interacting with db4o
    /// </summary>
    public class DBService : IService
    {
        /// <summary>
        /// A logger for this class
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof (DBService));

        /// <summary>
        /// Maintain a reference to the core of <b>Propaganda</b>
        /// </summary>
        private PropagandaCore _theCore;

        /// <summary>
        /// Main database file for <b>Propaganda</b>
        /// </summary>
        private IObjectServer _theDatabase;

        #region IService Members

        public void Start(object coreRef)
        {
            _log.Debug("Loading " + Identify());
            _theCore = coreRef as PropagandaCore;

            try
            {
                IConfiguration config = Db4oFactory.CloneConfiguration();
                config.LockDatabaseFile(false);
                config.UpdateDepth(2);

                // open the database connection
                _theDatabase = Db4oFactory.OpenServer(config, DBServiceConstants.DB_FILE_NAME, 0);
            }
            catch (Db4oException e)
            {
                throw new InitialisationException("Problem loading " + Identify(), e);
            }
        }

        public void Stop()
        {
            if (null != _theDatabase)
            {
                _theDatabase.Close();
            }
        }

        public string Identify()
        {
            return "db4o Database Service";
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
    }
}