using Editor.Analyzers.Asset;
using Editor.Analyzers.Asset.RecommendedRules;
using NUnit.Framework;
using UnityEngine;

namespace Editor.Analyzers.Project.Tests
{
    public class NamingConventionsRuleTest
    {
        private const string PATH_PREFIX = "Assets/TestAssets/AssetAnalyzer/NamingConventionFiles/";
        private readonly NamingConventionsRule _rule;

        public NamingConventionsRuleTest()
        {
            var analyzer = LintingEngine.GetAnalyzer<AssetAnalyzer>();
            _rule = analyzer.GetRule<NamingConventionsRule>();

            Assert.IsNotNull(_rule);
        }

        #region Regex

        [Test]
        public void Regex_UpperCamel_2upper_success()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.UpperCamelCase];

            Assert.True(regex.IsMatch("IEnumerableTest"));
        }

        [Test]
        public void Regex_UpperCamel_3upper_fail()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.UpperCamelCase];

            Assert.False(regex.IsMatch("IEnumerableABTest"));
        }

        [Test]
        public void Regex_UpperCamel_not_lowerCamel_fail()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.UpperCamelCase];

            Assert.False(regex.IsMatch("testificateALocator"));
        }

        [Test]
        public void Regex_lowerCamel_2upper_success()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.LowerCamelCase];

            Assert.True(regex.IsMatch("testificateALocator"));
        }

        [Test]
        public void Regex_lowerCamel_3upper_fail()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.LowerCamelCase];

            Assert.False(regex.IsMatch("testificateABLocator"));
        }

        [Test]
        public void Regex_lowerCamel_success()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.LowerCamelCase];

            Assert.True(regex.IsMatch("test3LowerCamel"));
        }

        [Test]
        public void Regex_UpperCamel_success()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.UpperCamelCase];

            Assert.True(regex.IsMatch("Test3LowerCamel"));
        }

        [Test]
        public void Regex_ALL_CAPS_success()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.AllCaps];

            Assert.True(regex.IsMatch("TEST3_ALL_CAPS"));
        }

        [Test]
        public void Regex_snake_success()
        {
            var regex = NamingConventionsRule.ConventionValidator[NamingConvention.SnakeCase];

            Assert.True(regex.IsMatch("test3_snake_case"));
        }

        #endregion

        #region Script

        [Test]
        public void Script_UpperCamel_success()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "ScriptUpperCamel.cs", out AssetIssue<Object> issue);

            Assert.IsFalse(result);
            Assert.IsNull(issue);
        }

        [Test]
        public void Script_snake_case_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "script_snake.cs", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Script_lowerCamel_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "scriptLowerCamel.cs", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Script_ALL_UPPER_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "SCRIPT_ALL_UPPER.cs", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        #endregion

        #region Test1

        [Test]
        public void Test1_UpperCamel_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "Test1UpperCamel.test1", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Test1_snake_case_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "test1_snake.test1", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Test1_lowerCamel_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "test1LowerCamel.test1", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Test1_ALL_UPPER_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "TEST1_ALL_UPPER.test1", out AssetIssue<Object> issue);

            Assert.IsFalse(result);
            Assert.IsNull(issue);
        }

        #endregion

        #region Test2

        [Test]
        public void Test2_UpperCamel_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "Test2UpperCamel.test2", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Test2_snake_case_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "test2_snake.test2", out AssetIssue<Object> issue);

            Assert.IsFalse(result);
            Assert.IsNull(issue);
        }

        [Test]
        public void Test2_lowerCamel_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "test2LowerCamel.test2", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Test2_ALL_UPPER_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "TEST2_ALL_UPPER.test2", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        #endregion

        #region Test3

        [Test]
        public void Test3_UpperCamel_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "Test3UpperCamel.test3", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Test3_snake_case_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "test3_snake.test3", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void Test3_lowerCamel_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "test3LowerCamel.test3", out AssetIssue<Object> issue);

            Assert.IsFalse(result);
            Assert.IsNull(issue);
        }

        [Test]
        public void Test3_ALL_UPPER_fail()
        {
            var result = _rule.HasIssue(PATH_PREFIX + "TEST3_ALL_UPPER.test3", out AssetIssue<Object> issue);

            Assert.IsTrue(result);
            Assert.IsNotNull(issue);
        }

        #endregion
    }
}
