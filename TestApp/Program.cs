/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.StandardLibrary;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Hosting;

namespace TestApp
{
	class MainClass : IHostApplication
	{

		static readonly string SCRIPT = @"// Отладочный скрипт
// в котором уже подключена наша компонента

Если АргументыКоманднойСтроки.Количество() = 0 Тогда
	ИмяФайла = ""test.dat"";
Иначе
	ИмяФайла = АргументыКоманднойСтроки[0];
КонецЕсли;

ЧтениеФайла = Новый ЧтениеФайла8(ИмяФайла);
Для Каждого мЭлемент Из ЧтениеФайла.Элементы Цикл
	Сообщить(мЭлемент.Имя + "":"" + мЭлемент.ВремяИзменения + "":"" + мЭлемент.ВремяСоздания);
	ЧтениеФайла.Извлечь(мЭлемент, ""test"", Истина);
КонецЦикла;
"
			;

		public static HostedScriptEngine StartEngine()
		{
			var mainEngine = DefaultEngineBuilder.Create()
				.SetDefaultOptions()
				.SetupEnvironment(envSetup =>
				{
					envSetup.AddAssembly(typeof(v8unpack.File8Reader).Assembly);
				})
				.Build();
			var engine = new HostedScriptEngine(mainEngine);
			engine.Initialize();

			return engine;
		}

		public static void Main(string[] args)
		{
			var engine = StartEngine();
			var script = engine.Loader.FromString(SCRIPT);
			var process = engine.CreateProcess(new MainClass(args), script);

			var result = process.Start();

			Console.WriteLine("Result = {0}", result);
		}

		private string[] args;

		public MainClass(string[] args)
		{
			this.args = args;
		}

		public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
		{
			Console.WriteLine(str);
		}

		public void ShowExceptionInfo(Exception exc)
		{
			Console.WriteLine(exc.ToString());
		}

		public bool InputString(out string result, string prompt, int maxLen, bool multiline)
		{
			throw new NotImplementedException();
		}

		public bool InputString(out string result, int maxLen)
		{
			throw new NotSupportedException();
		}

		public string[] GetCommandLineArguments()
		{
			return args;
		}
	}
}
