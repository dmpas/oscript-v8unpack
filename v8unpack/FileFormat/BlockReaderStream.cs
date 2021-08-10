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
	2014-2018       dmpas           sergey(dot)batanov(at)dmpas(dot)ru
*/
using System;
using System.IO;

namespace v8unpack
{
	/// <summary>
	/// Поточное чтение данных из блочного восьмофайла.
	/// </summary>
	public class BlockReaderStream : Stream
	{
		private FileFormat.BlockHeader currentHeader;
		private readonly Stream _reader;
		private readonly int _dataSize;

		private byte[] _currentPageData;
		private int _currentPageOffset;
		private bool _isPacked;
		private bool _isContainer;

		public BlockReaderStream(Stream basicStream)
		{
			_reader = basicStream;
			currentHeader = FileFormat.BlockHeader.Read(_reader);
			_dataSize = (int)currentHeader.DataSize;
			ReadPage();
			AnalyzeState();
		}

		private void ReadPage()
		{
			var currentDataSize = Math.Min(_dataSize, (int)currentHeader.PageSize);
			_currentPageData = new byte[currentDataSize];
			_reader.Read(_currentPageData, 0, currentDataSize);
			_currentPageOffset = 0;
		}

		private void AnalyzeState()
		{
			byte[] bufferToCheck = _currentPageData;
			try
			{
				var tmp = Ionic.Zlib.DeflateStream.UncompressBuffer(_currentPageData);
				_isPacked = true;
				bufferToCheck = tmp;
			}
			catch (Ionic.Zlib.ZlibException)
			{
				_isPacked = false;
			}

			_isContainer = FileFormat.IsContainer(bufferToCheck);
		}

		private void MoveNextBlock()
		{
			if (currentHeader.NextPageAddr == FileFormat.V8_FF_SIGNATURE)
			{
				_currentPageData = null;
				return;
			}
			_reader.Seek(currentHeader.NextPageAddr, SeekOrigin.Begin);
			currentHeader = FileFormat.BlockHeader.Read(_reader);
			ReadPage();
		}

		public bool IsPacked => _isPacked;

		public bool IsContainer => _isContainer;

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => _dataSize;

		public override long Position
		{
			get => throw new NotSupportedException();

			set => throw new NotSupportedException();
		}

		public override void Flush()
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
				}

				var readFromCurrentPage = Math.Min(leftInPage, countLeft);

				Buffer.BlockCopy(_currentPageData, _currentPageOffset, buffer, offset, readFromCurrentPage);
				_currentPageOffset += readFromCurrentPage;
				offset += readFromCurrentPage;

				bytesRead += readFromCurrentPage;
				countLeft -= readFromCurrentPage;
			}

			return bytesRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Считывает текущий блок данных из восьмофайла.
		/// </summary>
		/// <returns>Считанные данные.</returns>
		/// <param name="reader">Входящий поток.</param>
		public static byte[] ReadDataBlock(Stream reader)
		{
			var blockReader = new BlockReaderStream(reader);
			var buf = new byte[blockReader.Length];
			blockReader.Read(buf, 0, buf.Length);
			return buf;
		}
	}
}
