using SimAirport.Modding.Base;
using SimAirport.Modding.Settings;
using System;
using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
using UnityEngine;
using SimAirport.Modding.Data;

namespace qcs2017.modrunner {
	public class ScriptMod : BaseMod {
		public override string Name => "ScriptTestMod";

		public override string InternalName => "ScriptModTester.scripttestmod";

		public override string Description => "Script Test Mod";

		public override string Author => "ScriptModTester";

		public override SettingManager SettingManager { get; set; } //will be filled by game

		private double lastgametime = 0;

		private void doWork() {
            Debug.Log("ScriptTestMod doWork");
		}

		public override void OnTick() {
			double gametime = GameTime.Instance.TotalGameSeconds;
			if (lastgametime == 0) 
				lastgametime = gametime;

			Boolean dowork = false;

			//once per in game minute
			if (gametime - lastgametime > 60) {
				lastgametime = gametime;

				dowork = true; 
			}

			if (dowork) {
				doWork();
			}
		}

		public override void OnSettingsLoaded() {
            Debug.Log("ScriptTestMod OnSettingsLoaded");
			SettingManager.AddDefault("like", new SliderSetting {
			Minimum = 1,
			Maximum = 10,
			Name = "How much do you\nlike the modrunner?",
			Value = 10
		});
		}

	}
}
