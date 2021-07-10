using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Dev_Blog.Utils
{
    /// <summary>
    /// A custom parser for markdown. 
    /// <para>It supports code blocks, code, references, images, sounds, urls, horizontal rules,
    /// unordered and ordered lists, blockquotes, headings, bold, italics, strikethrough and underlines</para>
    /// 
    /// <para>It does not follow the markdown conventions perfectly, so take note of the following points below:</para>
    /// 
    /// <para> - Code blocks and code both do the exact same thing. 
    /// Code blocks have a higher priority than code, so you can't have code block characters inside of a code section.</para>
    /// 
    /// <para> - References cannot have a space between the two <c>[][]</c>.
    /// Don't forget that when the reference is an image or a sound, it needs to be written <c>![][]</c> or <c>?[][]</c></para>
    /// <para> - Sound markdown is used like images, like so: <c>?[Aria-label](audioSource)</c></para>
    /// <para> - Horizontal rules do not need empty lines before and after them</para>
    /// <para> - Nested lists are not supported</para>
    /// <para> - Headings only work when they start with hashtags(#), writing lines below the heading will just create a rule</para>
    /// <para> - Bold and italics cannot be done with underscores, only asterisks (*)</para>
    /// <para> - Strikethrough text is done by surrounding it with tildes (~)</para>
    /// <para> - Underlined text is done by surrounding it with underscores (_)</para>
    /// <para> - You can insert font-awesome icons with $[alt text](fa-car)</para>
    /// </summary>
    public static class MarkdownParser
    {
        /// <summary>
        /// Parses the given markdown string, but removes all HTML tags to return raw text.
        /// </summary>
        /// <param name="markdown">The markdown to parse into raw text</param>
        /// <returns>The article's raw text</returns>
        public static string RawText(string markdown)
        {
            string result = ParseString(markdown);

            result = result.Replace("<br />", "\n");
            result = result.Replace("</p><p>", "\n\n");
            result = Regex.Replace(result, @"<[^>]*?\/>|<.*?>", "");

            return result;
        }

        /// <summary>
        /// Parses the given markdown string into html code.
        /// Takes care of encoding html characters inside of the given string to avoid XSS.
        /// Refer to <see cref="MarkdownParser">MarkdownParser</see> for what is supported and what isn't.
        /// </summary>
        /// <param name="markdown">The markdown to parse into html</param>
        /// <returns>The html string</returns>
        public static string ParseString(string markdown)
        {
            if (String.IsNullOrWhiteSpace(markdown))
            {
                return "";
            }

            markdown = markdown.Replace("\r\n", "\n");
            if (!markdown.EndsWith("\n"))
            {
                markdown += "\n";
            }

            markdown = HttpUtility.HtmlEncode(markdown);

            markdown = parseCodeBlock(markdown);
            markdown = parseCode(markdown);
            markdown = parseReferences(markdown);
            markdown = parseFontAwesome(markdown);
            markdown = parseImages(markdown);
            markdown = parseSounds(markdown);
            markdown = parseYoutubeEmbed(markdown);
            markdown = parseUrls(markdown);
            markdown = parseHorizontalRules(markdown);
            markdown = parseUnorderedLists(markdown);
            markdown = parseOrderedLists(markdown);
            markdown = parseBlockQuotes(markdown);
            markdown = parseHeadings(markdown);
            markdown = parseBold(markdown);
            markdown = parseItalics(markdown);
            markdown = parseStrikethrough(markdown);
            markdown = parseUnderline(markdown);

            markdown = Regex.Replace(markdown, @"\\(.)", "$1");
            markdown = markdown.Replace("\n\n", "</p><p>");
            markdown = markdown.Replace("  \n", "<br />");
            markdown = $"<p>{markdown}</p>";
            markdown = Regex.Replace(markdown, @"<p>(\s|<br\s*\/>)*<\/p>", "");
            markdown = centerElements(markdown, "img", "audio", "span");
            return markdown;
        }

        private static string centerElements(string markdown, params string[] tags)
        {
            foreach (string tag in tags)
            {
                markdown = centerElement(markdown, tag);
            }

            return markdown;
        }

        private static string centerElement(string markdown, string tag)
        {
            string newMarkdown = markdown;

            Regex regex = new Regex(@$"(<p>)((\s|<br>|<br\s*\/>)*<{tag}.*(>.*<\/{tag}>|\/>))*(\s|<br>|<br\s*\/>)*<\/p>");
            Match match = regex.Match(newMarkdown);
            while (match.Success)
            {
                int index = match.Groups[1].Index;
                int toRemove = match.Groups[1].Length;

                //Remove the <p> element
                newMarkdown = newMarkdown.Remove(index, toRemove);

                newMarkdown = newMarkdown.Insert(index, @$"<p class=""group group-{tag}"">");

                match = regex.Match(newMarkdown);
            }

            return newMarkdown;
        }

        private static string parseYoutubeEmbed(string markdown)
        {
            string newMarkdown = markdown;
            Regex regex = new Regex(@"((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?");
            Match match = regex.Match(newMarkdown);
            while (match.Success)
            {
                int index = match.Index;
                int toRemove = match.Length;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                //It's important to escape the link so it no longer matches the regex
                string embedUrl = $"https://www.youtube.com/embed/{match.Groups[5].Value}".EscapeAllMarkdownChars();
                string embed = $"<iframe type=\"text/html\" src=\"{embedUrl}\"></iframe>";

                newMarkdown = newMarkdown.Insert(index, $"<span class=\"ytplayer\">{embed}</span>");

                match = regex.Match(newMarkdown);
            }
            return newMarkdown;
        }
        private static string parseUrls(string markdown)
        {
            markdown = parseIndirectUrls(markdown);
            markdown = parseDirectUrls(markdown);
            markdown = parseDirectEmails(markdown);
            return markdown;
        }

        private static string parseDirectUrls(string markdown)
        {
            string newMarkdown = markdown;
            Regex regex = new Regex(@"([^\\]|^)(https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?(&amp;)//=]*))");
            Match match = regex.Match(newMarkdown);
            while (match.Groups[2].Success)
            {
                int index = match.Groups[2].Index;
                int toRemove = match.Groups[2].Length;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                string matchText = match.Groups[2].Value.EscapeAllMarkdownChars();

                string href = $" href=\"{matchText}\"";
                string linkText = matchText;

                newMarkdown = newMarkdown.Insert(index, $"<a{href}>{linkText}</a>");

                match = regex.Match(newMarkdown);
            }

            return newMarkdown;
        }

        private static string parseDirectEmails(string markdown)
        {
            string newMarkdown = markdown;
            Regex regex = new Regex(@"\w+@\w+\.\w+");
            Match match = regex.Match(newMarkdown);
            while (match.Success)
            {
                int index = match.Index;
                int toRemove = match.Length;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                string matchText = match.Value.EscapeAllMarkdownChars();

                string href = $" href=\"mailto:{matchText}\"";
                string linkText = matchText;

                newMarkdown = newMarkdown.Insert(index, $"<a{href}>{linkText}</a>");

                match = regex.Match(newMarkdown);
            }

            return newMarkdown;
        }

        private static string parseIndirectUrls(string markdown)
        {
            string newMarkdown = markdown;
            Regex regex = new Regex(@"([^\\]|^)(\[(.*)\]\((.*?)( &quot;?(.*?)&quot;?)?\))");
            Match match = regex.Match(newMarkdown);
            while (match.Success)
            {
                int index = match.Groups[2].Index;
                int toRemove = match.Groups[2].Length;

                bool addTitle = match.Groups[6].Success && match.Groups[6].Value.Trim().Length != 0;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                string link = match.Groups[4].Value;

                if (Regex.IsMatch(link, @"\w+@\w+\.\w+"))
                {
                    link = "mailto:" + link;
                }
                string href = $" href=\"{link.EscapeAllMarkdownChars()}\"";
                string linkText = match.Groups[3].Value.EscapeLinks();
                string title = "";
                if (addTitle)
                {
                    title = $" title=\"{match.Groups[6].Value.EscapeAllMarkdownChars()}\"";
                }

                newMarkdown = newMarkdown.Insert(index, $"<a{href}{title}>{linkText}</a>");


                match = regex.Match(newMarkdown);
            }

            return newMarkdown;
        }

        private static string parseFontAwesome(string markdown)
        {
            string newMarkdown = markdown;
            Regex regex = new Regex(@"([^\\]|^)(\$\[(.*?)\]\((.*?)\))");
            Match match = regex.Match(newMarkdown);
            while (match.Success)
            {
                int index = match.Groups[2].Index;
                int toRemove = match.Groups[2].Length;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                string src = $"{match.Groups[4].Value.EscapeImportantMarkdownChars()}";
                string alt = $" aria-label=\"{match.Groups[3].Value.EscapeImportantMarkdownChars()}\"";
                string title = $" title=\"{match.Groups[3].Value.EscapeImportantMarkdownChars()}\"";

                newMarkdown = newMarkdown.Insert(index, $"<i class=\"{src}\"{alt}{title}></i>");
                match = regex.Match(newMarkdown);
            }

            return newMarkdown;
        }

        private static string parseImages(string markdown)
        {
            string newMarkdown = markdown;
            Regex regex = new Regex(@"([^\\]|^)(!\[(.*?)\]\((.*?)( &quot;(.*?)&quot;)?\))");
            Match match = regex.Match(newMarkdown);
            while (match.Success)
            {
                int index = match.Groups[2].Index;
                int toRemove = match.Groups[2].Length;

                bool addTitle = match.Groups[6].Success && match.Groups[6].Value.Trim().Length != 0;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                string src = $" src=\"{match.Groups[4].Value.EscapeAllMarkdownChars()}\"";
                string alt = $" alt=\"{match.Groups[3].Value.EscapeAllMarkdownChars()}\"";
                string title = "";
                if (addTitle)
                {
                    title = $" title=\"{match.Groups[6].Value.EscapeAllMarkdownChars()}\"";
                }

                newMarkdown = newMarkdown.Insert(index, $"<img{src}{alt}{title}/>");
                match = regex.Match(newMarkdown);
            }

            return newMarkdown;
        }

        private static string parseSounds(string markdown)
        {
            string newMarkdown = markdown;

            Regex regex = new Regex(@"([^\\]|^)(\?\[(.*)\]\((.*)\))");
            Match match = regex.Match(newMarkdown);
            while (match.Success)
            {
                int index = match.Groups[2].Index;
                int toRemove = match.Groups[2].Length;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                string src = $" src=\"{match.Groups[4].Value.EscapeAllMarkdownChars()}\"";
                string ariaLabel = $" aria-label=\"{match.Groups[3].Value.EscapeAllMarkdownChars()}\"";

                newMarkdown = newMarkdown.Insert(index, $"<audio {ariaLabel} controls><source{src}/></audio>");
                match = regex.Match(newMarkdown);
            }

            return newMarkdown;
        }

        private static string parseHorizontalRules(string markdown)
        {
            string newMarkdown = "";
            foreach (var line in markdown.Split("\n"))
            {
                newMarkdown += parseHorizontalRule(line);
            }

            return newMarkdown;
        }

        private static string parseHorizontalRule(string line)
        {
            if (Regex.IsMatch(line, @"^(-{3,}|\*{3,}|_{3,})$"))
            {
                return "</p><hr>\n<p>";
            }
            else
            {
                return line + "\n";
            }
        }

        private static string parseBlockQuotes(string markdown)
        {
            return parseRepeatedStartingChars(markdown, @"&gt;\s", "blockquote", null, true);
        }

        private static string parseOrderedLists(string markdown)
        {
            return parseRepeatedStartingChars(markdown, @"\d+\.\s", "ol", "li");
        }

        private static string parseUnorderedLists(string markdown)
        {
            return parseRepeatedStartingChars(markdown, @"[\*\+\-]\s", "ul", "li");
        }

        /// <summary>
        /// This method is used to parse blockquotes and lists.
        /// </summary>
        /// <param name="markdown">The markdown to process</param>
        /// <param name="regexToMatch">The regex to use</param>
        /// <param name="groupTag">The group tag to use</param>
        /// <param name="itemTag">The item tag to use. Set to null if there is none, like for blockquotes</param>
        /// <param name="paragraphInside">Whether the group has a paragraph inside it. Only used for blockquotes</param>
        /// <returns>The parsed markdown</returns>
        private static string parseRepeatedStartingChars(string markdown, string regexToMatch, string groupTag, string itemTag, bool paragraphInside = false)
        {
            string newMarkdown = "";
            string currBlock = "";
            foreach (var line in markdown.Split("\n"))
            {
                var match = Regex.Match(line, $"^({regexToMatch}).*$");

                if (!match.Success)
                {
                    if (String.IsNullOrEmpty(currBlock))
                    {
                        newMarkdown += line + "\n";
                        continue;
                    }

                    currBlock = parseUnorderedLists(currBlock);
                    currBlock = parseOrderedLists(currBlock);
                    currBlock = parseBlockQuotes(currBlock);

                    if (paragraphInside)
                    {
                        currBlock = $"<p>{currBlock}</p>";
                    }

                    newMarkdown += $"</p><{groupTag}>{currBlock}</{groupTag}><p>";
                    newMarkdown += line + "\n";
                    currBlock = "";

                    continue;
                }

                string newLine = line[match.Groups[1].Value.Length..];
                if (itemTag != null)
                {
                    newLine = $"<{itemTag}>{newLine}</{itemTag}>";
                }
                else
                {
                    newLine = parseHeading(newLine);
                    newLine = parseHorizontalRule(newLine);
                    newLine += "\n";
                }
                currBlock += newLine;
            }
            return newMarkdown;
        }

        private static string parseHeadings(string markdown)
        {
            string newMarkdown = "";
            foreach (var line in markdown.Split('\n'))
            {
                newMarkdown += parseHeading(line) + "\n";
            }

            return newMarkdown;
        }

        private static string parseHeading(string line)
        {
            string currHeader = "###### ";
            do
            {
                if (line.StartsWith(currHeader))
                {
                    line = line[currHeader.Length..];

                    return $"</p><h{currHeader.Length - 1}>{line}</h{currHeader.Length - 1}><p>";
                }
                //Remove one #
                currHeader = currHeader[1..];
            } while (currHeader.Length > 1);

            return line;
        }

        /// <summary>
        /// Parses given markdown with character that surrounds the c
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="character"></param>
        /// <param name="htmlTag"></param>
        /// <returns></returns>
        private static string parseSurroundingChars(string markdown, string character, string htmlTag, bool escapeContent = false)
        {
            string newMarkdown = "";

            var groups = markdown.Split(character).ToList();

            //We might need to escape the character if there was a \, so we'll reform groups that were wrongly split
            for (int i = 0; i < groups.Count - 1; i++)
            {
                string curr = groups[i];

                string finalBackslashes = Regex.Match(curr, @"^.*?(\\*)$", RegexOptions.Singleline).Groups[1].Value;

                if (isEscaped(finalBackslashes))
                {
                    groups[i] += character + groups[i + 1];
                    groups.RemoveAt(i + 1);
                    i--;
                }
            }

            bool open = false;
            for (int i = 0; i < groups.Count; i++)
            {
                string group = groups[i];

                //Do not open a new bracket if we're at the last group.
                if (i == groups.Count - 1 && !open)
                {
                    newMarkdown += group;
                    break;
                }
                if (open && escapeContent)
                {
                    group = group.EscapeAllMarkdownChars();
                }
                newMarkdown += group + (open ? $"</{htmlTag}>" : $"<{htmlTag}>");
                open = !open;
            }

            return newMarkdown;
        }

        public static string EscapeMarkdown(this string markdown, params string[] toEscape)
        {
            foreach (string val in toEscape)
            {
                markdown = markdown.Escape(val);
            }

            return markdown;
        }

        public static string EscapeAllMarkdownChars(this string markdown) => markdown.EscapeMarkdown(@"\", "*", "`", "~", "-", "#", "+", "1", "_", "h", "[", "!", "?", "@", ".");
        public static string EscapeLinks(this string markdown) => markdown.EscapeMarkdown("@", "h", ".");
        public static string EscapeImportantMarkdownChars(this string markdown) => markdown.EscapeMarkdown(@"\", "*", "`", "~", "#", "+", "_", "[", "!", "?", "@", ".");

        public static string Escape(this string markdown, string charToEscape)
        {

            Regex regex = new Regex(@$"(\\*){(@".$^{[(|)*+?\".Contains(charToEscape) ? @"\" : "")}{charToEscape}", RegexOptions.RightToLeft);

            var matches = regex.Matches(markdown);
            foreach (Match match in matches)
            {
                if (!isEscaped(match.Groups[1].Value))
                {
                    markdown = markdown.Insert(match.Index, @"\");
                }
            }

            return markdown;
        }

        private static string parseBold(string markdown)
        {
            return parseSurroundingChars(markdown, "**", "strong");
        }

        private static string parseItalics(string markdown)
        {
            return parseSurroundingChars(markdown, "*", "em");
        }

        private static string parseStrikethrough(string markdown)
        {
            return parseSurroundingChars(markdown, "~", "s");
        }

        private static string parseUnderline(string markdown)
        {
            return parseSurroundingChars(markdown, "_", "u");
        }

        private static string parseCodeBlock(string markdown)
        {
            return parseSurroundingChars(markdown, "```", "code", true);
        }

        private static string parseCode(string markdown)
        {
            return parseSurroundingChars(markdown, "`", "code", true);
        }

        /// <summary>
        /// Parses markdown references in the given markdown string. 
        /// Note that labels cannot have spaces or special characters in them.
        /// <para>Creating references is done on a new line, and looks like this: <c>[label]: link "optional title"</c></para>
        /// <para>Refering to a reference is done like this: <c>[alt or aria-label text][label]</c></para>
        /// </summary>
        /// <param name="markdown">The string to process</param>
        /// <returns>The parsed string</returns>
        private static string parseReferences(string markdown)
        {
            string newMarkdown = markdown;
            Regex regex = new Regex(@"([^\\]|^)\[.*?\](\[(\w+)\])");
            Match match = regex.Match(newMarkdown);
            while (match.Groups[2].Success)
            {
                int index = match.Groups[2].Index;
                int toRemove = match.Groups[2].Length;

                newMarkdown = newMarkdown.Remove(index, toRemove);

                newMarkdown = newMarkdown.Insert(index, $"({getReferenceValue(match.Groups[3].Value, newMarkdown)})");

                match = regex.Match(newMarkdown);
            }

            return removeReferences(newMarkdown);
        }

        /// <summary>
        /// Returns the value of the reference based on the given key
        /// </summary>
        /// <param name="key">The key to get the value for</param>
        /// <param name="markdown">The markdown to search for the key</param>
        /// <returns>The value of the key, or "missing reference" if the key isn't found.</returns>
        private static string getReferenceValue(string key, string markdown)
        {
            foreach (var line in markdown.Split("\n"))
            {
                Match match = Regex.Match(line, @$"\[{key}\]: (.*)");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return "missing reference";
        }

        /// <summary>
        /// Removes <c>[abc]: link</c> references from the markdown document so that they're not rendered.
        /// </summary>
        /// <param name="markdown">The markdown to modify</param>
        /// <returns>The markdown without references</returns>
        private static string removeReferences(string markdown)
        {
            string newMarkdown = "";
            foreach (var line in markdown.Split("\n"))
            {
                if (!Regex.IsMatch(line, @"\[\w+\]: .*"))
                {
                    newMarkdown += line + "\n";
                    continue;
                }
            }
            return newMarkdown;
        }


        /// <summary>
        /// Determines if a character is escaped, based on the amount of backslashes given. If odd, it is escaped.
        /// </summary>
        /// <param name="backslashes"></param>
        /// <returns></returns>
        private static bool isEscaped(string backslashes) => backslashes.Length % 2 == 1;
    }
}
