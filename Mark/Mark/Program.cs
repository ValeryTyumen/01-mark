using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Mark
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Incorrect arguments");
                return;
            }
            var file = args[0];
            if (! File.Exists(file))
            {
                Console.WriteLine("File doesn't exist");
                return;
            }
            var processor = new MarkDownProcessor();
            var text = "";
            using (var reader = new StreamReader(file))
                text = reader.ReadToEnd();
            using (var writer = new StreamWriter(args[0] + ".html"))
                writer.Write(processor.TranslateToHtml(text));
        }
    }

    enum Tag
    {
        Em,
        Strong,
        Code
    };

    enum TagType
    {
        Opening,
        Closing
    };

    internal class TagToPaste
    {
        public Tag Tag { get; private set; }
        public TagType Type { get; private set; }
        public int Position { get; private set; }

        public TagToPaste(Tag tag, TagType type, int position)
        {
            Tag = tag;
            Type = type;
            Position = position;
        }
    }

    public class MarkDownProcessor
    {
        private static string TranslateNewlines(string text)
        {
            var newParagraphRegex = new Regex(@"\r?\n\s*\r?\n");
            newParagraphRegex.Replace(text, "</p><p>");
            return "<p>" + newParagraphRegex.Replace(text, "</p><p>") + "</p>";
        }

        private static Tuple<Tag, TagType> GetTagParams(string mark)
        {
            if (mark == "`")
                return Tuple.Create(Tag.Code, TagType.Opening);
            var type = TagType.Opening;
            if (char.IsLetterOrDigit(mark[0]))
                type = TagType.Closing;
            var tag = Tag.Em;
            if (mark.Length == 3)
                tag = Tag.Strong;
            return Tuple.Create(tag, type);
        }

        private static Regex TagRegex = new Regex(@"((?<!\\)`|[^\w_\\]_{1,2}(?![\W_])|[^\W_\\]_{1,2}(?![\w_]))");

        private static int GetMatchIndex(Capture match)
        {
            if (match.Value == "`")
                return match.Index;
            else
                return match.Index + 1;
        }

        private static IEnumerable<TagToPaste> GetTagsToPaste(string text)
        {
            var result = new Stack<TagToPaste>();
            var bracketStack = new Stack<TagToPaste>();
            var gotCodeTag = false;
            foreach (Match match in TagRegex.Matches(text))
            {
                var tagParams = GetTagParams(match.Value);
                var current = new TagToPaste(tagParams.Item1, tagParams.Item2, GetMatchIndex(match));
                if (bracketStack.Count == 0)
                {
                    bracketStack.Push(current);
                    if (current.Tag == Tag.Code)
                        gotCodeTag = true;
                    continue;
                }
                if (MarkIsBacktick(current, bracketStack, result, ref gotCodeTag)) continue;
                AddUnderscoreTag(current, bracketStack, result);
            }
            return result.OrderBy(z => z.Position);
        }

        private static void AddUnderscoreTag
            (TagToPaste current, Stack<TagToPaste> bracketStack, Stack<TagToPaste> result)
        {
            var tagInStack = bracketStack.Peek();
            if (current.Type == TagType.Opening)
            {
                if (bracketStack.Count == 3)
                    return;
                if (tagInStack.Tag == current.Tag && tagInStack.Type == TagType.Opening)
                    return;
                bracketStack.Push(current);
            }
            else
            {
                if (tagInStack.Tag == current.Tag && tagInStack.Type == TagType.Opening)
                {
                    bracketStack.Pop();
                    result.Push(tagInStack);
                    result.Push(current);
                }
            }
        }

        private static bool MarkIsBacktick(TagToPaste current, Stack<TagToPaste> bracketStack, Stack<TagToPaste> result,
            ref bool gotCodeTag)
        {
            var tagInStack = bracketStack.Peek();
            if (current.Tag == Tag.Code)
            {
                if (gotCodeTag)
                {
                    while (tagInStack.Tag != Tag.Code)
                        tagInStack = bracketStack.Pop();
                    if (result.Count != 0)
                    {
                        var fakeResult = result.Peek();
                        while (result.Count != 0 && fakeResult.Position > tagInStack.Position)
                            fakeResult = result.Pop();
                    }
                    result.Push(tagInStack);
                    current = new TagToPaste(current.Tag, TagType.Closing, current.Position);
                    result.Push(current);
                    gotCodeTag = false;
                }
                else
                {
                    gotCodeTag = true;
                    bracketStack.Push(current);
                }
                return true;
            }
            return false;
        }

        private static string CreateTagString(Tag tag, TagType type)
        {
            var result = "<";
            if (type == TagType.Closing)
                result += "/";
            switch (tag)
            {
                case Tag.Code:
                    result += "code";
                    break;
                case Tag.Em:
                    result += "em";
                    break;
                case Tag.Strong:
                    result += "strong";
                    break;
            }
            return result + ">";
        }

        private static int GetTagLength(Tag tag)
        {
            if (tag == Tag.Strong)
                return 2;
            return 1;
        }

        private static string ReplaceMarksWithTags(string text)
        {
            var builder = new StringBuilder();
            var lastIndex = 0;
            foreach (var tagToPaste in GetTagsToPaste(text))
            {
                builder.Append(text.Substring(lastIndex, tagToPaste.Position - lastIndex));
                builder.Append(CreateTagString(tagToPaste.Tag, tagToPaste.Type));
                lastIndex = tagToPaste.Position + GetTagLength(tagToPaste.Tag);
            }
            builder.Append(text.Substring(lastIndex, text.Length - lastIndex));
            return builder.ToString();
        }

        public string TranslateToHtml(string text)
        {
            text = TranslateNewlines(text);
            text = ReplaceMarksWithTags(text);
            return text;
        }
    }
}
