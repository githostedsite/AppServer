/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


#if DEBUG
namespace ASC.Common.Tests.Utils
{
    using System.IO;

    using ASC.Common.Utils;

    using NUnit.Framework;

    [TestFixture]
    public class HtmlUtil_Test
    {
        [Test]
        public void GetTextBr()
        {
            var html = "Hello";
            Assert.AreEqual("Hello", HtmlUtil.GetText(html));

            html = "Hello    anton";
            Assert.AreEqual("Hello    anton", HtmlUtil.GetText(html));

            //html = "Hello<\\ br>anton";
            //Assert.AreEqual("Hello\n\ranton", HtmlUtil.GetText(html));
        }

        public void Hard()
        {
            var html = @"<a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/ViewBlog.aspx?blogID=94fae49d-2faa-46d3-bf34-655afbc6f7f4""><font size=""+1"">XXX</font></a>
<div class=""moz-text-html"" lang=""x-unicode""><hr />
A &quot;b&quot; c, d:<br />
<blockquote>mp3 &quot;s&quot;<br />
<br />
&quot;s&quot; - book...</blockquote>... <br />
<hr />
w <a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/UserPage.aspx?userid=731fa2f6-0283-41ab-b4a6-b014cc29f358"">AA</a> 20 a 2009 15:53<br />
<a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/ViewBlog.aspx?blogID=94fae49d-2faa-46d3-bf34-655afbc6f7f4#comments"">fg</a></div>";

            System.Diagnostics.Trace.Write(HtmlUtil.GetText(html));
        }

        [Test]
        public void FromFile()
        {
            var html = File.ReadAllText("tests/utils/html_test.html");//Include file!
            //var text = HtmlUtil.GetText(html);

            //var advancedFormating = HtmlUtil.GetText(html, true);
            var advancedFormating2 = HtmlUtil.GetText(html, 40);
            Assert.IsTrue(advancedFormating2.Length <= 40);

            var advancedFormating3 = HtmlUtil.GetText(html, 40, "...");
            Assert.IsTrue(advancedFormating3.Length <= 40);
            StringAssert.EndsWith(advancedFormating3, "...");

            var empty = HtmlUtil.GetText(string.Empty);
            Assert.AreEqual(string.Empty, empty);

            var invalid = HtmlUtil.GetText("This is not html <div>");
            Assert.AreEqual(invalid, "This is not html");

            var xss = HtmlUtil.GetText("<script>alert(1);</script> <style>html{color:#444}</style>This is not html <div on click='javascript:alert(1);'>");
            Assert.AreEqual(xss, "This is not html");

            //var litleText = HtmlUtil.GetText("12345678901234567890", 20, "...",true);

            var test1 = HtmlUtil.GetText(null);
            Assert.AreEqual(string.Empty, test1);

            var test2 = HtmlUtil.GetText("text with \r\n line breaks", 20);
            Assert.IsTrue(test2.Length <= 20);

            var test3 = HtmlUtil.GetText("long \r\n text \r\n with \r\n text with \r\n line breaks", 20);
            Assert.IsTrue(test3.Length <= 20);

            var test4 = HtmlUtil.GetText("text text text text text text text text!", 20);
            Assert.IsTrue(test3.Length <= 20);
            StringAssert.StartsWith(test4, "text text text");
        }
    }
}
#endif