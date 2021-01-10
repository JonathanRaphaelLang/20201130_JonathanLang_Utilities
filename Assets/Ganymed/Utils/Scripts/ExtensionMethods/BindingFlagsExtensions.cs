using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class BindingFlagsExtensions
    {
        public static bool Contains(this BindingFlags flags, BindingFlags bindingFlags) =>
            (flags & bindingFlags) == bindingFlags;

        public static bool MatchesExactly(this BindingFlags flags, BindingFlags bindingFlags) =>
            flags == bindingFlags;

        public static bool MatchesPartly(this BindingFlags flags, BindingFlags bindingFlags) =>
            (flags & bindingFlags) != 0;
    }
}