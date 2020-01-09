using SimAirport.Modding.Base;
using SimAirport.Modding.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
// using System.Text;
// using System.Threading.Tasks;
using UnityEngine;
using SimAirport.Modding.Data;
using System.CodeDom.Compiler;
// using System.Collections.Generic;
using Microsoft.CSharp;
using System.Reflection;

namespace qcs2017.modrunner {
	public class ScriptRunnerMod : BaseMod {
		private BaseMod mod;

		public override string Name {
			get { return mod==null?"N/A":mod.Name + " (powered by qcs2017.modrunner)";}
		}

		public override string InternalName  {
			get { return mod==null?"N/A":mod.InternalName;}
		}

		public override string Description {
			get { return mod==null?"N/A":mod.Description + "\n\n* powered by qcs2017.modrunner";}
		}

		public override string Author {
			get { return mod==null?"N/A":mod.Author;}
		}

		private SettingManager settings = null;

		public override SettingManager SettingManager { 
			get { return mod?.SettingManager;}
			set { settings = value;}
		 } //will be filled by game

		public override void OnTick() {
			mod?.OnTick();
		}

		public override void OnLoad(SimAirport.Modding.Data.GameState state) {
			Debug.Log("ModRunner OnLoad");
			mod = findMod();
			if (mod != null) {
				mod.SettingManager = settings;
				OnSettingsLoaded();
				mod.OnLoad(state);
			}
		}

		public override void OnAirportLoaded(Dictionary<string, object> saveData) {
			mod?.OnAirportLoaded(saveData);			
		}

		public override Dictionary<string, object> OnAirportSaving() {
			return mod?.OnAirportSaving();
		}

		public override void OnSettingsLoaded() {
			mod?.OnSettingsLoaded();
		}

		public override void OnDisabled() {
			mod.OnDisabled();
		}

		private BaseMod findMod() {
			//find location of Mod.cs file
			//the scripting dir is completely loaded as assembly, so can't use it,
			//use modrunner dir instead
			string cslocation = Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).Location)+Path.DirectorySeparatorChar+".."+Path.DirectorySeparatorChar+"modrunner"+Path.DirectorySeparatorChar+"Mod.cs";
Debug.Log(1);
			//find SimAirport.Modding.Base assembly
			string locModding=Assembly.GetAssembly(this.GetType().BaseType).Location;
Debug.Log(2);
			//find the ref Type we can use to find the Assembly-CSharp assembly
			//depending on whether we are in the game or menu
			//this could probably be done way better
			Type refType;
			if (MainMenu_UI.instance != null)
				refType = MainMenu_UI.instance.GetType();
			else 
				refType = Game.current.GetType();
			
			//find Assembly-CSharp assembly
			String locAssembly = Assembly.GetAssembly(refType).Location;

			//find UnityEngine.CoreModule assembly
			String locUnity = Assembly.GetAssembly(refType.BaseType).Location;
Debug.Log(3);

			//add all of the assemblies to a Set for the compiler
			HashSet<string> assemblies = new HashSet<string>();
			assemblies.Add(locModding);
			assemblies.Add(locAssembly);
			//find all dependencies/references of main assembly
            AssemblyName[] assemblyNames = Assembly.GetAssembly(refType).GetReferencedAssemblies();
			foreach(AssemblyName assname in assemblyNames) {
				//system seems to reference the mscorlib already, so do not add it again
				if (assname.Name != "mscorlib") {
					string loc = Assembly.Load(assname.Name).Location;
					assemblies.Add(loc);
				}
			}
Debug.Log(4);

			//prepare in-memory assembly compiler...
			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.7.1" } });
			//...add the references...
			CompilerParameters parameters = new CompilerParameters(assemblies.ToArray());
			parameters.GenerateInMemory = true;
	        parameters.GenerateExecutable = false;
			parameters.TreatWarningsAsErrors = false;
			//...and compile --- here you could also provide multiple source files, it's an array ---
            CompilerResults compilerResults = provider.CompileAssemblyFromFile(parameters, new[]{cslocation});
			compilerResults.Errors.Cast<CompilerError>().ToList().ForEach(error => Debug.Log("Error from compilation: " + error.ErrorText));
			//CompiledAssembly is either null (then the errors above are interesting), or a real assembly (then you can just treat the errors as warnings)
        	var asm = compilerResults.CompiledAssembly;
			if (asm == null) {
				Debug.Log("Generated assembly is null");
			} else {
				//TODO probably should search for all types which extend BaseMod
				Type t = asm.GetType("qcs2017.modrunner.ScriptMod");
				mod = (BaseMod)Activator.CreateInstance(t);
				return mod;
			}

			return null;
		}
	}
}