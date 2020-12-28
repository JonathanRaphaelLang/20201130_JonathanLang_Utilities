// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using Ganymed.Monitoring.Enumerations;
// using UnityEditor;
//
// namespace Ganymed.Commands.Editor
// {
//     [InitializeOnLoad]
//     public class ConsoleCommandManagerEditor
//     {
//         private static List<MethodInfo> methods = new List<MethodInfo>();
//
//         private const System.Reflection.BindingFlags MethodFlags =  System.Reflection.BindingFlags.Public   |
//                                                                     System.Reflection.BindingFlags.Instance |
//                                                                     System.Reflection.BindingFlags.Static   |
//                                                                     System.Reflection.BindingFlags.NonPublic;
//
//         static ConsoleCommandManagerEditor()
//         {
//             foreach (var mono in MonoImporter.GetAllRuntimeMonoScripts())
//             {
//                 if (mono.GetClass() == null) continue;
//             
//                 foreach (var method in mono.GetClass().GetMethods(MethodFlags))
//                 {
//                     foreach (var attribute in method.GetCustomAttributes(typeof(Command), true))
//                     {
//                         methods.Add(method);
//                     }
//                 }
//             }
//         }
//
//         private static void ExecuteMethods(UnityEventType type)
//         {
//             foreach (var methodInfo in methods)
//             {
//                 string[] myInputString = {"10", "Hallo"};
//             
//                 var myParams = methodInfo.GetParameters();
//
//                 var objArray = new object[methodInfo.GetParameters().Length];
//             
//                 for (var i = 0; i < objArray.Length; i++)
//                 {
//                     objArray[i]  = Convert.ChangeType(myInputString[i], myParams[i].ParameterType);
//                 }
//             
//                 if (methodInfo.IsStatic)
//                     methodInfo.Invoke(null, objArray);
//             }
//         }
//     }
// }