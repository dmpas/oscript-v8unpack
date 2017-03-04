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

namespace v8unpack
{
	/// <summary>
	/// Восьмофайл.
	/// </summary>
	[ContextClass("ФайлФормата8")]
	public class File8 : AutoContext<File8>
	{

		internal File8(FileFormat.ElementHeader header, UInt32 dataOffset)
		{
			DataOffset = (int)dataOffset;

			Name = header.Name;
			ModificationTime = header.ModificationDate;
			CreationTime = header.CreationDate;
		}

		/// <summary>
		/// Содержит имя файла в контейнере.
		/// </summary>
		/// <value>Имя файла.</value>
		[ContextProperty("Имя")]
		public string Name { get; }

		/// <summary>
		/// Содержит Дату и время последнего изменения файла.
		/// </summary>
		/// <value>Время изменения.</value>
		[ContextProperty("ВремяИзменения")]
		public DateTime ModificationTime { get; }

		/// <summary>
		/// Содержит дату и время создания файла.
		/// </summary>
		/// <value>Время создания.</value>
		[ContextProperty("ВремяСоздания")]
		public DateTime CreationTime { get; }

		/// <summary>
		/// Смещение блока данных в контейнере.
		/// </summary>
		/// <value>Смещение блока данных в контейнере.</value>
		public int DataOffset { get; }
	}
}
