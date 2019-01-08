using System;
using System.Collections.Generic;
using System.Text;
using NUnit;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Abstractions;
using Moq;
using VkNet.Model;
using VkNet.Utils;

namespace VkBotFramework.Tests
{
	[TestFixture]
    public class VkBotTest// : BaseTest
    {
		//VkBot Bot;

		[SetUp]
		public void TestSetup()
		{
			
		}



		[Test]
		public void Setup_PutMockThoughtDI_GetCorrectResolvedGroupId()
		{
			var dl = new ServiceCollection();
			var mockRepo = new MockRepository(MockBehavior.Default);
			var vkMock = mockRepo.Create<IVkApi>();
			var utilsMock = mockRepo.Create<IUtilsCategory>();
			vkMock.SetupGet(x => x.Utils).Returns(utilsMock.Object);

			long assumeGroupId = 111222;
			var resolveScreenNameResponse = new VkObject() { Type = VkNet.Enums.VkObjectType.Group, Id = assumeGroupId };
			utilsMock.Setup(x => x.ResolveScreenName(It.IsAny<string>())).Returns(resolveScreenNameResponse);
			dl.AddSingleton<IVkApi>(x => { return vkMock.Object; });
			var Bot = new VkBot(dl);
			Bot.Setup(accessToken:"test", groupUrl:"test");
			mockRepo.VerifyAll() ;
			Assert.That(Bot.GroupId, Is.EqualTo(assumeGroupId));

		}





		[TearDown]
		public void TestCleanup()
		{

		}

	}
}
