/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Collections.Generic;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace v8unpack
{
	internal class Awa_8_0_5_ObjectReader : IAwaObjectReader
	{

		private readonly int _pageSize = 4096;

		public virtual Stream OpenObjectStream(Stream reader)
		{
			var buf = new byte[_pageSize];
			reader.Read(buf, 0, buf.Length);

			var dataSize = BitConverter.ToInt32(buf, 8);
			var version = BitConverter.ToInt32(buf, 20);
			var pagesCount = 1018;
			var offset = 24;

			var fullPageList = new List<int>();
			for (int i = 0; i < pagesCount; i++)
			{
				var pageIndex = BitConverter.ToInt32(buf, offset + 4*i);
				if (pageIndex > 0)
				{
					var pageOffset = pageIndex * _pageSize;
					reader.Seek(pageOffset, SeekOrigin.Begin);
					var allocationBuf = new byte[_pageSize];
					reader.Read(allocationBuf, 0, allocationBuf.Length);

					var numBlocks = BitConverter.ToInt32(allocationBuf, 0);
					var offset2 = 4;
					var pagesCount2 = 1023;
					for (int j = 0; j < pagesCount2; j++)
					{
						var dataPageIndex = BitConverter.ToInt32(allocationBuf, offset2 + 4 * j);
						if (dataPageIndex != 0)
						{
							fullPageList.Add(dataPageIndex);
						}
					}
				}

			}
			var pages = fullPageList.ToArray();
			return new AwaFilePageStream(reader, dataSize, _pageSize, pages);
		}

		public virtual AwaFileFormat.RootObjectHeader ReadRootObject(Stream reader)
		{
			var buf = new byte[_pageSize];
			reader.Read(buf, 0, buf.Length);

			var lang = System.Text.Encoding.ASCII.GetString(buf, 0, 8);
			var numBlocks = BitConverter.ToInt32(buf, 8);
			var offset = 12;
			var blocks = new int[numBlocks];
			for (int i = 0; i < numBlocks; i++)
			{
				blocks[i] = BitConverter.ToInt32(buf, offset);
				offset += 4;
			}
			return new AwaFileFormat.RootObjectHeader(lang, blocks);
		}
	}
}
