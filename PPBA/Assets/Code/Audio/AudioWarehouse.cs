using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public enum ClipsPawn
	{
		DEFAULT,
		GO_TO_COVER_01,             //KI-Einheit sucht sicht Deckung
		VOICE_SCREAM_DEATH_01,      //Todesschrei
		BUILD_REPAIR_01,            //Objekt wird von KI-Einheit gebaut oder repariert. Sollte loopbar sein, da längerer Prozess. Evtl. nur hörbar, wenn man sehr nach ranzoomt
		DECONSTRUCT_01,             //Objekt wird von KI-Einheit abgerissen. Sollte loopbar sein, da längerer Prozess. Evtl. nur hörbar, wenn man sehr nach ranzoomt
		GO_TO_BORDER_01,            //KI-Einheit rückt zur Grenze vor
		SHOT_MP_01,                 //Sound, wenn die Einheiten mit ihrer MP schießen
		VOICE_ATTACK_01,            //Töne und Textabschnitte, die die Einheiten sprechen, wenn sie angreifen
		VOICE_BRINGSUPPLIES_01,     //Einheit bringt Vorräte von A nach B.
		VOICE_BUILD_ONIT_01,        //Einheit führt einen beaufragten Bau eines Gebäudes oder Objektes durch.
		VOICE_CLICK_UNIT_01,        //Sound / Voice, wenn KI-Einheit angewählt wird (z. B. Spruch oder Bestätigung)
		VOICE_CONQUERBUILDING_01,   //KI-Einheit übernimmt das Gebäude oder Objekt des gegnerischen Teams
		VOICE_DECONSTRUCT_01,       //KI-Einheit reißt ein Objekt/Gebäude ab, das nicht benötigt wird.
		VOICE_FOLLOW_01,            //KI-Einheit folgt einer anderen Einheit bzw. läuft ihr nach.
		VOICE_GETSUPPLIES_01,       //KI-Einheit holt Vorrats-Nachschub
		VOICE_GOTOFLAG_IDLE_01,     //Einheit begibt sich zur Flagge, die der Spieler gesetzt hat.
		VOICE_GOTOHEAL_01,          //KI-Einheit hat wenig Lebenspunkte und geht zu einem Lazarett
		VOICE_MOUNT_BUILDING_01,    //Einheit begibt sich zu z. B. einem Geschützturm, um ihn zu benutzen
		VOICE_SCREAM_FLEE_01,       //Angstschrei oder Spruch beim fliehen
		VOICE_SCREAM_INPAIN_01,     //wenn KI-Einheit Schaden zugefügt bekommt
		VOICE_STAYINCOVER_01,       //Einheit bleibt bei starkem Beschuss weiterhin in Deckung
		VOICE_UNIT_IDLE_BEHAVIOR_01,//Verhalten der KI-Einheiten, wenn sie gerade nicht zu tun haben und an einer Stelle stehen.
		VOICE_WIN_CHEER_01,         //Verhalten der KI-Einheiten, wenn ihr Team das Spiel gewonnen hat.
	}

	public enum ClipsBuilding
	{
		DEFAULT,
		ERROR_PLACE_BUILDING_01,    //Objekt kann hier nicht platziert werden
		CLICK_AMMOFACTORY_IDLE_01,  //Munitionsfabrik Sound, wenn ausgewählt
		CLICK_DEPOT_IDLE_01,        //Depot Sound, wenn ausgewählt
		CLICK_HQ_IDLE_01,           //HQ Sound, wenn ausgewählt
		CLICK_MEDTEND_IDLE_01,      //Lazarett Sound, wenn ausgewählt
		CLICK_REFINERY_IDLE_01,     //Raffinerie Sound, wenn ausgewählt
		CLICK_ROCKWALL_IDLE_01,     //Steinwand Sound, wenn ausgewählt
		CLICK_STREET_IDLE_01,       //Straßen Sound, wenn ausgewählt
		CLICK_TRASHWALL_IDLE_01,    //trashwall Sound, wenn ausgewählt
		DESTROY_BUILDING_01,        //Sound abspielen, wenn das Gebäude vollständig zerstört wurde und vom Spielfeld verschwindet.
		PLACE_BUILDING_01,          //Objekt wird platziert (Feedback)
		PLACE_FLAG_01,              //Flaggen Sound, wenn platziert
		UNIT_PRODUCED_01,           //Sound, wenn Einheiten produziert wurden
	}

	public enum ClipsUI
	{
		DEFAULT,
		BUTTON_BACK_01,             //Dieser Button sollte einen unterscheidbaren Sound ggü. den anderen Buttons haben
		BUTTON_CLICK_01,            //wenn mit Maus der Button geklickt wird
		BUTTON_CLOSE_BACK_01,   //anderen Sound als bei restlichen Buttons ?
		BUTTON_HOVER_01,        //Wenn man mit der Maus über einen Button fährt. 
		BUTTON_PING_01,         //wenn etwas auf der Minimap durch einen Ping angezeigt wird, leichten "Klingel"-Sound ???
		BUTTON_SELECTMINIMAP_01,//Dieser Button sollte einen unterscheidbaren Sound ggü. den anderen Buttons haben
		ICON_CLICK_01,          //Sound, wenn ein Button im User Interface geklickt wird
		ICON_HOVER_01,          //wenn man mit der Maus über einen Button im User Interface fährt 

	}

	public enum ClipsMusic
	{
		DEFAULT,
		MUSIC_TRACK_01,
		MUSIC_ENDSCREEN_LOSE_01,
		MUSIC_ENDSCREEN_WIN_01,
	}

	public enum ClipsEnvironment
	{
		DEFAULT,
		ENVIRONMENT_RIVER_01,           //Wasser (z. B. Meer-Strömung, plätschern)
		ENVIRONMENT_BIRDS_01,           //Vögel die zum Schnee Setting passen (Eulen, Krähen, Raben)
		ENVIRONMENT_ICE_CRACKING_01,    //Gletscher und zugefrorener Fluss (Eis knacken, da das Eis in Bewegung)
		ENVIRONMENT_LIGHT_SNOWSTORM_01, // Schneesturm/Wind
		ENVIRONMENT_WAR_01,             //
		EXPLOSION_01,                   //Explosion, wenn z. B. eine Granate trifft
	}

	public class AudioWarehouse : Singleton<AudioWarehouse>
	{
		#region Name Arrays
		[HideInInspector]
		public static ClipsPawn[] _namesPawn = new ClipsPawn[] {
			ClipsPawn.DEFAULT,
			ClipsPawn.GO_TO_COVER_01,
			ClipsPawn.VOICE_SCREAM_DEATH_01,
			ClipsPawn.BUILD_REPAIR_01,
			ClipsPawn.DECONSTRUCT_01,
			ClipsPawn.GO_TO_BORDER_01,
			ClipsPawn.SHOT_MP_01,
			ClipsPawn.VOICE_ATTACK_01,
			ClipsPawn.VOICE_BRINGSUPPLIES_01,
			ClipsPawn.VOICE_BUILD_ONIT_01,
			ClipsPawn.VOICE_CLICK_UNIT_01,
			ClipsPawn.VOICE_CONQUERBUILDING_01,
			ClipsPawn.VOICE_DECONSTRUCT_01,
			ClipsPawn.VOICE_FOLLOW_01,
			ClipsPawn.VOICE_GETSUPPLIES_01,
			ClipsPawn.VOICE_GOTOFLAG_IDLE_01,
			ClipsPawn.VOICE_GOTOHEAL_01,
			ClipsPawn.VOICE_MOUNT_BUILDING_01,
			ClipsPawn.VOICE_SCREAM_FLEE_01,
			ClipsPawn.VOICE_SCREAM_INPAIN_01,
			ClipsPawn.VOICE_STAYINCOVER_01,
			ClipsPawn.VOICE_UNIT_IDLE_BEHAVIOR_01,
			ClipsPawn.VOICE_WIN_CHEER_01,
		};

		[HideInInspector]
		public static ClipsBuilding[] _namesBuilding = new ClipsBuilding[] {
			ClipsBuilding.DEFAULT,
			ClipsBuilding.ERROR_PLACE_BUILDING_01,
			ClipsBuilding.CLICK_AMMOFACTORY_IDLE_01,
			ClipsBuilding.CLICK_DEPOT_IDLE_01,
			ClipsBuilding.CLICK_HQ_IDLE_01,
			ClipsBuilding.CLICK_MEDTEND_IDLE_01,
			ClipsBuilding.CLICK_REFINERY_IDLE_01,
			ClipsBuilding.CLICK_ROCKWALL_IDLE_01,
			ClipsBuilding.CLICK_STREET_IDLE_01,
			ClipsBuilding.CLICK_TRASHWALL_IDLE_01,
			ClipsBuilding.DESTROY_BUILDING_01,
			ClipsBuilding.PLACE_BUILDING_01,
			ClipsBuilding.PLACE_FLAG_01,
			ClipsBuilding.UNIT_PRODUCED_01,
		};

		[HideInInspector]
		public static ClipsUI[] _namesUI = new ClipsUI[] {
			ClipsUI.DEFAULT,
			ClipsUI.BUTTON_BACK_01,
			ClipsUI.BUTTON_CLICK_01,
			ClipsUI.BUTTON_CLOSE_BACK_01,
			ClipsUI.BUTTON_HOVER_01,
			ClipsUI.BUTTON_PING_01,
			ClipsUI.BUTTON_SELECTMINIMAP_01,
			ClipsUI.ICON_CLICK_01,
			ClipsUI.ICON_HOVER_01,
		};

		[HideInInspector]
		public static ClipsMusic[] _namesMusic = new ClipsMusic[] {
			ClipsMusic.DEFAULT,
			ClipsMusic.MUSIC_TRACK_01,
			ClipsMusic.MUSIC_ENDSCREEN_LOSE_01,
			ClipsMusic.MUSIC_ENDSCREEN_WIN_01,
		};

		[HideInInspector]
		public static ClipsEnvironment[] _namesEnvironment = new ClipsEnvironment[] {
			ClipsEnvironment.DEFAULT,
			ClipsEnvironment.ENVIRONMENT_RIVER_01,
			ClipsEnvironment.ENVIRONMENT_BIRDS_01,
			ClipsEnvironment.ENVIRONMENT_ICE_CRACKING_01,
			ClipsEnvironment.ENVIRONMENT_LIGHT_SNOWSTORM_01,
			ClipsEnvironment.ENVIRONMENT_WAR_01,
			ClipsEnvironment.EXPLOSION_01,
		};
		#endregion

		public static Dictionary<ClipsPawn, AudioClip> _pawnClipDict = new Dictionary<ClipsPawn, AudioClip>();
		public static Dictionary<ClipsBuilding, AudioClip> _buildingClipDict = new Dictionary<ClipsBuilding, AudioClip>();
		public static Dictionary<ClipsUI, AudioClip> _uiClipDict = new Dictionary<ClipsUI, AudioClip>();
		public static Dictionary<ClipsMusic, AudioClip> _musicClipDict = new Dictionary<ClipsMusic, AudioClip>();
		public static Dictionary<ClipsEnvironment, AudioClip> _environmentClipDict = new Dictionary<ClipsEnvironment, AudioClip>();

		public static AudioClip _defaultClip;

		void Awake()
		{
			LoadAll();

			//DontDestroyOnLoad(this.transform.parent);
		}

		void Start()
		{

		}

		public AudioClip Clip(ClipsBuilding effect) => _buildingClipDict.ContainsKey(effect) ? _buildingClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsUI effect) => _uiClipDict.ContainsKey(effect) ? _uiClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsMusic effect) => _musicClipDict.ContainsKey(effect) ? _musicClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsPawn effect) => _pawnClipDict.ContainsKey(effect) ? _pawnClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsEnvironment effect) => _environmentClipDict.ContainsKey(effect) ? _environmentClipDict[effect] : _defaultClip;

		public void LoadAll()
		{
			foreach(ClipsPawn item in _namesPawn)
			{
				string path = "ClipsPawn/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_pawnClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsBuilding item in _namesPawn)
			{
				string path = "ClipsBuilding/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_buildingClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsUI item in _namesPawn)
			{
				string path = "ClipsUI/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_uiClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsMusic item in _namesPawn)
			{
				string path = "ClipsMusic/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_musicClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsEnvironment item in _namesEnvironment)
			{
				string path = "ClipsEnvironment/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_environmentClipDict[item] = null != temp ? temp : _defaultClip;
			}
		}
	}
}
