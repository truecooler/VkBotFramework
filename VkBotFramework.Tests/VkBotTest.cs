using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using VkNet.Abstractions;
using VkNet.Model;

namespace VkBotFramework.Tests
{
	[TestFixture]
	public class VkBotTest
	{
		[SetUp]
		public void TestSetup()
		{
		}


		[TearDown]
		public void TestCleanup()
		{
		}

		ServiceCollection MockResolveScreenNameApi(VkObject resolveScreenNameFakeResponse = null)
		{
			if (resolveScreenNameFakeResponse == null)
				resolveScreenNameFakeResponse = new VkObject() {Type = VkNet.Enums.VkObjectType.Group, Id = 123456};

			var di = new ServiceCollection();

			var mockRepo = new MockRepository(MockBehavior.Default);

			var vkMock = mockRepo.Create<IVkApi>();
			var utilsMock = mockRepo.Create<IUtilsCategory>();

			vkMock.SetupGet(x => x.Utils).Returns(utilsMock.Object);
			utilsMock.Setup(x => x.ResolveScreenName(It.IsAny<string>())).Returns(resolveScreenNameFakeResponse);

			di.AddSingleton<IVkApi>(x => { return vkMock.Object; });

			return di;
		}

		[Test]
		public void VkBotCtor_PutFakeGroupId_ResolvedGroupIdShouldBeAsFake()
		{
			long expectedGroupId = 111222;
			var resolveScreenNameFakeResponse = new VkObject()
				{Type = VkNet.Enums.VkObjectType.Group, Id = expectedGroupId};
			var di = MockResolveScreenNameApi(resolveScreenNameFakeResponse);

			var bot = new VkBot("test", "test", di);

			Assert.AreEqual(bot.GroupId, expectedGroupId);
		}

		[Test]
		public void VkBotCtor_PutFilteredGroupUrl_GroupUrlShouldBeAsFiltered()
		{
			var di = MockResolveScreenNameApi();
			string expectedGroupUrl = "someGroupUrl";

			var bot = new VkBot("test", expectedGroupUrl, di);

			Assert.AreEqual(bot.FilteredGroupUrl, expectedGroupUrl);
		}


		[Test]
		[TestCase("https://vk.com/someGroupUrl", "someGroupUrl")]
		[TestCase("https://vk.com/folder/url/someGroupUrl", "someGroupUrl")]
		[TestCase("///////someGroupUrl", "someGroupUrl")]
		[TestCase("9999/someGroupUrl", "someGroupUrl")]
		public void VkBotCtor_PutUnfilteredGroupUrl_GroupUrlShouldBeFiltered(string unfilteredGroupUrl,
			string expectedGroupUrl)
		{
			var di = MockResolveScreenNameApi();

			var bot = new VkBot("test", unfilteredGroupUrl, di);

			Assert.AreEqual(bot.FilteredGroupUrl, expectedGroupUrl);
		}
	}
}