using Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    public class SqlServerEntity {
        protected Database Database;
        protected SqlServerDatabase SqlServerDatabase;

        protected void Initialize(ConnectionInfo connectionInfo) {
            var serverConnection = new ServerConnection(connectionInfo.HostName, connectionInfo.User, connectionInfo.Password);
            var server = new Server(serverConnection);

            server.Databases.Refresh();
            Database = server.Databases[connectionInfo.DatabaseName];
            SqlServerDatabase = new SqlServerDatabase(connectionInfo);
        }
    }
}