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
using ASC.Geolocation;

using NUnit.Framework;

namespace ASC.Common.Tests.Geolocation
{
    [TestFixture]
    public class GeolocationTest
    {
        [Test]
        public void GetIPGeolocationTest()
        {
            var helper = new GeolocationHelper(null, null);
            var info = helper.GetIPGeolocation("62.213.10.13");
            Assert.AreEqual("Nizhny Novgorod", info.City);
            Assert.AreEqual("062.213.011.127", info.IPEnd);
            Assert.AreEqual("062.213.008.240", info.IPStart);
            Assert.AreEqual("RU", info.Key);
            Assert.AreEqual("Europe/Moscow", info.TimezoneName);
            Assert.AreEqual(4d, info.TimezoneOffset);

            info = helper.GetIPGeolocation("");
            Assert.AreEqual(IPGeolocationInfo.Default.City, info.City);
            Assert.AreEqual(IPGeolocationInfo.Default.IPEnd, info.IPEnd);
            Assert.AreEqual(IPGeolocationInfo.Default.IPStart, info.IPStart);
            Assert.AreEqual(IPGeolocationInfo.Default.Key, info.Key);
            Assert.AreEqual(IPGeolocationInfo.Default.TimezoneName, info.TimezoneName);
            Assert.AreEqual(IPGeolocationInfo.Default.TimezoneOffset, info.TimezoneOffset);
        }
    }
}
#endif