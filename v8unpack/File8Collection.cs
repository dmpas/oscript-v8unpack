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
	2014-2025       dmpas           sergey(dot)batanov(at)dmpas(dot)ru
*/
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace v8unpack
{
	/// <summary>
	/// Коллекция восьмофайлов
	/// </summary>
	[ContextClass("КоллекцияФайловФормата8")]
	public class File8Collection : AutoContext<File8Collection>, ICollectionContext<File8>
	{

		private readonly IReadOnlyList<File8> _data;

		public File8Collection(IEnumerable<File8> data)
		{
			var fileList = new List<File8>();
			fileList.AddRange(data);
			_data = fileList;
		}

		/// <summary>
		/// Возвращает количество элементов в коллекции.
		/// </summary>
		[ContextMethod("Количество")]
		public int Count()
		{
			return _data.Count;
		}

		public int Count(IBslProcess process)
		{
			return Count();
		}

        /// <summary>
        /// Получает файл по имени или индексу.
        /// </summary>
        /// <param name="index">Номер или имя.</param>
        [ContextMethod("Получить")]
		public File8 Get(IValue index)
		{
			if (index.SystemType == BasicTypes.Number)
			{
				return Get((int)index.AsNumber());
			}

			if (index.SystemType == BasicTypes.String)
			{
				return Get(index.ExplicitString());
			}

			throw RuntimeException.InvalidArgumentType(nameof(index));
		}

		public File8 Get(int index)
		{
			return _data[index];
		}

		public File8 Get(string name)
		{
			return _data.First((f) => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Находит элемент по имени или Неопределено, если не найден.
		/// </summary>
		/// <param name="name">Имя файла в контейнере.</param>
		[ContextMethod("Найти")]
		public File8 Find(string name)
		{
			return _data.FirstOrDefault((f) => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		public IEnumerator<File8> GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		private IEnumerator<IValue> GetEngineEnumerator()
		{
			foreach (var value in _data)
			{
				yield return value;
			}
		}

		public CollectionEnumerator GetManagedIterator()
		{
			return new CollectionEnumerator(GetEngineEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _data.GetEnumerator();
		}
	}
}
