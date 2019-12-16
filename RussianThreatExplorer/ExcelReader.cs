using ExcelDataReader;
using System;
using System.Data;
using System.IO;

namespace RussianThreatExplorer
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
	        try
	        {
		        object result = (T) _table.Rows[row][col];

		        if (result is string text)
			        result = ReplaceInvalidSymbols(text);

		        return (T) result;
	        }
	        catch
	        {
		        return default(T);
	        }
        }

        private static string ReplaceInvalidSymbols(string text)
        {
	        return text.Replace("_x000d_", "");
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
