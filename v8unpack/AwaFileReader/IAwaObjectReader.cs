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
	/// Общий интерфейс для работы с Ава-файлами разных форматов.
	/// </summary>
	public interface IAwaObjectReader
	{
		// AwaFileFormat.ObjectHeader ReadObject(Stream reader);

		Stream OpenObjectStream(Stream reader);
		AwaFileFormat.RootObjectHeader ReadRootObject(Stream reader);
	}
}
