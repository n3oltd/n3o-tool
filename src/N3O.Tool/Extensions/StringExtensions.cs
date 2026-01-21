using System;
using System.Collections.Generic;

namespace N3O.Tool.Extensions;

public static class StringExtensions {
    public static bool HasValue(this string s) {
        return !string.IsNullOrWhiteSpace(s);
    }

    public static IReadOnlyList<string> SplitArgs(this string s) {
        return (s ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}