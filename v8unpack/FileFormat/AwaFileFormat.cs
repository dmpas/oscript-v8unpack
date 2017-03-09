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

namespace v8unpack
{
	/// <summary>
	/// Формат Ава-файла. Описание по ссылкам:
	/// http://infostart.ru/public/19734/
	/// http://infostart.ru/public/187832/
	/// http://infostart.ru/public/536343/
	/// </summary>
	public static class AwaFileFormat
	{
		/// <summary>
		/// Заголовок Ава-файла
		/// </summary>
		public struct FileHeader
		{
			/// <summary>
			/// Версия формата файла.
			/// </summary>
			public readonly string Version;

			/// <summary>
			/// Длина файла в страницах.
			/// </summary>
			public readonly int Length;

			/// <summary>
			/// Размер страницы.
			/// </summary>
			public readonly int PageSize;

			/// <summary>
			/// Обработчик для указанной версии формата.
			/// </summary>
			public readonly IAwaObjectReader ObjectReader;

			FileHeader(string version,
			           int length,
			           int pageSize,
			           IAwaObjectReader objectReader)
			{
				this.Version = version;
				this.Length = length;
				this.PageSize = pageSize;
				this.ObjectReader = objectReader;
			}

			/// <summary>
			/// Загружает заголовок из страницы данных.
			/// </summary>
			/// <returns>Заголовок файла.</returns>
			/// <param name="buf">Страница данных.</param>
			public static FileHeader FromBytes(byte[] buf)
			{
				int ver1 = buf[8];
				int ver2 = buf[9];
				int ver3 = buf[10];
				int ver4 = buf[11];

				var version = string.Format("{0}.{1}.{2}.{3}", ver1, ver2, ver3, ver4);
				var length = BitConverter.ToInt32(buf, 12);

				var pageSize = 4096;
				IAwaObjectReader objectReader;
				if (version.Equals("8.3.8.0", StringComparison.Ordinal))
				{
					pageSize = BitConverter.ToInt32(buf, 20);
					objectReader = new Awa_8_3_8_ObjectReader();
				}
				else
				{
					objectReader = new Awa_8_2_14_ObjectReader();
				}

				return new FileHeader(version, length, pageSize, objectReader);
			}
		}

		/// <summary>
		/// Заголовок объекта Ава-файла.
		/// </summary>
		public struct ObjectHeader
		{
			/// <summary>
			/// Версия.
			/// </summary>
			public readonly int Version;

			/// <summary>
			/// Размер данных в байтах.
			/// </summary>
			public readonly long DataSize;

			/// <summary>
			/// Номера блоков хранения данных таблицы размещения.
			/// </summary>
			public readonly int[] AllocationTable;

			internal ObjectHeader(long dataSize, int version, int[] allocationTable)
			{
				this.Version = version;
				this.AllocationTable = allocationTable;
				this.DataSize = dataSize;
			}
		}

		/// <summary>
		/// Заголовок корневого объекта хранилища.
		/// </summary>
		public struct RootObjectHeader
		{
			/// <summary>
			/// Код локали.
			/// </summary>
			public readonly string Lang;

			/// <summary>
			/// Блоки.
			/// </summary>
			public readonly int[] FileIndexes;

			internal RootObjectHeader(string lang, int[] fileIndexes)
			{
				this.Lang = lang;
				this.FileIndexes = fileIndexes;
			}
		}

	}
}
