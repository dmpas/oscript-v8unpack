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
	internal class Awa_8_3_8_ObjectReader : Awa_8_2_14_ObjectReader
	{
		/* TODO: 8.3.8
		private readonly int _pageSize;

		Awa_8_3_8_ObjectReader(int pageSize)
		{
			_pageSize = pageSize;
		}

		public override Stream OpenObjectStream(Stream reader)
		{
			var buf = new byte[_pageSize];
			reader.Read(buf, 0, buf.Length);

			var extendedStorage = BitConverter.ToInt16(buf, 2);
			var version = BitConverter.ToInt32(buf, 4);
			var dataSize = BitConverter.ToInt64(buf, 16);

			var offset = 24;
			int pagesCount = (buf.Length - offset) / 4;

			for (int i = 0; i < pagesCount; i++)
			{
				var pageIndex = BitConverter.ToInt32(buf, offset);
				var pageOffset = pageIndex * _pageSize;
				{
				}
				offset += 4;
			}
			return new AwaFileFormat.ObjectHeader(dataSize, version, pages);
		}
		*/
	}
}
