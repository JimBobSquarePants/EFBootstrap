using EFBootstrap.Sessions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace EFBootstrap
{
    class Class1
    {
        public string Name { get; set; }

        public ICollection<Class2> Class2s { get; set; }
        public ICollection<Class3> Class3s { get; set; }
    }

    class Class2
    {

    }

    class Class3
    {

    }

    class SessionWrapper : ReadOnlySession
    {

        public SessionWrapper(DbContext context)
            : base(context)
        {

        }

    }


    class Class4
    {

        public void GetALL()
        {

            DbContext x = new DbContext("jnjkn");

            SessionWrapper session = new SessionWrapper(x);

            IQueryable<Class1> classes = session.AnyWithInclude<Class1>(t => t.Name == "someId", t => t.Class2s, t => t.Class3s);
        }
    }
}
