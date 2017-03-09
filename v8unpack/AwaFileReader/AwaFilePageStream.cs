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
	public class AwaFilePageStream : Stream
	{

		private readonly Stream _reader;
		private readonly int _pageSize;
		private readonly int[] _pages;
		private readonly long _dataSize;

		private long _dataLeft;
		private int _currentPageIndex = 0;
		private int _currentPageOffset = 0;
		private byte[] _currentPageData;
		private long _position = 0;

		public AwaFilePageStream(Stream reader, long dataSize, int pageSize, int[] pages)
		{
			_reader = reader;
			_pageSize = pageSize;
			_pages = pages;
			_dataSize = dataSize;
			_dataLeft = dataSize;

			_currentPageIndex = -1;
			MoveNextBlock();
		}

		private void MoveNextBlock()
		{
			if (_currentPageIndex + 1 >= _pages.Length)
			{
				_currentPageData = null;
				_currentPageOffset = 0;
				return;
			}
			++_currentPageIndex;

			var fileOffset = _pages[_currentPageIndex] * _pageSize;
			_reader.Seek(fileOffset, SeekOrigin.Begin);

			var _currentDataSize = Math.Min(_pageSize, _dataLeft);
			_currentPageData = new byte[_currentDataSize];
			_reader.Read(_currentPageData, 0, _currentPageData.Length);
			_currentPageOffset = 0;

			_dataLeft -= _currentDataSize;
		}

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				return _dataSize;
			}
		}

		public override long Position
		{
			get
			{
				return _position;
			}

			set
			{
				throw new NotSupportedException();
			}
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_currentPageData == null)
			{
				return 0;
			}
			int bytesRead = 0;
			int countLeft = count;

			while (countLeft > 0)
			{
				var leftInPage = _currentPageData.Length - _currentPageOffset;
				if (leftInPage == 0)
				{
					MoveNextBlock();
					if (_currentPageData == null)
					{
						break;
					}
					leftInPage = _currentPageData.Length - _currentPageOffset;
				}

				var readFromCurrentPage = Math.Min(leftInPage, countLeft);

				Buffer.BlockCopy(_currentPageData, _currentPageOffset, buffer, offset, readFromCurrentPage);
				_currentPageOffset += readFromCurrentPage;
				offset += readFromCurrentPage;

				bytesRead += readFromCurrentPage;
				countLeft -= readFromCurrentPage;
			}
			_position += bytesRead;

			return bytesRead;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}
