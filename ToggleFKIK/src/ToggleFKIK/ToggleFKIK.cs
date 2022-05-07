using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Studio;
using Studio;
using UnityEngine;


namespace LSP_Plugins
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class ToggleFKIK : BaseUnityPlugin
    {
        public const string PluginName = "ToggleFKIK";
        public const string GUID = "certain.lsp.togglefkik";
        public const string Version = "1.0.0";

        internal static new ManualLogSource Logger;
        public static ToggleFKIK Instance;

        // Config
        public static ConfigEntry<KeyboardShortcut> toggleFKIKHotKey { get; private set; }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            toggleFKIKHotKey = Config.Bind("Keyboard Shortcuts", "Toggle Sync FK & AK", new KeyboardShortcut(KeyCode.M), "Copy FK to IK, or vice versa. Sync with bone inplace when using FK&IK.");
        }

        protected void Update()
        {
            if (toggleFKIKHotKey.Value.IsDown())
                {
                    foreach (OCIChar selectedChar in StudioAPI.GetSelectedCharacters()) {
                        ToggleKeys(selectedChar);
                    }
                }


        }

        internal static void ToggleKeys(OCIChar selectedChar) 
        {
            Logger.LogInfo(string.Format("Toggle FK and IK for: {0}", selectedChar?.charInfo.name));

            // Save current neck mode
            var origPtn = selectedChar.neckLookCtrl.ptnNo;

            // IK enabled, copy to FK
            if (selectedChar.finalIK.enabled && ! selectedChar.fkCtrl.enabled)
            {
                // Copy current bones to FK
                foreach (OIBoneInfo.BoneGroup target in FKCtrl.parts) {
                    selectedChar.fkCtrl.CopyBone(target);
                }
                // Enable all FK              
                for (int i = 0; i < FKCtrl.parts.Length; i++)
                {
                    selectedChar.oiCharInfo.activeFK[i] = true;
                }                    
                selectedChar.ActiveKinematicMode(OICharInfo.KinematicMode.FK, true, true);

            }
            
            // FK enabled, copy to IK
            else if (!selectedChar.finalIK.enabled && selectedChar.fkCtrl.enabled)
            {
                // Then copy bones to to IK
                foreach (OCIChar.IKInfo ikinfo in selectedChar.listIKTarget)
                {
                    ikinfo.CopyBone();
                }
                // Then enable Ik
                for (int i = 0; i < 5; i++)
                {
                    selectedChar.oiCharInfo.activeIK[i] = true;
                }
                selectedChar.ActiveKinematicMode(OICharInfo.KinematicMode.IK, true, true);
            }

            // Might be using FK&IK, just sync the position from bones
            else {
                // Copy current bones to FK
                foreach (OIBoneInfo.BoneGroup target in FKCtrl.parts) {
                    selectedChar.fkCtrl.CopyBone(target);
                }
                // Then copy bones to to IK
                foreach (OCIChar.IKInfo ikinfo in selectedChar.listIKTarget)
                {
                    ikinfo.CopyBone();
                }
            }

            //Finally restore the original neck look pattern which will have been overwritten
            if (origPtn != selectedChar.neckLookCtrl.ptnNo)
                selectedChar.ChangeLookNeckPtn(origPtn);
        }
    }
}
