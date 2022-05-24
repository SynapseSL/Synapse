using LiteDB;
using Synapse.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Database
{
    public abstract class Repository<TK> : IRawRepository where TK : IDatabaseEntity
    {
        public Type GenericType => typeof(TK);

        /// <summary>
        /// Creates a new Database transaction
        ///
        /// It is absolutely vital to dispose this
        /// transaction since it would cancel out all
        /// other transactions on the system 
        /// </summary>
        [Unstable]
        public RepositoryTransaction<TK> Transaction 
            => new RepositoryTransaction<TK>();

        [API]
        public TK GetById(int id)
        {
            using var trc = Transaction;
            return trc.Collection.FindById(id);
        }

        [API]
        public TR Query<TR>(Func<ILiteQueryable<TK>, TR> func)
        {
            using var trc = Transaction;
            return func.Invoke(trc.Query());
        }

        [API]
        public TK Get(BsonExpression query)
        {
            using var trc = Transaction;
            return trc.Collection.FindOne(query);
        }

        [API]
        public bool Exists(BsonExpression query)
        {
            using var trc = Transaction;
            return trc.Collection.Exists(query);
        }

        [API]
        public TK Insert(TK tk)
        {
            using var trc = Transaction;
            _ = trc.Collection.Insert(tk);
            return tk;
        }

        [API]
        public int Insert(IEnumerable<TK> list)
        {
            using var trc = Transaction;
            return trc.Collection.InsertBulk(list);
        }

        [API]
        public bool Delete(TK tk)
        {
            using var trc = Transaction;
            return trc.Collection.Delete(tk.GetId());
        }

        [API]
        public bool Save(TK tk)
        {
            using var trc = Transaction;
            return trc.Collection.Update(tk);
        }

        [API]
        public List<TK> All()
        {
            using var trc = Transaction;
            return trc.Collection.FindAll().ToList();
        }

        [API]
        public List<TK> Find(BsonExpression query)
        {
            using var trc = Transaction;
            return trc.Collection.Find(query).ToList();
        }
    }

    public interface IRawRepository { }

    public class RepositoryTransaction<TK> : IDisposable where TK : IDatabaseEntity
    {
        public LiteDatabase Database;
        public ILiteCollection<TK> Collection;

        public RepositoryTransaction()
        {
            Database = DatabaseManager.LiteDatabase;
            Collection = Database.GetCollection<TK>($"{typeof(TK).Assembly.GetName().Name.Replace(" ", "")}_{typeof(TK).Name}");

        }

        public ILiteQueryable<TK> Query() => Collection.Query();

        public void Dispose()
        {
            _ = Database.Commit();
            Database.Dispose();
        }
    }

    public interface IDatabaseEntity
    {
        int GetId();
    }
}