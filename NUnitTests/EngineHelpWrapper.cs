/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using OneScript.Sources;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;

namespace NUnitTests
{
	public class EngineHelpWrapper : IHostApplication
	{
		public HostedScriptEngine Engine { get; private set; }

		public IValue TestRunner { get; private set; }

		public HostedScriptEngine StartEngine()
		{
			var mainEngine = DefaultEngineBuilder.Create()
				.SetDefaultOptions()
				.SetupEnvironment(envSetup =>
				{
					envSetup.AddAssembly(typeof(v8unpack.File8Reader).Assembly);
					envSetup.AddAssembly(typeof(EngineHelpWrapper).Assembly);
				})
				.Build();
			
			Engine = new HostedScriptEngine(mainEngine);

			var testrunnerSource = LoadFromAssemblyResource("NUnitTests.Tests.testrunner.os");
			var testrunnerModule = Engine.GetCompilerService().Compile(testrunnerSource);

			Engine.EngineInstance.AttachedScriptsFactory.RegisterTypeModule("TestRunner", testrunnerModule);

			TestRunner = NewInstanceOf("TestRunner", Engine.EngineInstance);

			return Engine;
		}

		public void RunTestScript(string resourceName)
		{
			var source = LoadFromAssemblyResource(resourceName);
			var module = Engine.GetCompilerService().Compile(source);
			Engine.EngineInstance.AttachedScriptsFactory.RegisterTypeModule(resourceName, module);

			var test = NewInstanceOf(resourceName, Engine.EngineInstance);
			ArrayImpl testArray;
			{
				int methodIndex = test.FindMethod("ПолучитьСписокТестов");

				{
					IValue ivTests;
					test.CallAsFunction(methodIndex, new IValue[] { TestRunner }, out ivTests);
					testArray = ivTests as ArrayImpl;
				}
			}

			foreach (var ivTestName in testArray)
			{
				string testName = ivTestName.AsString();
				int methodIndex = test.FindMethod(testName);
				if (methodIndex == -1)
				{
					// Тест указан, но процедуры нет или она не экспортирована
					continue;
				}

				test.CallAsProcedure(methodIndex, new IValue[] { });
			}
		}

		public SourceCode LoadFromAssemblyResource(string resourceName)
		{
			var asm = System.Reflection.Assembly.GetExecutingAssembly();
			string codeSource;

			using (Stream s = asm.GetManifestResourceStream(resourceName))
			{
				using (StreamReader r = new StreamReader(s))
				{
					codeSource = r.ReadToEnd();
				}
			}

			return Engine.Loader.FromString(codeSource);
		}

		private UserScriptContextInstance NewInstanceOf(string typeName, ScriptingEngine mainEngine)
		{
			var activationContext = new TypeActivationContext
			{
				TypeName = typeName,
				Services = mainEngine.Services,
				TypeManager = mainEngine.TypeManager
			};
			
			return AttachedScriptsFactory.ScriptFactory(activationContext, Array.Empty<IValue>());
		}

		public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
		{
		}

		public string[] GetCommandLineArguments()
		{
			return new string[] { };
		}

		public bool InputString(out string result, string prompt, int maxLen, bool multiline)
		{
			result = "";
			return false;
		}

		public void ShowExceptionInfo(Exception exc)
		{
		}
	}
}
