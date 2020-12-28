using Ganymed.Monitoring.Enumerations;
using Ganymed.Monitoring.Interfaces;
using Ganymed.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ganymed.Monitoring.Core
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    [ExecuteInEditMode]
    public class MonitoringElement : MonoBehaviour
    {
        #region --- MEMBER ---

        private TextMeshProUGUI textMesh = null;
        private VerticalLayoutGroup layoutGroup = null;
        private Image background = null;
        private Module module;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        public static MonitoringElement CreateComponent(GameObject where, Module module)
        {
            var target = where.AddComponent<MonitoringElement>();
            target.GetComponents();
            target.SetText(module.GetState(ValueInterpretationOption.DefaultValue), InvokeOrigin.Constructor);
            target.module = module;
            
            module.AddOnValueChangedListener(OnValueChangedContext.Initialization, target.ModuleEventInitialization);
            module.AddOnValueChangedListener(OnValueChangedContext.Update, target.ModuleEventUpdate);
            module.AddOnValueChangedListener(OnValueChangedContext.Show, target.ModuleEventShow);
            module.AddOnValueChangedListener(OnValueChangedContext.Hide, target.ModuleEventHide);
            
            module.AddOnGUIChangedListener(target.ModuleGUIEvent);

            return target;
        }
        
        private void OnApplicationQuit()
        {
            if(module == null) return;
            module.RemoveOnValueChangedListener(OnValueChangedContext.Initialization);
            module.RemoveOnValueChangedListener(OnValueChangedContext.Update);
            module.RemoveOnValueChangedListener(OnValueChangedContext.Show);
            module.RemoveOnValueChangedListener(OnValueChangedContext.Hide);
            
            module.RemoveOnGUIChangedListener(ModuleGUIEvent);
        }

        //--------------------------------------------------------------------------------------------------------------

        #region --- MODULE GUI EVENT ----

        private void ModuleGUIEvent(Configuration.Configurable elementConfigurable, string state, InvokeOrigin invokeOrigin)
        {
            if(elementConfigurable == null) return;
            
            SetText(state,invokeOrigin);
            SetMargins(elementConfigurable.Margins, invokeOrigin);
            SetColor(elementConfigurable.ColorBackground, invokeOrigin);
            SetFontSize(elementConfigurable.FontSize, invokeOrigin);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- MODULE EVENTS ---
    
        
        private void ModuleEventInitialization (IModuleUpdateData data)
        {
            SetText(data.State, InvokeOrigin.Initialization);
        }

        private void ModuleEventShow(IModuleUpdateData data)
        {
            if(textMesh != null)
                textMesh.enabled = true;
        }

        private void ModuleEventHide(IModuleUpdateData data)
        {
            if(textMesh != null)
                textMesh.enabled = false;
        }

        private void ModuleEventUpdate(IModuleUpdateData data)
        {
            SetText(data.State, InvokeOrigin.Constructor);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        private void GetComponents()
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>();
            layoutGroup = GetComponentInChildren<VerticalLayoutGroup>();
            background = GetComponentInChildren<Image>();
        }

        //--------------------------------------------------------------------------------------------------------------

        #region --- INSTANCE SETTER ---

        private void SetText(string text, InvokeOrigin source)
        {
            if(textMesh != null)
                textMesh.text = text;
        }
        
        private void SetColor(Color color, InvokeOrigin source)
        {
            if(background != null)
                background.color = color;
        }

        private void SetFontSize(int size, InvokeOrigin source)
        {
            if(textMesh != null)
                textMesh.fontSize = Mathf.Clamp(size, Configuration.Configurable.MINFONTSIZE, Configuration.Configurable.MAXFONTSIZE);
        }

        private void SetMargins(Vector4 margin, InvokeOrigin source)
        {
            if(textMesh == null)
                return;
            
            textMesh.margin = margin;
            
            /*
             * Do not remove the following lines!
             * If removed the margin of the TMP will cease to properly update 
             */
            textMesh.fontSize++;
            textMesh.fontSize--;
            
            // Only force update canvas if application is not in playmode to prevent it being send by message (Awake/Start)
            if(source != InvokeOrigin.UnityMessage)
                Canvas.ForceUpdateCanvases();
        }

        #endregion
    }
}
