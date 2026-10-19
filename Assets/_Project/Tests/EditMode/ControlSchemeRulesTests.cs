using GraceRun.Core.Settings;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ControlSchemeRulesTests
    {
        [Test]
        public void MixedKeepsEveryDefaultBinding()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.Mixed, null);
            Assert.AreEqual("<Keyboard>/j", layout.AttackPath);
            Assert.AreEqual("<Keyboard>/k", layout.KeyPaths[0]);
            Assert.AreEqual("<Keyboard>/semicolon", layout.KeyPaths[2]);
            Assert.AreEqual("<Mouse>/leftButton", layout.PointerPaths[0]);
            Assert.AreEqual("<Mouse>/rightButton", layout.PointerPaths[2]);
            Assert.AreEqual(0, layout.SliceSlot);
            Assert.IsTrue(layout.CursorAimKeys);
        }

        [Test]
        public void KeyboardOnlyHasNoMouseAndNoSlice()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null);
            for (var i = 0; i < ControlLayout.AbilitySlots; i++)
            {
                Assert.IsNotEmpty(layout.KeyPaths[i]);
                Assert.IsEmpty(layout.PointerPaths[i]);
            }
            Assert.AreEqual(-1, layout.SliceSlot);
            Assert.IsFalse(layout.CursorAimKeys);
            Assert.AreEqual("<Keyboard>/j", layout.AttackPath);
        }

        [Test]
        public void MouseOnlyHasNoKeysAndSlicesOnTheLeftButton()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.MouseOnly, null);
            Assert.IsEmpty(layout.AttackPath);
            for (var i = 0; i < ControlLayout.AbilitySlots; i++)
            {
                Assert.IsEmpty(layout.KeyPaths[i]);
                Assert.IsNotEmpty(layout.PointerPaths[i]);
            }
            Assert.AreEqual(0, layout.SliceSlot);
        }

        [Test]
        public void CustomSendsMouseChoicesToThePointerBinding()
        {
            var custom = CustomControls.Default;
            custom.Abilities[0] = ControlChoices.IndexOf("<Keyboard>/q");
            custom.Abilities[1] = ControlChoices.IndexOf("<Mouse>/backButton");
            var layout = ControlSchemeRules.Build(ControlScheme.Custom, custom);

            Assert.AreEqual("<Keyboard>/q", layout.KeyPaths[0]);
            Assert.IsEmpty(layout.PointerPaths[0]);
            Assert.AreEqual("<Mouse>/backButton", layout.PointerPaths[1]);
            Assert.IsEmpty(layout.KeyPaths[1]);
        }

        [Test]
        public void CustomSliceNeedsThatAbilityOnAMouseButton()
        {
            var custom = CustomControls.Default;
            custom.Slice = 0;
            custom.Abilities[0] = ControlChoices.IndexOf("<Keyboard>/k");
            Assert.AreEqual(-1, ControlSchemeRules.Build(ControlScheme.Custom, custom).SliceSlot);

            custom.Abilities[0] = ControlChoices.IndexOf("<Mouse>/rightButton");
            Assert.AreEqual(0, ControlSchemeRules.Build(ControlScheme.Custom, custom).SliceSlot);
        }

        [Test]
        public void CustomAttackCanBeNoneOrAMouseButton()
        {
            var custom = CustomControls.Default;
            custom.Attack = 0;
            Assert.IsEmpty(ControlSchemeRules.Build(ControlScheme.Custom, custom).AttackPath);
            custom.Attack = ControlChoices.IndexOf("<Mouse>/middleButton");
            Assert.AreEqual("<Mouse>/middleButton", ControlSchemeRules.Build(ControlScheme.Custom, custom).AttackPath);
        }

        [Test]
        public void CustomFromAPresetBehavesTheSameWhereOneChoicePerActionAllows()
        {
            var fromMouse = ControlSchemeRules.Build(ControlScheme.Custom, ControlSchemeRules.CustomFrom(ControlSchemeRules.Build(ControlScheme.MouseOnly, null)));
            Assert.IsEmpty(fromMouse.AttackPath);
            Assert.AreEqual("<Mouse>/leftButton", fromMouse.PointerPaths[0]);
            Assert.AreEqual(0, fromMouse.SliceSlot);

            var fromMixed = ControlSchemeRules.CustomFrom(ControlSchemeRules.Build(ControlScheme.Mixed, null));
            Assert.AreEqual(ControlChoices.IndexOf("<Keyboard>/k"), fromMixed.Abilities[0]);
        }

        [Test]
        public void CycleSliceGoesThroughOffAndTheThreeSlots()
        {
            Assert.AreEqual(0, ControlSchemeRules.CycleSlice(-1, 1));
            Assert.AreEqual(2, ControlSchemeRules.CycleSlice(1, 1));
            Assert.AreEqual(-1, ControlSchemeRules.CycleSlice(2, 1));
            Assert.AreEqual(2, ControlSchemeRules.CycleSlice(-1, -1));
        }

        [Test]
        public void ChoicesRoundTripThroughTheirPath()
        {
            for (var i = 1; i < ControlChoices.Count; i++)
                Assert.AreEqual(i, ControlChoices.IndexOf(ControlChoices.PathOf(i)));
            Assert.AreEqual(0, ControlChoices.IndexOf("<Keyboard>/nonsense"));
        }

        [Test]
        public void LabelsNameBothBindingsOfAMixedAbility()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.Mixed, null);
            Assert.AreEqual("K / Left Mouse", ControlSchemeRules.AbilityLabel(layout, 0));
            Assert.AreEqual("J", ControlSchemeRules.AttackLabel(layout));
            Assert.AreEqual("None", ControlSchemeRules.AttackLabel(ControlSchemeRules.Build(ControlScheme.MouseOnly, null)));
        }

        [Test]
        public void EverySchemeBindsTheHeavyAttack()
        {
            foreach (var scheme in new[] { ControlScheme.Mixed, ControlScheme.KeyboardOnly, ControlScheme.MouseOnly, ControlScheme.Custom })
            {
                var layout = ControlSchemeRules.Build(scheme, null);
                Assert.IsTrue(layout.HeavyKeyPath.Length > 0 || layout.HeavyPointerPath.Length > 0, scheme.ToString());
            }
            Assert.AreEqual("<Keyboard>/u", ControlSchemeRules.Build(ControlScheme.Mixed, null).HeavyKeyPath);
            Assert.AreEqual("<Mouse>/forwardButton", ControlSchemeRules.Build(ControlScheme.Mixed, null).HeavyPointerPath);
            Assert.IsEmpty(ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null).HeavyPointerPath);
            Assert.IsEmpty(ControlSchemeRules.Build(ControlScheme.MouseOnly, null).HeavyKeyPath);
        }

        [Test]
        public void CustomHeavyGoesToTheKeyOrThePointerPath()
        {
            var custom = CustomControls.Default;
            custom.Heavy = ControlChoices.IndexOf("<Mouse>/backButton");
            var layout = ControlSchemeRules.Build(ControlScheme.Custom, custom);
            Assert.AreEqual("<Mouse>/backButton", layout.HeavyPointerPath);
            Assert.IsEmpty(layout.HeavyKeyPath);

            custom.Heavy = ControlChoices.IndexOf("<Keyboard>/i");
            layout = ControlSchemeRules.Build(ControlScheme.Custom, custom);
            Assert.AreEqual("<Keyboard>/i", layout.HeavyKeyPath);
            Assert.IsEmpty(layout.HeavyPointerPath);
        }

        [Test]
        public void CustomFromAPresetKeepsTheHeavyBinding()
        {
            var fromMixed = ControlSchemeRules.CustomFrom(ControlSchemeRules.Build(ControlScheme.Mixed, null));
            Assert.AreEqual(ControlChoices.IndexOf("<Keyboard>/u"), fromMixed.Heavy);

            var fromMouse = ControlSchemeRules.CustomFrom(ControlSchemeRules.Build(ControlScheme.MouseOnly, null));
            Assert.AreEqual(ControlChoices.IndexOf("<Mouse>/forwardButton"), fromMouse.Heavy);
        }

        [Test]
        public void HeavyLabelNamesBothBindings()
        {
            Assert.AreEqual("U / Mouse Forward", ControlSchemeRules.HeavyLabel(ControlSchemeRules.Build(ControlScheme.Mixed, null)));
            Assert.AreEqual("U", ControlSchemeRules.HeavyLabel(ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null)));
        }

        [Test]
        public void DescribeListsTheHeavyAttack()
        {
            StringAssert.Contains("Heavy attack</color>    U / Mouse Forward", ControlSchemeRules.Describe(ControlLayout.Default));
        }

        [Test]
        public void DescribeListsTheActiveControls()
        {
            var text = ControlSchemeRules.Describe(ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null));
            StringAssert.Contains("J (aims the way you face)", text);
            StringAssert.Contains("Slice</color>    off", text);
        }
    }
}
