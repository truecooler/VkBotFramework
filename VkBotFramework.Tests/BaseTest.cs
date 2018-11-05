using System;
using System.Collections.Generic;
using System.Text;

using VkBotFramework;

using NUnit.Framework;
using Moq;

namespace VkBotFramework.Tests
{
    class BaseTest
    {
		/// <summary>
		/// Экземпляр класса API.
		/// </summary>
		protected VkBot Bot;

		/// <summary>
		/// Ответ от сервера.
		/// </summary>
		protected string Json;

		/// <summary>
		/// Url запроса.
		/// </summary>
		protected string Url;


		/// <summary>
		/// Пред установки выполнения каждого теста.
		/// </summary>
		[SetUp]
		public void Init()
		{

		}

		/// <summary>
		/// После исполнения каждого теста.
		/// </summary>
		[TearDown]
		public void Cleanup()
		{
			Json = null;
			Url = null;
		}

	}
}
