using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rodkulman.RedditReader
{
    public static class Strings
    {
        public static string RemoveDiacritics(this string str)
        {
            if (str == null) { return null; }

            var normalized = str.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int i = 0; i < normalized.Length; i++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(normalized[i]) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(normalized[i]);
                }
            }

            return sb.ToString();
        }
    }
}
