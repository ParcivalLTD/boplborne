using GraceRun.Core.Settings;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ControlSchemeRulesTests
    {
        private static bool UsesKeys(ControlLayout layout)
        {
            if (layout.AttackPath.Length > 0 && ControlChoices.IsKey(layout.AttackPath)) return true;
            if (layout.HeavyKeyPath.Length > 0) return true;
            foreach (var key in layout.KeyPaths) if (key.Length > 0) return true;
            return false;
        }

        private static bool UsesMouse(ControlLayout layout)
        {
            if (ControlChoices.IsPointer(layout.AttackPath) || layout.HeavyPointerPath.Length > 0) return true;
            foreach (var pointer in layout.PointerPaths) if (pointer.Length > 0) return true;
            return false;
        }

        [Test]
        public void KeyboardOnlyHasNoMouseAndNoSlice()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null);
            Assert.AreEqual("<Keyboard>/j", layout.AttackPath);
            Assert.AreEqual("<Keyboard>/k", layout.KeyPaths[0]);
            Assert.AreEqual("<Keyboard>/semicolon", layout.KeyPaths[2]);
            Assert.IsFalse(UsesMouse(layout));
            Assert.AreEqual(-1, layout.SliceSlot);
            Assert.IsFalse(layout.CursorAimKeys);
        }

        [Test]
        public void MouseOnlyHasNoKeysAndSlicesOnTheLeftButton()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.MouseOnly, null);
            Assert.IsEmpty(layout.AttackPath);
            Assert.IsFalse(UsesKeys(layout));
            Assert.AreEqual("<Mouse>/leftButton", layout.PointerPaths[0]);
            Assert.AreEqual(0, layout.SliceSlot);
        }

        [Test]
        public void NoSchemeMixesKeyboardAndMouse()
        {
            foreach (var scheme in new[] { ControlScheme.KeyboardOnly, ControlScheme.MouseOnly, ControlScheme.Custom })
            {
                var layout = ControlSchemeRules.Build(scheme, null);
                Assert.IsFalse(UsesKeys(layout) && UsesMouse(layout), scheme.ToString());
            }
            Assert.IsFalse(UsesMouse(ControlLayout.Default));
        }

        [Test]
        public void CustomDropsChoicesFromTheOtherDevice()
        {
            var custom = CustomControls.Default;
            custom.Abilities[1] = ControlChoices.IndexOf("<Mouse>/backButton");
            var layout = ControlSchemeRules.Build(ControlScheme.Custom, custom);
            Assert.IsEmpty(layout.KeyPaths[1]);
            Assert.IsEmpty(layout.PointerPaths[1]);
            Assert.IsFalse(UsesKeys(layout) && UsesMouse(layout));

            custom = CustomControls.DefaultFor(true);
            custom.Abilities[0] = ControlChoices.IndexOf("<Keyboard>/q");
            layout = ControlSchemeRules.Build(ControlScheme.Custom, custom);
            Assert.IsEmpty(layout.KeyPaths[0]);
            Assert.IsEmpty(layout.PointerPaths[0]);
        }

        [Test]
        public void CustomMouseDeviceUsesPointerPathsAndSlices()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.Custom, CustomControls.DefaultFor(true));
            Assert.AreEqual("<Mouse>/leftButton", layout.PointerPaths[0]);
            Assert.AreEqual("<Mouse>/forwardButton", layout.HeavyPointerPath);
            Assert.AreEqual(0, layout.SliceSlot);
            Assert.IsFalse(UsesKeys(layout));
        }

        [Test]
        public void CustomSliceNeedsTheMouseDeviceAndThatAbilityOnAButton()
        {
            var custom = CustomControls.Default;
            custom.Slice = 0;
            Assert.AreEqual(-1, ControlSchemeRules.Build(ControlScheme.Custom, custom).SliceSlot);

            custom = CustomControls.DefaultFor(true);
            custom.Abilities[0] = 0;
            Assert.AreEqual(-1, ControlSchemeRules.Build(ControlScheme.Custom, custom).SliceSlot);
        }

        [Test]
        public void CustomAttackCanBeNone()
        {
            var custom = CustomControls.Default;
            custom.Attack = 0;
            Assert.IsEmpty(ControlSchemeRules.Build(ControlScheme.Custom, custom).AttackPath);
        }

        [Test]
        public void SwitchingDeviceStartsThatDevicesDefaults()
        {
            var custom = CustomControls.Default;
            custom.SetMouse(true);
            Assert.IsTrue(custom.Mouse);
            Assert.AreEqual(ControlChoices.IndexOf("<Mouse>/leftButton"), custom.Abilities[0]);
            custom.SetMouse(false);
            Assert.AreEqual(ControlChoices.IndexOf("<Keyboard>/k"), custom.Abilities[0]);
        }

        [Test]
        public void CycleChoiceStaysOnTheDevice()
        {
            var index = ControlChoices.IndexOf("<Keyboard>/b");
            for (var i = 0; i < ControlChoices.Count * 2; i++)
            {
                index = ControlSchemeRules.CycleChoice(index, 1, false);
                Assert.IsTrue(ControlChoices.FitsDevice(index, false));
            }
            index = ControlChoices.IndexOf("<Mouse>/leftButton");
            for (var i = 0; i < ControlChoices.Count * 2; i++)
            {
                index = ControlSchemeRules.CycleChoice(index, -1, true);
                Assert.IsTrue(ControlChoices.FitsDevice(index, true));
            }
        }

        [Test]
        public void CustomFromAPresetKeepsItsDeviceAndBindings()
        {
            var fromKeys = ControlSchemeRules.CustomFrom(ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null));
            Assert.IsFalse(fromKeys.Mouse);
            Assert.AreEqual(ControlChoices.IndexOf("<Keyboard>/k"), fromKeys.Abilities[0]);
            Assert.AreEqual(ControlChoices.IndexOf("<Keyboard>/u"), fromKeys.Heavy);

            var fromMouse = ControlSchemeRules.CustomFrom(ControlSchemeRules.Build(ControlScheme.MouseOnly, null));
            Assert.IsTrue(fromMouse.Mouse);
            Assert.AreEqual(ControlChoices.IndexOf("<Mouse>/forwardButton"), fromMouse.Heavy);
            var layout = ControlSchemeRules.Build(ControlScheme.Custom, fromMouse);
            Assert.IsEmpty(layout.AttackPath);
            Assert.AreEqual("<Mouse>/leftButton", layout.PointerPaths[0]);
            Assert.AreEqual(0, layout.SliceSlot);
        }

        [Test]
        public void EverySchemeBindsTheHeavyAttack()
        {
            foreach (var scheme in new[] { ControlScheme.KeyboardOnly, ControlScheme.MouseOnly, ControlScheme.Custom })
            {
                var layout = ControlSchemeRules.Build(scheme, null);
                Assert.IsTrue(layout.HeavyKeyPath.Length > 0 || layout.HeavyPointerPath.Length > 0, scheme.ToString());
            }
            Assert.AreEqual("<Keyboard>/u", ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null).HeavyKeyPath);
            Assert.AreEqual("<Mouse>/forwardButton", ControlSchemeRules.Build(ControlScheme.MouseOnly, null).HeavyPointerPath);
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
        public void LabelsNameTheBindings()
        {
            var layout = ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null);
            Assert.AreEqual("K", ControlSchemeRules.AbilityLabel(layout, 0));
            Assert.AreEqual("J", ControlSchemeRules.AttackLabel(layout));
            Assert.AreEqual("U", ControlSchemeRules.HeavyLabel(layout));
            Assert.AreEqual("None", ControlSchemeRules.AttackLabel(ControlSchemeRules.Build(ControlScheme.MouseOnly, null)));
        }

        [Test]
        public void DescribeListsTheActiveControls()
        {
            var text = ControlSchemeRules.Describe(ControlSchemeRules.Build(ControlScheme.KeyboardOnly, null));
            StringAssert.Contains("J (aims the way you face)", text);
            StringAssert.Contains("Slice</color>    off", text);
            StringAssert.Contains("Heavy attack</color>    U", text);
        }
    }
}
