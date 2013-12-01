using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wia.Utility {
    public static class StringExtensions {
        public static bool IsNullOrEmpty(this string source) {
            return string.IsNullOrEmpty(source);
        }

        public static bool Contains(this string source, string compareWith, StringComparison comparison) {
            return source.IndexOf(compareWith, comparison) >= 0;
        }
    }
}