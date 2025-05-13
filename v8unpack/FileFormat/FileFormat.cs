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
using System.Collections.Generic;

namespace v8unpack
{

	internal static class FileFormat
	{

		public const UInt32 V8_FF_SIGNATURE = 0x7fffffff;
		public const UInt32 V8_DEFAULT_PAGE_SIZE = 512;

		public struct ContainerHeader
		{

			public ContainerHeader(UInt32 NextPageAddr = V8_FF_SIGNATURE,
			                  UInt32 PageSize = V8_DEFAULT_PAGE_SIZE,
			                  UInt32 StorageVer = 0,
			                  UInt32 Reserved = 0)
			{
				this.NextPageAddr = NextPageAddr;
				this.PageSize = PageSize;
				this.StorageVer = 0;
				this.Reserved = 0;
			}

			public readonly UInt32 NextPageAddr;
			public readonly UInt32 PageSize;
			public readonly UInt32 StorageVer;
			public readonly UInt32 Reserved;

			public static ContainerHeader Read(Stream reader)
			{
				const int HeaderSize = 16;
				var buf = new byte[HeaderSize];
				if (reader.Read(buf, 0, HeaderSize) < HeaderSize)
				{
					throw new File8FormatException();
				}

				return new ContainerHeader(
					NextPageAddr: BitConverter.ToUInt32(buf, 0),
					PageSize: BitConverter.ToUInt32(buf, 4),
					StorageVer: BitConverter.ToUInt32(buf, 8),
					Reserved: BitConverter.ToUInt32(buf, 12)
				);
			}
		}

		public struct ElementAddress
		{

			public ElementAddress(UInt32 HeaderAddress, UInt32 DataAddress, UInt32 Signature = V8_FF_SIGNATURE)
			{
				this.HeaderAddress = HeaderAddress;
				this.DataAddress = DataAddress;
				this.Signature = Signature;
			}

			public readonly UInt32 HeaderAddress;
			public readonly UInt32 DataAddress;
			public readonly UInt32 Signature;

			public override string ToString()
			{
				return string.Format("{0:x8}:{1:x8}:{2:x8}", HeaderAddress, DataAddress, Signature);
			}

			public static IList<ElementAddress> Parse(byte[] buf)
			{
				var elemenSize = 4 + 4 + 4;

				var result = new List<ElementAddress>();
				for (var offset = 0; offset + elemenSize <= buf.Length; offset += elemenSize)
				{
					var headerAddress = BitConverter.ToUInt32(buf, offset);
					var dataAddress = BitConverter.ToUInt32(buf, offset + 4);
					var signature = BitConverter.ToUInt32(buf, offset + 8);

					result.Add(new ElementAddress(headerAddress, dataAddress, signature));
				}

				return result;
			}
		}

		public struct BlockHeader
		{
			public readonly UInt32 DataSize;
			public readonly UInt32 PageSize;
			public readonly UInt32 NextPageAddr;

			public BlockHeader(UInt32 DataSize = 0,
			                   UInt32 PageSize = V8_DEFAULT_PAGE_SIZE,
			                   UInt32 NextPageAddr = V8_FF_SIGNATURE)
			{
				this.DataSize = DataSize;
				this.PageSize = PageSize;
				this.NextPageAddr = NextPageAddr;
			}

			private static void ReadExpectedByte(Stream reader, int expectedValue)
			{
				if (reader.ReadByte() != expectedValue)
					throw new File8FormatException();
			}

			private static UInt32 ReadHexData(Stream reader)
			{
				byte[] hex = new byte[8];
				if (reader.Read(hex, 0, 8) < 8)
				{
					throw new File8FormatException();
				}

				try
				{
					return Convert.ToUInt32(System.Text.Encoding.ASCII.GetString(hex), 16);
				}
				catch
				{
					throw new File8FormatException();
				}
			}

			public static BlockHeader Read(Stream reader)
			{
				ReadExpectedByte(reader, 0x0D);
				ReadExpectedByte(reader, 0x0A);

				var dataSize = ReadHexData(reader);
				ReadExpectedByte(reader, 0x20);
				var pageSize = ReadHexData(reader);
				ReadExpectedByte(reader, 0x20);
				var nextPageAddr = ReadHexData(reader);
				ReadExpectedByte(reader, 0x20);

				ReadExpectedByte(reader, 0x0D);
				ReadExpectedByte(reader, 0x0A);

				return new BlockHeader(dataSize, pageSize, nextPageAddr);
			}

		}

		public struct ElementHeader
		{
			public readonly DateTime CreationDate;
			public readonly DateTime ModificationDate;
			public readonly string Name;

			public ElementHeader(string name, DateTime creationDate, DateTime modificationDate)
			{
				this.Name = name;
				this.CreationDate = creationDate;
				this.ModificationDate = modificationDate;
			}

			public static DateTime File8Date(UInt64 serializedDate)
			{
				return new DateTime((long) serializedDate * 1000);
			}

			public static ElementHeader Parse(byte[] buf)
			{
				var serializedCreationDate = BitConverter.ToUInt64(buf, 0);
				var serializedModificationDate = BitConverter.ToUInt64(buf, 8);
				// 4 байта на Reserved
				var enc = new System.Text.UnicodeEncoding(bigEndian: false, byteOrderMark: false);

				var NameOffset = 8 + 8 + 4;
				var name = enc.GetString(buf, NameOffset, buf.Length - NameOffset - 4).TrimEnd('\0');

				var creationDate = File8Date(serializedCreationDate);
				var modificationDate = File8Date(serializedModificationDate);

				return new ElementHeader(name, creationDate, modificationDate);
			}
		}

		public static bool IsContainer(byte[] data)
		{
			var reader = new MemoryStream(data);
			try
			{
				FileFormat.ContainerHeader.Read(reader);
				FileFormat.BlockHeader.Read(reader);
			}
			catch (File8FormatException)
			{
				return false;
			}

			return true;
		}
	}
}
