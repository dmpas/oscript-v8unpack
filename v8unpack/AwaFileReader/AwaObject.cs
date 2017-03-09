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
	/// Объект Ава-файла.
	/// </summary>
	[ContextClass("ОбъектАваФайла")]
	public sealed class AwaObject : AutoContext<AwaObject>
	{
		private readonly long _headerPageIndex;
		private readonly long _dataSize;

		public AwaObject(long dataSize, long headerPageIndex)
		{
			_dataSize = dataSize;
			_headerPageIndex = headerPageIndex;
		}

		/// <summary>
		/// Размер объекта в байтах.
		/// </summary>
		/// <value>Размер.</value>
		[ContextProperty("Размер")]
		public decimal Size
		{
			get
			{
				return _dataSize;
			}
		}

		public long HeaderPageIndex
		{
			get { return _headerPageIndex; }
		}

	}
}
