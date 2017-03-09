/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;

namespace v8unpack
{
	internal class Awa_8_1_0_ObjectReader : Awa_8_0_5_ObjectReader
	{
		private readonly int _pageSize = 4096;
		public override AwaFileFormat.RootObjectHeader ReadRootObject(Stream reader)
		{
			var buf = new byte[_pageSize];
			reader.Read(buf, 0, buf.Length);

			var lang = System.Text.Encoding.ASCII.GetString(buf, 0, 32);
			var numBlocks = BitConverter.ToInt32(buf, 32);
			var offset = 36;
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
