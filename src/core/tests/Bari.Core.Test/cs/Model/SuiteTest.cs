﻿using Bari.Core.Model;
using Bari.Core.Test.Helper;
using FluentAssertions;
using NUnit.Framework;

namespace Bari.Core.Test.Model
{
    [TestFixture]
    public class SuiteTest
    {
        [Test]
        public void SuiteNameCanBeModified()
        {
            var suite = new Suite(new TestFileSystemDirectory("root")) {Name = "test"};
            suite.Name.Should().Be("test");
            suite.Name = "test2";
            suite.Name.Should().Be("test2");
        }

        [Test]
        public void SuiteHasNoModulesInitially()
        {
            var suite = new Suite(new TestFileSystemDirectory("root"));
            suite.Modules.Should().BeEmpty();
        }

        [Test]
        public void GetModuleCreatesInstanceIfMissing()
        {
            var suite = new Suite(new TestFileSystemDirectory("root"));
            var mod1 = suite.GetModule("mod");

            mod1.Should().NotBeNull();
            mod1.Name.Should().Be("mod");
        }

        [Test]
        public void GetModuleReturnsTheSameInstanceIfCalledTwice()
        {
            var suite = new Suite(new TestFileSystemDirectory("root"));
            var mod1 = suite.GetModule("mod");
            var mod2 = suite.GetModule("mod");

            mod1.Should().BeSameAs(mod2);
        } 

        [Test]
        public void HasModuleWorksCorrectly()
        {
            var suite = new Suite(new TestFileSystemDirectory("root"));

            suite.HasModule("mod").Should().BeFalse();
            suite.GetModule("mod");
            suite.HasModule("mod").Should().BeTrue();
        }

        [Test]
        public void StoresReferenceToRoot()
        {
            var fs = new TestFileSystemDirectory("root");
            var suite = new Suite(fs);

            suite.SuiteRoot.Should().Be(fs);
        }
    }
}