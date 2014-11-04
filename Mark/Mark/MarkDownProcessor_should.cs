using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;

namespace Mark
{
    class MarkDownProcessor_should
    {
        private IMarkDownProcessor processor;

        [SetUp]
        private void arrange_processor()
        {
        }

        [Test]
        public void translate_empty_to_empty()
        {
            var htmlText = processor.TranslateToHtml("");
            Assert.AreEqual(htmlText, "");
        }

        [Test]
        public void tag_paragraphs()
        {
            var newLine = Environment.NewLine;
            var htmlText = processor.TranslateToHtml(String.Format("1{0}{0}2", newLine));
            Assert.AreEqual(htmlText, "<p>1</p><p>2</p>");
        }

        [Test]
        public void tag_paragraphs_ignoring_whitespaces()
        {
            var newLine = Environment.NewLine;
            var htmlText = processor.TranslateToHtml(String.Format("1{0}   {0}2", newLine));
            Assert.AreEqual(htmlText, "<p>1</p><p>2</p>");
        }

        [Test]
        public void tag_cursive_text()
        {
            var htmlText = processor.TranslateToHtml("blah blah _cursive_ blah blah");
            Assert.AreEqual(htmlText, "blah blah <em>cursive</em> blah blah");
        }

        [Test]
        public void screen_marking_symbols()
        {
            var htmlText = processor.TranslateToHtml("\\_normal\\_ \\__cursive_\\_ \\`__bold__\\`");
            Assert.AreEqual(htmlText, "normal <em>cursive</em> <strong>bold</strong>");
        }

        [Test]
        public void tag_bold_text()
        {
            var htmlText = processor.TranslateToHtml("blah blah __bold__ blah blah");
            Assert.AreEqual(htmlText, "blah blah <strong>bold</strong> blah blah");
        }

        [Test]
        public void tag_cursive_in_bold_text()
        {
            var htmlText = processor.TranslateToHtml("blah __blah _blah_ blah__ blah");
            Assert.AreEqual(htmlText, "blah <strong>blah <em>blah</em> blah</strong> blah");
        }

        [Test]
        public void tag_bold_in_cursive_text()
        {
            var htmlText = processor.TranslateToHtml("blah _blah __blah__ blah_ blah");
            Assert.AreEqual(htmlText, "blah <em>blah <strong>blah</strong> blah</em> blah");
        }

        [Test]
        public void tag_coded_text()
        {
            var htmlText = processor.TranslateToHtml("blah `blah _blah_ blah` blah");
            Assert.AreEqual(htmlText, "blah <code>blah blah blah</code> blah");
        }

        [Test]
        public void not_tag_underscore_inside_text_and_numbers()
        {
            const string text = "blah_blah__blah___blah 1_2__3___4";
            var htmlText = processor.TranslateToHtml(text);
            Assert.AreEqual(htmlText, text);
        }

        [Test]
        public void not_tag_unpaired_marking_symbols()
        {
            const string text = "blah _blah `blah __blah blah";
            var htmlText = processor.TranslateToHtml(text);
            Assert.AreEqual(htmlText, text);
        }
    }
}
