using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Mark
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }

    public interface IMarkDownProcessor
    {
        string TranslateToHtml(string text);
    }



    public class MarkDownProcessor : IMarkDownProcessor
    {
        private static string TranslateNewlines(string text)
        {
            var newParagraphRegex = new Regex(@"\r?\n\s*\r?\n");
            newParagraphRegex.Replace(text, "</p><p>");
            return "<p>" + newParagraphRegex.Replace(text, "</p><p>") + "</p>";
        }

        private enum Tag
        {
            Em,
            Strong,
            Code
        };

        private readonly static Dictionary<Tag, Regex> MarkingUnderscoreRegexes =
            new Dictionary<Tag, Regex>
        {
            {Tag.Em, new Regex(@"((?<![\w_])_(?![\W_])|(?<=[^\W_]|\\)_(?![\w_]))")},
            {Tag.Strong, new Regex(@"((?<=\W)__(?=\w)|(?<=\w|\\)__(?=\W))")}
        };

        private readonly static Dictionary<Tag, string> TagNames =
            new Dictionary<Tag, string>
        {
            {Tag.Em, "em"},
            {Tag.Strong, "strong"},
            {Tag.Code, "code"}
        };

        private static string TranslateUnderscores(string text, Tag marking)
        {
            var markingUnderscoreRegex = MarkingUnderscoreRegexes[marking];
            var builder = new StringBuilder();
            var match = markingUnderscoreRegex.Match(text);
            if (! match.Success)
                return text;
            var lastMatchIndex = 0;
            while (match.Success)
            {
                builder.Append(text.Substring(lastMatchIndex, match.Index - lastMatchIndex));
                lastMatchIndex = match.Index + match.Length;
                builder.Append(TranslateOneUnderscoreMark(text, match.Index, marking));
                match = match.NextMatch();
            }
            builder.Append(text.Substring(lastMatchIndex, text.Length - lastMatchIndex));
            return builder.ToString();
        }


        private readonly static Regex LetterRegex = new Regex(@"([^\W_]|\\)");

        private static bool IsLetter(char sign)
        {
            string signString = sign.ToString();
            return LetterRegex.Match(signString).ToString() == signString;
        }

        private static string TranslateOneUnderscoreMark
            (string text, int index, Tag marking)
        {
            var markLength = (marking == Tag.Em) ? 1 : 2;
            var tag = TagNames[marking];
            if (IsLetter(text[index - 1]))
                return "</" + tag + ">";
            else
                return "<" + tag + ">";
        }

        private static string ScreenSomeUnderscores(string text, Tag marking)
        {
            var wrongTagRegex = new Regex(@"\\</?" + TagNames[marking] + @">");
            if (marking == Tag.Em)
                return wrongTagRegex.Replace(text, "_");
            else
                return wrongTagRegex.Replace(text, "__");
        }

        public static string TranslateBackticks(string text)
        {
            var builder = new StringBuilder();
            var lastBacktickIndex = 0;
            var lastWasClosing = true;
            for (var i = 0; i < text.Length; i++)
                if (text[i] == '`')
                {
                    if (lastWasClosing)
                    {
                        builder.Append(text.Substring(lastBacktickIndex, i - lastBacktickIndex));
                        builder.Append("<code>");
                        lastBacktickIndex = i + 1;
                        lastWasClosing = false;
                    }
                    else
                    {
                        var subString = text.Substring(lastBacktickIndex, i - lastBacktickIndex);
                        builder.Append(TranslateOnePairOfBackticks(subString));
                        builder.Append("</code>");
                        lastBacktickIndex = i + 1;
                        lastWasClosing = true;
                    }
                }
            var tail = text.Substring(lastBacktickIndex, text.Length - lastBacktickIndex);
            builder.Append(tail);
            return builder.ToString();
        }

        public static string TranslateOnePairOfBackticks(string text)
        {
            text = text.Replace("<em>", "_");
            text = text.Replace("</em>", "_");
            text = text.Replace("<strong>", "__");
            text = text.Replace("</strong>", "__");
            return text;
        }

        private static string RemoveUnpairedTags(string text, Tag marking)
        {
            var builder = new StringBuilder();
            var mark = "_";
            if (marking == Tag.Strong)
                mark = "__";
            if (marking == Tag.Code)
                mark = "`";
            var openingTag = "<" + TagNames[marking] + ">";
            var tagRegex = new Regex(@"</?" + TagNames[marking] + ">");
            var lastWasClosing = true;
            var lastCorrectMatchIndex = 0;
            foreach (Match match in tagRegex.Matches(text))
            {
                if (lastWasClosing)
                {
                    if (match.Value == openingTag)
                    {
                        var substring = text.Substring(lastCorrectMatchIndex, match.Index - lastCorrectMatchIndex);
                        builder.Append(RemoveAllTags(substring, marking));
                        lastWasClosing = false;
                        lastCorrectMatchIndex = match.Index + match.Length;
                    }
                }
                else
                {
                    if (match.Value != openingTag)
                    {
                        var substring = text.Substring(lastCorrectMatchIndex, match.Index - lastCorrectMatchIndex);
                        builder.Append(openingTag);
                        builder.Append(RemoveAllTags(substring, marking));
                        builder.Append(match.Value);
                        lastWasClosing = true;
                        lastCorrectMatchIndex = match.Index + match.Length;
                    }
                }
            }
            if (!lastWasClosing)
                builder.Append(mark);
            var tail = text.Substring(lastCorrectMatchIndex, text.Length - lastCorrectMatchIndex);
            builder.Append(RemoveAllTags(tail, marking));
            return builder.ToString();
        }

        private static string RemoveAllTags(string text, Tag marking)
        {
            var mark = "_";
            if (marking == Tag.Strong)
                mark = "__";
            if (marking == Tag.Code)
                mark = "`";
            var tagRegex = new Regex(@"</?" + TagNames[marking] + ">");
            return tagRegex.Replace(text, mark);
        }

        public string TranslateToHtml(string text)
        {
            text = TranslateNewlines(text);
            text = TranslateUnderscores(text, Tag.Em);
            text = TranslateUnderscores(text, Tag.Strong);
            text = ScreenSomeUnderscores(text, Tag.Em);
            text = ScreenSomeUnderscores(text, Tag.Strong);
            text = RemoveUnpairedTags(text, Tag.Em);
            text = RemoveUnpairedTags(text, Tag.Strong);
            text = TranslateBackticks(text);
            text = RemoveUnpairedTags(text, Tag.Code);
            return text;
        }
    }
}
