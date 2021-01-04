﻿using System;
using System.Threading.Tasks;
using Ganymed.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ganymed.Monitoring.Core
{
    [RequireComponent(typeof(ContentSizeFitter))]
    [ExecuteInEditMode]
    public class ModuleCanvasElement : MonoBehaviour
    {
        #region --- [INSPECTOR] ---

        [SerializeField] private TextMeshProUGUI moduleText = null;
        [SerializeField] private Image background = null;        
        [Header("Scale")]
        [SerializeField] private RectTransform scaleTarget = null;
        [SerializeField] private RectTransform scaleRoot = null;
        [SerializeField] private TextMeshProUGUI sizeDisplayTextField = null;

        [Header("Animations")]
        [SerializeField] private Animator animator = null;
        [SerializeField] private GameObject header = null;
        [SerializeField] private GameObject reactivationButton = null;
        [SerializeField] private HorizontalOrVerticalLayoutGroup layoutGroup = null;
        
        #endregion
        
        #region --- [FIELDS] ---

        private Module module;
        private float scale = 1;
        private static readonly int DecInst = Animator.StringToHash("DecInst");
        private const float Increment = .1f;
        private const float MaxScale = 2f;
        private const float MinScale = .5f;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        
        public void IncrementScale()
        {
            layoutGroup.childControlWidth = false;
            scale = Mathf.Clamp(scale += Increment, MinScale, MaxScale);
            scaleTarget.localScale = new Vector3(scale,scale,scale);
            LayoutRebuilder.ForceRebuildLayoutImmediate(scaleRoot);
            sizeDisplayTextField.text = $"{scale * 100:000}%";
        }

        public void DecrementScale()
        {
            layoutGroup.childControlWidth = false;
            scale = Mathf.Clamp(scale -= Increment, MinScale, MaxScale);
            scaleTarget.localScale = new Vector3(scale,scale,scale);
            LayoutRebuilder.ForceRebuildLayoutImmediate(scaleRoot);
            sizeDisplayTextField.text = $"{scale * 100:000}%";
        }

        public void DeactivateModule()
        {
            layoutGroup.childControlWidth = true;
            module.SetActive(false);
        }

        public void ActivateModule()
        {
            module.SetActive(true);
            scale = 1;
            scaleTarget.localScale = new Vector3(scale,scale,scale);
            LayoutRebuilder.ForceRebuildLayoutImmediate(scaleRoot);
            sizeDisplayTextField.text = $"{scale * 100:000}%";
        }
        
        private async void ModuleActiveAndEnabledStateChanged(bool active, bool enable)
        {
            if (this == null || header == null)
            {
                module.OnActiveAndEnabledStateChanged -= ModuleActiveAndEnabledStateChanged;
                return;
            }

            await Task.Run(delegate { });
            
            if (active && enable)
            {
                header.SetActive(true);
                reactivationButton.SetActive(false);
                moduleText.enabled = true;
            }
            
            else if(!active && enable)
            {
                if(Application.isPlaying)
                    animator.SetTrigger(DecInst);
                header.SetActive(false);
                reactivationButton.SetActive(true);
                moduleText.enabled = false;
            }
            
            else
            {
                if(Application.isPlaying)
                    header.SetActive(false);
                reactivationButton.SetActive(false);
                moduleText.enabled = false;
            }
        }

        
        //--------------------------------------------------------------------------------------------------------------

        public static ModuleCanvasElement CreateComponent(GameObject where, Module module)
        {
            var target = where.GetComponent<ModuleCanvasElement>();
            target.SetText(module.GetState(ValueInterpretationOption.DefaultValue), InvokeOrigin.Constructor);
            target.module = module;
            
            module.AddOnValueChangedListener(OnValueChangedContext.Initialization, target.ModuleEventInitialization);
            module.AddOnValueChangedListener(OnValueChangedContext.Update, target.ModuleEventUpdate);
            
            module.AddOnRepaintChangedListener(target.ModuleGUIEvent);
            module.OnActiveAndEnabledStateChanged += target.ModuleActiveAndEnabledStateChanged;

            return target;
        }

        private void OnDestroy()
        {
            if(module == null) return;
            module.RemoveOnValueChangedListener(OnValueChangedContext.Initialization);
            module.RemoveOnValueChangedListener(OnValueChangedContext.Update);
            
            module.RemoveOnRepaintChangedListener(ModuleGUIEvent);
            module.OnActiveAndEnabledStateChanged -= ModuleActiveAndEnabledStateChanged;
        }

        private void OnApplicationQuit()
        {
            if(module == null) return;
            module.RemoveOnValueChangedListener(OnValueChangedContext.Initialization);
            module.RemoveOnValueChangedListener(OnValueChangedContext.Update);
            
            module.RemoveOnRepaintChangedListener(ModuleGUIEvent);
            module.OnActiveAndEnabledStateChanged -= ModuleActiveAndEnabledStateChanged;
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
    
        
        private void ModuleEventInitialization (IModuleData data)
        {
            SetText(data.State, InvokeOrigin.Initialization);
            var ctx = data.Sender.Configuration;
            moduleText.fontSize = ctx.individualFontSize? ctx.infixFontSize : ctx.fontSize;
        }

        private void Show(IModuleData data)
        {
            if(moduleText != null)
                moduleText.enabled = true;
        }

        private void Hide(IModuleData data)
        {
            if(moduleText != null)
                moduleText.enabled = false;
        }

        private void ModuleEventUpdate(IModuleData data)
        {
            SetText(data.State, InvokeOrigin.Constructor);
        }

        #endregion
        

        //--------------------------------------------------------------------------------------------------------------

        #region --- INSTANCE SETTER ---

        private void SetText(string text, InvokeOrigin source)
        {
            if(moduleText != null)
                moduleText.text = text;
        }
        
        private void SetColor(Color color, InvokeOrigin source)
        {
            if(background != null)
                background.color = color;
        }

        private void SetFontSize(float size, InvokeOrigin source)
        {
            if(moduleText != null)
                moduleText.fontSize = Mathf.Clamp(size, Configuration.Configurable.MINFONTSIZE, Configuration.Configurable.MAXFONTSIZE);
        }

        private void SetMargins(Vector4 margin, InvokeOrigin source)
        {
            if(moduleText == null)
                return;
            
            moduleText.margin = margin;
            
            /*
             * Do not remove the following lines!
             * If removed the margin of the TMP will cease to properly update 
             */
            moduleText.fontSize++;
            moduleText.fontSize--;
            
            // Only force update canvas if application is not in playmode to prevent it being send by message (Awake/Start)
            if(source != InvokeOrigin.UnityMessage)
                Canvas.ForceUpdateCanvases();
        }

        #endregion
    }
}
