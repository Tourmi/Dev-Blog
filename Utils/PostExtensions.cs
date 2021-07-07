using Dev_Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Utils
{
    public static class PostExtensions
    {
        public static string RawPreview(this Post post, int maxLength = 500)
        {
            string raw = post.RawText();
            int newParagraphIndex = raw.IndexOf("\n\n");
            if (newParagraphIndex < 0)
            {
                if (raw.Length <= maxLength)
                {
                    return raw.Trim();
                }

                newParagraphIndex = raw.Length;
            }

            int lastWordIndex = raw.Substring(0, Math.Min(newParagraphIndex, maxLength)).LastIndexOf(" ");
            if (lastWordIndex < 0)
            {
                lastWordIndex = Math.Min(maxLength, newParagraphIndex);
            }

            raw = raw.Substring(0, lastWordIndex) + " [...]";

            return raw.Trim();
        }

        public static string Parse(this Post post) => MarkdownParser.ParseString(post.Content);
        public static string RawText(this Post post) => MarkdownParser.RawText(post.Content);
    }
}
