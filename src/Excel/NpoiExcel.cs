﻿using Excel.Excel.Abstraction;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using src.ClassMapper;
using src.ClassMapper.Abstraction;
using src.Excel.Abstraction;
using src.Excel.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace src.Excel
{
    public class NpoiExcel : IExcel
    {
        private IWorkbook _workBook;

        private ConcurrentDictionary<string, Dictionary<int, string>> _headers = new ConcurrentDictionary<string, Dictionary<int, string>>();

        public void Load(Stream file)
        {
            _workBook = new XSSFWorkbook(file);
        }

        public ISheetValidateResult Validate<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public ISheetReadResult<T> Read<T>() where T : class, new()
        {
            var res = new SheetReadResult<T>();
            try
            {
                var mapper = GetClassMapper<T>();
                ISheet sheet = _workBook.GetSheet(mapper.MapName);
                ReadSheet(res, sheet);
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public ISheetWriteResult Write<T>(IEnumerable<T> data) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Save(Stream file, bool leaveOpen)
        {
            throw new NotImplementedException();
        }

        private void ReadSheet<T>(ISheetReadResult<T> res, ISheet sheet)
            where T : class, new()
        {
            for (int i = 1; i < sheet.PhysicalNumberOfRows; i++)
            {
                ReadRow(res, sheet.GetRow(i));
            }
        }

        private void ReadRow<T>(ISheetReadResult<T> res, IRow row)
            where T : class, new()
        {
            var data = new Dictionary<string, object>();
            var header = GetSheetHeader<T>();
            for (int i = 0; i < row.PhysicalNumberOfCells; i++)
            {
                var value = row.GetCell(i).GetValue();
                var propertyMapper = GetClassMapper<T>().PropertyMappers.Find(o => o.MapName == header[i]);
                data.Add(propertyMapper.Name, value);
            }
            res.Data.Add(JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data)));
        }

        private Dictionary<int, string> GetSheetHeader<T>()
            where T : class
        {
            var mapper = GetClassMapper<T>();
            if (!_headers.TryGetValue(mapper.MapName, out var header))
            {
                header = ReadSheetHeader(_workBook.GetSheet(mapper.MapName));
                _headers.TryAdd(mapper.MapName, header);
            }
            return header;
        }

        private Dictionary<int, string> ReadSheetHeader(ISheet sheet)
        {
            var res = new Dictionary<int, string>();
            var headerRow = sheet.GetRow(0);
            for (int i = 0; i < headerRow.PhysicalNumberOfCells; i++)
            {
                res.Add(i, headerRow.GetCell(i).StringCellValue);
            }
            return res;
        }

        private IClassMapper<T> GetClassMapper<T>() where T : class
        {
            var mapper = ClassMapperDict.GetMapper<T>();
            if (mapper == null)
            {
                
            }
            return mapper as IClassMapper<T>;
        }
    }
}
