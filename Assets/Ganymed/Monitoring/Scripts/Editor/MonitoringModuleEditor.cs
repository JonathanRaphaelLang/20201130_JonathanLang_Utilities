using System.Reflection;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;
using Module = Ganymed.Monitoring.Core.Module;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(Module), editorForChildClasses: true),CanEditMultipleObjects]
    public class MonitoringModuleEditor : UnityEditor.Editor
    {
        private Module Target;
        
        private void OnEnable()
        {
            Target = (Module) target;
        }
        
        
        public static string GetTooltip(FieldInfo field, bool inherit)
        {
            var attributes
                = field.GetCustomAttributes(typeof(TooltipAttribute), inherit)
                    as TooltipAttribute[];
 
            var ret = "";
            if (attributes.Length > 0)
                ret = attributes[0].tooltip;
 
            return ret;
        }

        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Module", InspectorDrawer.H0);
            InspectorDrawer.DrawLine(Color.white);
            
            Target = (Module) target;
            
            Target.autoInspect = EditorGUILayout.Toggle(new GUIContent(
                "Enable Auto Inspection", GetTooltip(Target.GetType().GetField(nameof(Target.autoInspect)), true)),
                Target.autoInspect);
            
            if (Target.autoInspect)
            {
                Target.InspectOn = (Module.InspectPeriods) EditorGUILayout.EnumPopup("Inspect On", Target.InspectOn);
                if (Target.InspectOn == Module.InspectPeriods.Yield)
                {
                    Target.milliseconds = EditorGUILayout.IntField("Milliseconds", Target.milliseconds);
                    if (Target.milliseconds < 500)
                        Target.milliseconds = 500;
                }
            }

            DrawDefaultInspector();
        }
    }
}
