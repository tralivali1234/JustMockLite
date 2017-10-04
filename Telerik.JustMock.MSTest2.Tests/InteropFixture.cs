﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telerik.JustMock.MSTest2.Tests
{
	[TestClass]
	public unsafe class InteropFixture
	{
		public abstract class UnsafeClass1
		{
			public abstract void Do(void* ptr);
		}

		public abstract class UnsafeClass2
		{
			public abstract void* Alloc();

			public abstract int* Realloc(int* x);
		}

		[TestMethod]
		public void ShouldArrangeMethodWithPointerArg()
		{
			var mock = Mock.Create<UnsafeClass1>();
			int called = 0;

			Mock.ArrangeSet(() => mock.Do((void*)0)).IgnoreArguments().DoInstead(() => called++);
			mock.Do((void*)0);
			mock.Do((void*)123);
			Assert.Equal(2, called);
		}

		[TestMethod]
		public void ShouldArrangeMethodWithPointerReturnType()
		{
			var mock = Mock.Create<UnsafeClass2>();
			Mock.Arrange(mock, x => (IntPtr)x.Alloc()).Returns(new IntPtr(1234));
			var result = mock.Alloc();
			Assert.Equal(1234, new IntPtr(result).ToInt32());
		}

		[TestMethod]
		public void ShouldPassPointersToAndFromReturns()
		{
			var mock = Mock.Create<UnsafeClass2>();
			Mock.Arrange(mock, x => (IntPtr)x.Realloc((int*)0)).IgnoreArguments().Returns((IntPtr a) => a + 1000);
			var result = mock.Realloc((int*)123);
			Assert.Equal(1123, (int)result);
		}

		public abstract class UnsafeClassRef
		{
			public abstract void Do(ref void* arg);
		}

		public delegate void DoDelegate(ref IntPtr arg);

		[TestMethod]
		public void ShouldPassPointersByRef()
		{
			var mock = Mock.Create<UnsafeClassRef>();

			void* arg = null;
			Mock.NonPublic.Arrange(mock, "Do").DoInstead(new DoDelegate((ref IntPtr x) => x += 500));

			arg = (void*)1000;
			mock.Do(ref arg);
			Assert.Equal(1500, (int)arg);
		}

		public class UnsafeClass3
		{
			public virtual void* Do(void* x)
			{
				return (void*)((IntPtr)x + 1000);
			}
		}

		[TestMethod]
		public void ShouldCallOriginalImplementationWithPointers()
		{
			var mock = Mock.Create<UnsafeClass3>();
			Mock.Arrange(mock, x => x.Do(null)).IgnoreArguments().When((IntPtr x) => x.ToInt32() > 1000).CallOriginal();

			Assert.Equal(0, (int)mock.Do((void*)100));
			Assert.Equal(2500, (int)mock.Do((void*)1500));
		}

	}
}
