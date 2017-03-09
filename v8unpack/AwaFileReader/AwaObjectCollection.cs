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
using System.Collections.Generic;
using System.Collections;

namespace v8unpack
{
	/// <summary>
	/// Чтение файла формата Ава. (http://infostart.ru/public/19734/)
	/// </summary>
	[ContextClass("КоллекцияОбъектовАваФайла")]
	public class AwaObjectCollection : AutoContext<AwaObjectCollection>, ICollectionContext, IEnumerable<AwaObject>
	{
		private readonly List<AwaObject> _data = new List<AwaObject>();

		public AwaObjectCollection(IEnumerable<AwaObject> data)
		{
			_data.AddRange(data);
		}

		/// <summary>
		/// Количество элементов в коллекции.
		/// </summary>
		/// <returns>Количество элементов в коллекции.</returns>
		[ContextMethod("Количество")]
		public int Count()
		{
			return _data.Count;
		}

		public IEnumerator<AwaObject> GetEnumerator()
		{
			return ((IEnumerable<AwaObject>)_data).GetEnumerator();
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
			return ((IEnumerable<AwaObject>)_data).GetEnumerator();
		}
	}
}
