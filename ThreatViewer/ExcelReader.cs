using ExcelDataReader;
using System;
using System.Data;
using System.IO;

namespace ThreatViewer
{
    class ExcelReader : IDisposable
    {
        private string _path;

        private IExcelDataReader _reader;
        private DataSet _set;
        private DataTable _table;

        public ExcelReader(string path)
        {
            _path = path;
            FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            _set = _reader.AsDataSet();
        }

        public void OpenTable(int i)
        {
            _table = _set.Tables[i];
        }

        public int GetRowsCount()
        {
            return _table.Rows.Count;
        }

        public T TryRead<T>(int row, int col)
        {
            try { return (T)_table.Rows[row][col]; }
            catch { return default(T); }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
