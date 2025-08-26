using lofi.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lofi.Data
{
    public sealed class SqliteTodoRepository : ITodoRepository
    {
        private readonly string _conn;

        public SqliteTodoRepository()
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "lofi_todo.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            _conn = $"Data Source={dbPath}";
        }

        public async Task InitializeAsync()
        {
            await using var c = new SqliteConnection(_conn);
            await c.OpenAsync();

            await new SqliteCommand("PRAGMA foreign_keys=ON;", c).ExecuteNonQueryAsync();
            await new SqliteCommand("PRAGMA journal_mode=WAL;", c).ExecuteNonQueryAsync();
            await new SqliteCommand("PRAGMA busy_timeout=5000;", c).ExecuteNonQueryAsync();

            var ddl = @"
            CREATE TABLE IF NOT EXISTS todos(
              id         INTEGER PRIMARY KEY AUTOINCREMENT,
              content    TEXT    NOT NULL CHECK (length(trim(content))>0),
              is_done    INTEGER NOT NULL DEFAULT 0 CHECK (is_done IN(0,1)),
              is_deleted INTEGER NOT NULL DEFAULT 0 CHECK (is_deleted IN(0,1))
            );
            CREATE INDEX IF NOT EXISTS idx_todos_active ON todos(is_deleted, is_done);";
            await new SqliteCommand(ddl, c).ExecuteNonQueryAsync();
        }

        public async Task<List<(int Id, string Content, bool IsDone)>> GetAllAsync()
        {
            var list = new List<(int, string, bool)>();
            await using var c = new SqliteConnection(_conn);
            await c.OpenAsync();

            var cmd = c.CreateCommand();
            cmd.CommandText = @"SELECT id,content,is_done FROM todos WHERE is_deleted=0 ORDER BY id DESC;";

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add((rd.GetInt32(0), rd.GetString(1), rd.GetInt32(2) == 1));
            }
            return list;
        }

        public async Task<int> AddAsync(string content)
        {
            await using var c = new SqliteConnection(_conn);
            await c.OpenAsync();

            var cmd = c.CreateCommand();
            cmd.CommandText = @"INSERT INTO todos(content,is_done,is_deleted) VALUES ($c,0,0); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$c", content);
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task SetDoneAsync(int id, bool isDone)
        {
            await using var c = new SqliteConnection(_conn);
            await c.OpenAsync();

            var cmd = c.CreateCommand();
            cmd.CommandText = "UPDATE todos SET is_done=$d WHERE id=$id;";
            cmd.Parameters.AddWithValue("$d", isDone ? 1 : 0);
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            await using var c = new SqliteConnection(_conn);
            await c.OpenAsync();

            var cmd = c.CreateCommand();
            cmd.CommandText = "UPDATE todos SET is_deleted=1 WHERE id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
