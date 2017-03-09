/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using System.Collections.Generic;

namespace v8unpack
{
	/// <summary>
	/// Чтение файла формата Ава. (http://infostart.ru/public/19734/ http://infostart.ru/public/187832/)
	/// </summary>
	[ContextClass("ЧтениеАваФайла")]
	public sealed class AwaFileReader : AutoContext<AwaFileReader>, IDisposable
	{

		private readonly Stream _reader;
		private readonly int _blockCount;
		private readonly string _version;
		private readonly int _pageSize;
		private readonly IAwaObjectReader _objectReader;

		private readonly AwaFileFormat.RootObjectHeader _rootObject;

		/// <summary>
		/// Создаёт чтение Ава-файла по входящему потоку данных.
		/// </summary>
		/// <param name="reader">Входящий поток.</param>
		public AwaFileReader(Stream reader)
		{
			_reader = reader;
			AwaFileFormat.FileHeader header;
			ReadHeader(out header, out _rootObject);
			_blockCount = header.Length;
			_version = header.Version;
			_pageSize = header.PageSize;
			_objectReader = header.ObjectReader;

			var objects = new List<AwaObject>();
			foreach (var fileIndex in _rootObject.FileIndexes)
			{
				var fileOffset = fileIndex * _pageSize;
				_reader.Seek(fileOffset, SeekOrigin.Begin);

				using (var currentObjectReader = _objectReader.OpenObjectStream(_reader))
				{
					var awaObject = new AwaObject(currentObjectReader.Length, fileIndex);
					objects.Add(awaObject);
				}
			}

			Elements = new AwaObjectCollection(objects);
		}

		private void ReadHeader(out AwaFileFormat.FileHeader header, out AwaFileFormat.RootObjectHeader rootObject)
		{
			var page0 = new byte[4096];
			_reader.Read(page0, 0, page0.Length);
			header = AwaFileFormat.FileHeader.FromBytes(page0);
			var pageSize = header.PageSize;

			if (pageSize > page0.Length)
			{
				_reader.Seek(pageSize - page0.Length, SeekOrigin.Current);
			}

			_reader.Seek(2 * pageSize, SeekOrigin.Begin);
			using (var rootReader = header.ObjectReader.OpenObjectStream(_reader))
			{
				rootObject = header.ObjectReader.ReadRootObject(rootReader);
			}
		}

		/// <summary>
		/// Длина файла в блоках
		/// </summary>
		/// <value>Длина файла в блоках.</value>
		[ContextProperty("Длина")]
		public decimal Length
		{
			get
			{
				return _blockCount;
			}
		}

		/// <summary>
		/// Версия формата файла.
		/// </summary>
		/// <value>Версия формата файла.</value>
		[ContextProperty("Версия")]
		public string Version
		{
			get
			{
				return _version;
			}
		}

		/// <summary>
		/// Объекты, содержащиеся в Ава-файле.
		/// </summary>
		/// <value>Элементы Ава-файла.</value>
		[ContextProperty("Элементы")]
		public AwaObjectCollection Elements { get; }


		[ContextMethod("Извлечь")]
		public void Extract(AwaObject element, string filename)
		{
			_reader.Seek(_pageSize * element.HeaderPageIndex, SeekOrigin.Begin);
			using (var reader = _objectReader.OpenObjectStream(_reader))
			using (var writer = new FileStream(filename, FileMode.Create))
			{
				reader.CopyTo(writer);
			}

		}

		[ContextMethod("ИзвлечьВсе")]
		public void ExtractAll(string dirname)
		{
			var index = 0;
			foreach (var element in Elements)
			{
				var filename = Path.Combine(dirname, string.Format("{0}.header", index));
				Extract(element, filename);
				++index;
			}
		}

		/// <summary>
		/// По имени файла. Открывает чтение Ава-файла.
		/// </summary>
		/// <returns>ЧтениеАваФайла.</returns>
		/// <param name="fileName">File name.</param>
		[ScriptConstructor]
		public static IRuntimeContextInstance Constructor(IValue fileName)
		{
			Stream reader = new FileStream(fileName.AsString(), FileMode.Open);
			return new AwaFileReader(reader);
		}

		public void Dispose()
		{
			_reader.Dispose();
		}
	}
}
