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

		//[Test]
		//public void VkBot_CallConstructorWithNullDepency_ShouldHasAllDependencies()
		//{
		//	//var dl = new ServiceCollection();
		//	//dl.AddSingleton<IVkApi, VkApi>();
		//	var Bot = new VkBotTest();
		//	Assert.That(Bot.Api, Is.Not.Null);
		//	Assert.That(Bot.Logger, Is.Not.Null);

		//}


		[Test]
		public void VkBot_CallConstructorWithDepency_ShouldHasAllDependencies()
		{
			var dl = new ServiceCollection();
			var vk = new Mock<IVkApi>();
			vk.Setup(x => x.Authorize(It.IsAny<ApiAuthParams>()));
			vk.Setup(x => x.RefreshToken(It.IsAny<Func<string>>()) );
			vk.Setup(x => x.CallLongPoll(It.IsAny<string>(),It.IsAny<VkParameters>()));
			dl.AddSingleton<IVkApi>(x => vk.Object);
			var Bot = new VkBot();
			vk.Verify();
			//Assert.That(Bot.Api, Is.Not.Null);
			//Assert.That(Bot.Logger, Is.Not.Null);

		}



		[TearDown]
		public void TestCleanup()
		{

		}

	}
}
