using MyEvernote.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.DataAccessLayer.EntityFramework
{
    public class RepositoryBase
    {
        protected static DatabaseContext Context;
        private static object _lockSync = new object();
        protected RepositoryBase()
        {
            CreateContext();
        }
        public static void CreateContext()
        {
            if (Context == null)
            {
                lock (_lockSync)
                {
                    Context = new DatabaseContext();
                }
            }
            
        }
    }
}
