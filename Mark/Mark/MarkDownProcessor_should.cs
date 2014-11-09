using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;

namespace Mark
{
    class MarkDownProcessor_should
    {
        private IMarkDownProcessor processor;

        [SetUp]
        public void arrange_processor()
        {
            processor = new MarkDownProcessor();
        }

        [Test]
        public void tag_one_paragraph()
        {
            var htmlText = processor.TranslateToHtml("abcd1234");
            Assert.AreEqual("<p>abcd1234</p>", htmlText);
        }

        
        [Test]
        public void tag_unix_paragraphs()
        {
            var htmlText = processor.TranslateToHtml("1\n\n2");
            Assert.AreEqual("<p>1</p><p>2</p>", htmlText);
        }
        
        [Test]
        public void tag_dos_paragraphs()
        {
            var htmlText = processor.TranslateToHtml("1\r\n\r\n2");
            Assert.AreEqual("<p>1</p><p>2</p>", htmlText);
        }
        
        [Test]
        public void tag_unix_and_dos_paragraphs()
        {
            const string text = "1\n\n2\r\n\r\n3\r\n\n4\n\r\n5";
            const string expectedHtmlText = "<p>1</p><p>2</p><p>3</p><p>4</p><p>5</p>";
            var htmlText = processor.TranslateToHtml(text);
            Assert.AreEqual(expectedHtmlText, htmlText);
        }

        
        [Test]
        public void tag_paragraphs_ignoring_whitespaces()
        {
            const string text = "1\n \n2\r\n  \r\n3\r\n   \n4\n    \r\n5";
            const string expectedHtmlText = "<p>1</p><p>2</p><p>3</p><p>4</p><p>5</p>";
            var htmlText = processor.TranslateToHtml(text);
            Assert.AreEqual(expectedHtmlText, htmlText);
        }
        
        [Test]
        public void tag_cursive_text()
        {
            var htmlText = processor.TranslateToHtml("blah blah _cursive_ blah blah");
            Assert.AreEqual("<p>blah blah <em>cursive</em> blah blah</p>", htmlText);
        }

        [Test]
        public void tag_cursive_all_text()
        {
            var htmlText = processor.TranslateToHtml("_cursive_");
            Assert.AreEqual("<p><em>cursive</em></p>", htmlText);
        }

        [Test]
        public void tag_bold_text()
        {
            var htmlText = processor.TranslateToHtml("blah blah __bold__ blah blah");
            Assert.AreEqual("<p>blah blah <strong>bold</strong> blah blah</p>", htmlText);
        }

        [Test]
        public void tag_bold_all_text()
        {
            var htmlText = processor.TranslateToHtml("__bold__");
            Assert.AreEqual("<p><strong>bold</strong></p>", htmlText);
        }

        [Test]
        public void tag_cursive_in_bold_text()
        { 
            var htmlText = processor.TranslateToHtml("blah __blah _blah_ blah__ blah");
            Assert.AreEqual("<p>blah <strong>blah <em>blah</em> blah</strong> blah</p>", htmlText);
        }

        [Test]
        public void tag_bold_in_cursive_text()
        {
            var htmlText = processor.TranslateToHtml("blah _blah __blah__ blah_ blah");
            Assert.AreEqual("<p>blah <em>blah <strong>blah</strong> blah</em> blah</p>", htmlText);
        }

        [Test]
        public void not_tag_underscore_inside_text_and_numbers()
        {
            const string text = "blah_blah__blah___blah 1_2__3___4";
            var htmlText = processor.TranslateToHtml(text);
            Assert.AreEqual(htmlText, "<p>" + text + "</p>");
        }
        
        [Test]
        public void screen_cursive_marks()
        {
            var htmlText = processor.TranslateToHtml("\\_1\\_ _3 4\\_ 5_");
            Assert.AreEqual("<p>_1_ <em>3 4_ 5</em></p>", htmlText);
        }

        [Test]
        public void screen_bold_marks()
        {
            var htmlText = processor.TranslateToHtml("\\__1\\__ __3 4\\__ 5__");
            Assert.AreEqual("<p>__1__ <strong>3 4__ 5</strong></p>", htmlText);
        }

        [Test]
        public void tag_coded_text()
        {
            var htmlText = processor.TranslateToHtml("blah `blah _blah_ __blah__ blah` blah");
            Assert.AreEqual("<p>blah <code>blah _blah_ __blah__ blah</code> blah</p>", htmlText);
        }
        
        [Test]
        public void not_tag_unpaired_marking_symbols()
        {
            const string text = "blah _blah `blah __blah blah";
            var htmlText = processor.TranslateToHtml(text);
            Assert.AreEqual("<p>" + text + "</p>", htmlText);
        }
    }
}
