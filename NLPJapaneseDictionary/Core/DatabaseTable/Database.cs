/**
 * Copyright © 2017-2018 Anki Universal Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using SQLite;
using System.Collections;

namespace NLPJapaneseDictionary.DatabaseTable.NLPJDictCore
{
    public class DBCorruptException : Exception
    {
        public string message;
        public DBCorruptException(string message, SQLiteException error)
            : base(message, error)
        {

        }
    }

    public class Database : IDisposable
    {
        private SQLiteConnection databaseConnection;

        public Database(string absolutePath)
        {
            try
            {                
                databaseConnection = new SQLiteConnection(absolutePath);
            }
            catch (SQLiteException e)
            {
                if (databaseConnection == null)
                {
                    string msg = String.Format("Can't open the database at {0}", absolutePath);
                    throw new DBCorruptException(msg, e);
                }
            }
        }

        public void Dispose()
        {            
            databaseConnection.Dispose();
        }

        public string GetPath()
        {
            return databaseConnection.DatabasePath;
        }

        public void Execute(string sql, params object[] obj)
        {
            databaseConnection.Execute(sql, obj);
        }

        public void RunInTransaction(Action action)
        {
            databaseConnection.RunInTransaction(action);
        }

        public bool HasTable<T>() where T : class
        {
            var name = typeof(T).Name;
            var count = databaseConnection.ExecuteScalar<int>("SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = ?", name);
            if (count > 0)
                return true;
            else
                return false;
        }

        public bool HasTable(string name) 
        {
            var count = databaseConnection.ExecuteScalar<int>("SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = ?", name);
            if (count > 0)
                return true;
            else
                return false;
        }

        public string SaveTransactionPoint()
        {
            return databaseConnection.SaveTransactionPoint();
        }

        public void Rollback()
        {
            databaseConnection.Rollback();
        }

        public void RollbackTo(string savepoint)
        {
            databaseConnection.RollbackTo(savepoint);
        }

        public void BeginTransaction()
        {
            databaseConnection.BeginTransaction();
        }

        public void Commit()
        {
            databaseConnection.Commit();
        }

        public T QueryScalar<T>(string query)
        {
            return databaseConnection.ExecuteScalar<T>(query);
        }

        public T QueryScalar<T>(string query, params object[] obj)
        {
            return databaseConnection.ExecuteScalar<T>(query, obj);
        }

        public List<T> QueryColumn<T>(string query) where T : class, new()
        {
            return databaseConnection.Query<T>(query);
        }

        public List<T> QueryColumn<T>(string query, params object[] args) where T : class, new()
        {
            return databaseConnection.Query<T>(query, args);
        }

        public List<T> QueryFirstRow<T>(string query) where T : class, new()
        {
            string s = " limit 1";
            return databaseConnection.Query<T>(query + s);
        }

        public List<T> QueryFirstRow<T>(string query, params object[] args) where T : class, new()
        {
            string s = " limit 1";
            return databaseConnection.Query<T>(query + s, args);
        }

        public void CreateTable<T>(CreateFlags flag = CreateFlags.None)
        {
            databaseConnection.CreateTable<T>(flag);
        }

        public void CreateTable(Type type, CreateFlags flag = CreateFlags.None)
        {
            databaseConnection.CreateTable(type, flag);
        }

        public void Insert(object obj, Type type = null)
        {
            if (type != null)
                databaseConnection.Insert(obj, type);
            else
                databaseConnection.Insert(obj);
        }

        public void InsertOrReplace(object obj, Type type = null)
        {
            if (type != null)
                databaseConnection.InsertOrReplace(obj, type);
            else
                databaseConnection.InsertOrReplace(obj);
        }

        public void Update(object obj, Type type = null)
        {            
            if (type != null)
                databaseConnection.Update(obj, type);
            else
                databaseConnection.Update(obj);
        }

        public void DropTable<T>()
        {
            try
            {
                databaseConnection.DropTable<T>();
            }
            catch //If we cannot drop table -> do nothing
            { }
        }

        public void InsertAll(IEnumerable obj, bool runInTransaction = true)
        {
            databaseConnection.InsertAll(obj, runInTransaction);
        }

        public void Delete<T>(object primaryKey)
        {
            databaseConnection.Delete<T>(primaryKey);
        }

        public void Close()
        {
            databaseConnection.Close();
        }

        public TableQuery<T> GetTable<T>() where T : class, new()
        {
            return databaseConnection.Table<T>();
        }
    }
}
