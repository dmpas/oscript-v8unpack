/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
/*
	Author:			disa_da
	E-mail:			disa_da2@mail.ru
/**
	2014-2017       dmpas           sergey(dot)batanov(at)dmpas(dot)ru
*/

using System;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using System.IO;
using System.Collections.Generic;
using Ionic.Zlib;

namespace v8unpack
{
	/// <summary>
	/// Осуществляет чтение данных из контейнера восьмофайлов.
	/// </summary>
	[ContextClass("ЧтениеФайла8")]
	public class File8Reader : AutoContext<File8Reader>, IDisposable
	{
		private readonly Stream _reader;
		private readonly bool _dataPacked;
		private int _storageVersion;

		public File8Reader(Stream reader, bool dataPacked = true)
		{
			_reader = reader;
			_dataPacked = dataPacked;
			var fileList = ReadFileList();
			Elements = new File8Collection(fileList);
		}

		private List<File8> ReadFileList()
		{
			var containerHeader = FileFormat.ContainerHeader.Read(_reader);
			_storageVersion = (int)containerHeader.StorageVer;
			var elemsAddrsBuf = BlockReaderStream.ReadDataBlock(_reader);
			var addresses = FileFormat.ElementAddress.Parse(elemsAddrsBuf);

			var fileList = new List<File8>();
			foreach (var address in addresses)
			{
				if (address.HeaderAddress == FileFormat.V8_FF_SIGNATURE
					|| address.Signature != FileFormat.V8_FF_SIGNATURE)
				{
					continue;
				}

				_reader.Seek(address.HeaderAddress, SeekOrigin.Begin);
				var buf = BlockReaderStream.ReadDataBlock(_reader);

				var fileHeader = FileFormat.ElementHeader.Parse(buf);
				fileList.Add(new File8(fileHeader, address.DataAddress));
			}

			return fileList;
		}

		/// <summary>
		/// Версия формата контейнера.
		/// </summary>
		/// <value>Версия формата контейнера.</value>
		[ContextProperty("ВерсияФормата")]
		public decimal StorageVersion
		{
			get
			{
				return _storageVersion;
			}
		}

		/// <summary>
		/// Коллекция файлов контейнера.
		/// </summary>
		/// <value>Версия формата контейнера.</value>
		[ContextProperty("Элементы")]
		public File8Collection Elements { get; }

		/// <summary>
		/// Извлекает указанный файл из контейнера.
		/// </summary>
		/// <param name="element">Выбранный файл.</param>
		/// <param name="destDir">Каталог назначения.</param>
		/// <param name="recursiveUnpack">Если установлен в Истина, то все найденные вложенные восьмофайлы
		/// будут распакованы в отдельные подкаталоги. Необязательный.</param>
		[ContextMethod("Извлечь")]
		public void Extract(File8 element, string destDir, bool recursiveUnpack = false)
		{

			if (!Directory.Exists(destDir))
			{
				Directory.CreateDirectory(destDir);
			}

			_reader.Seek(element.DataOffset, SeekOrigin.Begin);

			Stream fileExtractor;
			var blockExtractor = new BlockReaderStream(_reader);
			if (blockExtractor.IsPacked && _dataPacked)
			{
				fileExtractor = new DeflateStream(blockExtractor, CompressionMode.Decompress);
			}
			else
			{
				fileExtractor = blockExtractor;
			}

			if (blockExtractor.IsContainer && recursiveUnpack)
			{
				string outputDirectory = Path.Combine(destDir, element.Name);
				var tmpData = new MemoryStream(); // TODO: переделать MemoryStream --> FileStream
				fileExtractor.CopyTo(tmpData);
				tmpData.Seek(0, SeekOrigin.Begin);

				var internalContainer = new File8Reader(tmpData, dataPacked: false);
				internalContainer.ExtractAll(outputDirectory, recursiveUnpack);

				return;
			}

			// Просто файл
			string outputFileName = Path.Combine(destDir, element.Name);
			using (var outputFile = new FileStream(outputFileName, FileMode.Create))
			{
				fileExtractor.CopyTo(outputFile);
			}
		}

		/// <summary>
		/// Извлекает все файлы из контейнера.
		/// </summary>
		/// <param name="destDir">Каталог назначения.</param>
		/// <param name="recursiveUnpack">Если установлен в Истина, то все найденные вложенные восьмофайлы
		/// будут распакованы в отдельные подкаталоги. Необязательный.</param>
		[ContextMethod("ИзвлечьВсе")]
		public void ExtractAll(string destDir, bool recursiveUnpack = false)
		{
			foreach (var element in Elements)
			{
				Extract(element, destDir, recursiveUnpack);
			}
		}

		public void Dispose()
		{
			_reader.Close();
		}

		[ContextMethod("Закрыть")]
		public void Close()
		{
			_reader.Close();
		}

		/// <summary>
		/// Создаёт чтение контейнера восьмофайлов по имени файла.
		/// </summary>
		/// <param name="filename">Путь к файлу.</param>
		[ScriptConstructor]
		public static IRuntimeContextInstance Constructor(IValue filename)
		{
			const int MAGIC_SIZE = 100 * 1024;
			var fileStream = new FileStream(filename.AsString(), FileMode.Open);
			if (fileStream.Length >= MAGIC_SIZE)
			{
				return new File8Reader(fileStream);
			}

			var memoryStream = new MemoryStream();
			fileStream.CopyTo(memoryStream);

			memoryStream.Seek(0, SeekOrigin.Begin);
			return new File8Reader(memoryStream);
		}

	}
}
