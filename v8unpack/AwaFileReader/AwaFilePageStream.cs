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
	/// Чтение страничного Ава-файла.
	/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="T:v8unpack.AwaFilePageStream"/> class.
		/// </summary>
		/// <param name="reader">Поток чтения данных.</param>
		/// <param name="dataSize">Размер данных в байтах.</param>
		/// <param name="pageSize">Размер страницы в байтах.</param>
		/// <param name="pages">Массив номеров страниц.</param>
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

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:v8unpack.AwaFilePageStream"/> can read.
		/// </summary>
		/// <value><c>true</c> if can read; otherwise, <c>false</c>.</value>
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:v8unpack.AwaFilePageStream"/> can seek.
		/// </summary>
		/// <value><c>true</c> if can seek; otherwise, <c>false</c>.</value>
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:v8unpack.AwaFilePageStream"/> can write.
		/// </summary>
		/// <value><c>true</c> if can write; otherwise, <c>false</c>.</value>
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the length.
		/// </summary>
		/// <value>The length.</value>
		public override long Length
		{
			get
			{
				return _dataSize;
			}
		}

		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>The position.</value>
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

		/// <summary>
		/// Flush this instance.
		/// </summary>
		public override void Flush()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Seek the specified offset and origin.
		/// </summary>
		/// <returns>The seek.</returns>
		/// <param name="offset">Offset.</param>
		/// <param name="origin">Origin.</param>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Sets the length.
		/// </summary>
		/// <param name="value">Value.</param>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Read the specified buffer, offset and count.
		/// </summary>
		/// <returns>The read.</returns>
		/// <param name="buffer">Buffer.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
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

		/// <summary>
		/// Write the specified buffer, offset and count.
		/// </summary>
		/// <returns>The write.</returns>
		/// <param name="buffer">Buffer.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}
