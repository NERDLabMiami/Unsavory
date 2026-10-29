using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Unsavory.Toasts.Tests
{
    public class RuleEvaluatorTests
    {
        static Dictionary<string, string> When(params string[] pairs)
        {
            var d = new Dictionary<string, string>();
            for (int i = 0; i < pairs.Length; i += 2) d[pairs[i]] = pairs[i + 1];
            return d;
        }

        static NarrativeContext Ctx(params object[] pairs)
        {
            var c = new NarrativeContext();
            for (int i = 0; i < pairs.Length; i += 2)
            {
                string key = (string)pairs[i];
                object v = pairs[i + 1];
                if (v is string) c.Set(key, (string)v);
                else if (v is bool) c.Set(key, (bool)v);
                else c.Set(key, System.Convert.ToDouble(v));
            }
            return c;
        }

        [TestCase("==1", 1.0, true)]
        [TestCase("==1", 0.0, false)]
        [TestCase("==0", 0.0, true)]
        [TestCase(">=11", 11.0, true)]
        [TestCase(">=11", 10.0, false)]
        [TestCase("<30", 29.9, true)]
        [TestCase("<30", 30.0, false)]
        [TestCase(">0", 1.0, true)]
        [TestCase(">0", 0.0, false)]
        [TestCase("<=2", 2.0, true)]
        [TestCase("<=2", 3.0, false)]
        [TestCase("!=3", 2.0, true)]
        [TestCase("5", 5.0, true)]
        [TestCase(" >= 4 ", 4.0, true)]
        public void NumericComparisons(string expr, double actual, bool expected)
        {
            Assert.AreEqual(expected, RuleEvaluator.Compare(expr, actual));
        }

        [Test]
        public void StringValuesUseExactEquality()
        {
            Assert.IsTrue(RuleEvaluator.Compare("insurance", "insurance"));
            Assert.IsFalse(RuleEvaluator.Compare("insurance", "payment"));
            Assert.IsFalse(RuleEvaluator.Compare("insurance", "Insurance"));
            Assert.IsTrue(RuleEvaluator.Compare("!=insurance", "payment"));
        }

        [Test]
        public void NumbersAndStringsNeverCrossMatch()
        {
            Assert.IsFalse(RuleEvaluator.Compare("==1", "1"));
            Assert.IsFalse(RuleEvaluator.Compare("insurance", 1.0));
        }

        [Test]
        public void MalformedConditionsDoNotMatchAndAreReported()
        {
            var errors = new List<string>();
            Diagnostics.Error = errors.Add;
            try
            {
                Assert.IsFalse(RuleEvaluator.Compare(">=", 5.0));
                Assert.IsFalse(RuleEvaluator.Compare("", 5.0));
                Assert.IsFalse(RuleEvaluator.Compare(">insurance", "insurance"));
            }
            finally { Diagnostics.Error = null; }
            Assert.AreEqual(3, errors.Count);
        }

        [Test]
        public void MissingKeyIsANonMatchNotZero()
        {
            var ctx = Ctx("day", 3);
            Assert.IsFalse(RuleEvaluator.Matches(When("fruit_count", "==0"), ctx));
            Assert.IsFalse(RuleEvaluator.Matches(When("fruit_count", "<=2"), ctx));
            Assert.IsTrue(RuleEvaluator.Matches(When("day", "==3"), ctx));
        }

        [Test]
        public void ExplicitZeroAndFalseMatchEqualsZero()
        {
            var ctx = Ctx("fruit_count", 0, "catering", false);
            Assert.IsTrue(RuleEvaluator.Matches(When("fruit_count", "==0", "catering", "==0"), ctx));
        }

        [Test]
        public void AllPredicatesAreAnded()
        {
            var ctx = Ctx("day", 12, "health", 40);
            Assert.IsTrue(RuleEvaluator.Matches(When("day", ">=11", "health", ">=30"), ctx));
            Assert.IsFalse(RuleEvaluator.Matches(When("day", ">=11", "health", "<30"), ctx));
        }

        [Test]
        public void MissingWhenMatchesEverything()
        {
            Assert.IsTrue(RuleEvaluator.Matches(null, Ctx()));
            Assert.IsTrue(RuleEvaluator.Matches(new Dictionary<string, string>(), Ctx()));
        }

        [Test]
        public void FirstMatchWinsInFileOrder()
        {
            var rules = new List<Rule>
            {
                new Rule { Id = "a", When = When("day", ">=5"), Text = "a" },
                new Rule { Id = "b", When = When("day", ">=1"), Text = "b" },
                new Rule { Id = "c", Text = "c" },
            };
            Assert.AreEqual("a", RuleEvaluator.First(rules, Ctx("day", 9)).Id);
            Assert.AreEqual("b", RuleEvaluator.First(rules, Ctx("day", 2)).Id);
            Assert.AreEqual("c", RuleEvaluator.First(rules, Ctx("day", 0)).Id);
        }

        [Test]
        public void OnceRulesAreSkippedAfterTheyWereShown()
        {
            var rules = new List<Rule>
            {
                new Rule { Id = "intro", Once = "run", Text = "intro" },
                new Rule { Id = "after", Text = "after" },
            };
            Assert.AreEqual("intro", RuleEvaluator.First(rules, Ctx(), id => false).Id);
            Assert.AreEqual("after", RuleEvaluator.First(rules, Ctx(), id => id == "intro").Id);
        }

        [Test]
        public void SpecialIsExclusiveAndWorldThenStateOtherwise()
        {
            var channels = new EntryChannels();
            channels.Special.Add(new Rule { Id = "sp", When = When("first_visit", "==1"), Text = "sp" });
            channels.World.Add(new Rule { Id = "w", Text = "w" });
            channels.State.Add(new Rule { Id = "s", When = When("missed_bills", ">=1"), Text = "s" });

            var special = RuleEvaluator.SelectEntry(channels, Ctx("first_visit", 1, "missed_bills", 2));
            Assert.AreEqual(new[] { "sp" }, Ids(special));

            var both = RuleEvaluator.SelectEntry(channels, Ctx("first_visit", 0, "missed_bills", 2));
            Assert.AreEqual(new[] { "w", "s" }, Ids(both));

            var worldOnly = RuleEvaluator.SelectEntry(channels, Ctx("first_visit", 0, "missed_bills", 0));
            Assert.AreEqual(new[] { "w" }, Ids(worldOnly));
        }

        [Test]
        public void SpecialAlreadyShownFallsBackToWorldAndState()
        {
            var channels = new EntryChannels();
            channels.Special.Add(new Rule { Id = "sp", Once = "run", Text = "sp" });
            channels.World.Add(new Rule { Id = "w", Text = "w" });
            Assert.AreEqual(new[] { "w" }, Ids(RuleEvaluator.SelectEntry(channels, Ctx(), id => id == "sp")));
        }

        [Test]
        public void InterpolationFillsKnownNamesAndWarnsOnUnknown()
        {
            var warnings = new List<string>();
            Diagnostics.Warn = warnings.Add;
            try
            {
                var ctx = Ctx("missed_bills", 2, "pill_price", "$5");
                Assert.AreEqual("You have 2 overdue bill(s).", RuleEvaluator.Interpolate("You have {missed_bills} overdue bill(s).", ctx, "r"));
                Assert.AreEqual("This will cost $5.", RuleEvaluator.Interpolate("This will cost {pill_price}.", ctx, "r"));
                Assert.AreEqual("Cost {nope}.", RuleEvaluator.Interpolate("Cost {nope}.", ctx, "r"));
                // Expressions are never evaluated.
                Assert.AreEqual("{missed_bills+1}", RuleEvaluator.Interpolate("{missed_bills+1}", ctx, "r"));
            }
            finally { Diagnostics.Warn = null; }
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void MessagesPreferLinesAndDropEmpty()
        {
            var rule = new Rule { Id = "x", Lines = new List<string> { "one", "", "two" }, Text = "ignored" };
            Assert.AreEqual(new[] { "one", "two" }, RuleEvaluator.Messages(rule, Ctx()).ToArray());
        }

        static string[] Ids(List<Rule> rules)
        {
            var ids = new string[rules.Count];
            for (int i = 0; i < ids.Length; i++) ids[i] = rules[i].Id;
            return ids;
        }
    }

    // The shipped toasts.txt (schema v3) against the acceptance scenarios.
    public class NarrativeDataTests
    {
        static NarrativeDocument _doc;

        static NarrativeDocument Doc
        {
            get
            {
                if (_doc != null) return _doc;
                string path = Path.Combine(Application.dataPath, "Resources", "toasts.txt");
                _doc = NarrativeParser.Parse(File.ReadAllText(path));
                return _doc;
            }
        }

        // A mid-run, healthy morning with nothing wrong; tests override what they care about.
        static NarrativeContext Ctx(params object[] overrides)
        {
            var values = new Dictionary<string, object>
            {
                { "day", 3 }, { "run_end", 0 }, { "first_visit", false }, { "rent_pending", false },
                { "health", 60 }, { "money", 100 }, { "pills", 0 }, { "pill_price", "$5" }, { "can_afford_pills", true },
                { "missed_bills", 0 }, { "bill_due", 0 }, { "unread_mail", 0 }, { "phone_service", true },
                { "car_ok", true }, { "late", 0 }, { "fruit_count", 5 }, { "tutorial_done", true }, { "catering", false },
                { "acquisition_known", false }, { "new_vision_known", false }, { "skipped_previous_shift", false },
                { "insurance_lapsed", false }, { "crashed_commute", false },
            };
            for (int i = 0; i < overrides.Length; i += 2) values[(string)overrides[i]] = overrides[i + 1];
            var c = new NarrativeContext();
            foreach (var kv in values)
            {
                if (kv.Value == null) continue;           // null removes the key
                if (kv.Value is string) c.Set(kv.Key, (string)kv.Value);
                else if (kv.Value is bool) c.Set(kv.Key, (bool)kv.Value);
                else c.Set(kv.Key, System.Convert.ToDouble(kv.Value));
            }
            return c;
        }

        static string[] Entry(string kind, NarrativeContext ctx, System.Func<string, bool> shown = null)
        {
            var rules = RuleEvaluator.SelectEntry(Doc.SceneEntry[kind], ctx, shown);
            var ids = new string[rules.Count];
            for (int i = 0; i < ids.Length; i++) ids[i] = rules[i].Id;
            return ids;
        }

        static string Id(List<Rule> rules, NarrativeContext ctx)
        {
            var rule = RuleEvaluator.First(rules, ctx);
            return rule == null ? null : rule.Id;
        }

        static Rule Modal(string key, NarrativeContext ctx) { return RuleEvaluator.First(Doc.Modals[key], ctx); }

        static bool Enabled(Rule modal, string action, NarrativeContext ctx)
        {
            foreach (var c in modal.Choices) if (c.Action == action) return RuleEvaluator.IsEnabled(c, ctx, modal.Id);
            return false;
        }

        static bool Offers(Rule modal, string action)
        {
            foreach (var c in modal.Choices) if (c.Action == action) return true;
            return false;
        }

        [Test]
        public void DataLoadsAndValidatesWithoutErrors()
        {
            var errors = NarrativeValidator.Validate(Doc);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void ValidatorCatchesBrokenData()
        {
            var doc = NarrativeParser.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "Resources", "toasts.txt")));
            doc.SchemaVersion = 2;
            doc.Hover["phone"].Add(new Rule { Id = doc.Hover["phone"][0].Id, Text = "dup" });
            doc.Hover["bed"].Add(new Rule { Id = "bad.when", When = new Dictionary<string, string> { { "day", ">=" } }, Text = "x" });
            doc.Hover["door"].Add(new Rule { Id = "no.text" });
            doc.Modals["bed"].Add(new Rule
            {
                Id = "bad.modal", Title = "t", Text = "x",
                Choices = new List<Choice> { new Choice { Label = "Go", Action = "fly" } },
            });
            var errors = string.Join("\n", NarrativeValidator.Validate(doc));
            StringAssert.Contains("schema_version", errors);
            StringAssert.Contains("duplicate id", errors);
            StringAssert.Contains("bad.when", errors);
            StringAssert.Contains("no.text", errors);
            StringAssert.Contains("unknown action 'fly'", errors);
            StringAssert.Contains("no cancel choice", errors);
        }

        [Test]
        public void FirstVisitPlaysTheIntroAloneAndOnlyOnce()
        {
            var ctx = Ctx("first_visit", true, "day", 1, "missed_bills", 3);
            Assert.AreEqual(new[] { "home.first" }, Entry("home", ctx));
            Assert.AreEqual(new[] { "home.world.week1", "home.state.overdue" }, Entry("home", ctx, id => id == "home.first"));
        }

        [Test]
        public void HomeEntryShowsWorldThenState()
        {
            var ctx = Ctx("day", 6, "missed_bills", 2);
            Assert.AreEqual(new[] { "home.world.day6", "home.state.overdue" }, Entry("home", ctx));
            var messages = RuleEvaluator.Messages(RuleEvaluator.First(Doc.SceneEntry["home"].State, ctx), ctx);
            Assert.AreEqual("You have 2 overdue bill(s).", messages[0]);
        }

        [Test]
        public void StateChannelDoesNotMaskTheWorldBeat()
        {
            var ctx = Ctx("day", 11, "missed_bills", 1, "health", 20, "rent_pending", true);
            var ids = Entry("home", ctx);
            Assert.AreEqual("home.world.day11", ids[0]);
            Assert.AreEqual("home.state.rent", ids[1]);
        }

        [Test]
        public void SkippedShiftHandoffShowsOnTheNextMorning()
        {
            Assert.AreEqual(new[] { "home.world.week1", "home.state.skipped" }, Entry("home", Ctx("skipped_previous_shift", true)));
            Assert.AreEqual(new[] { "home.world.week1" }, Entry("home", Ctx()));
        }

        [Test]
        public void EndingEntriesPointAtTheEmailWithoutSpoilingIt()
        {
            var layoff = Ctx("run_end", 1, "day", 21);
            Assert.AreEqual(new[] { "home.end.layoff" }, Entry("home", layoff));
            var text = string.Join(" ", RuleEvaluator.Messages(RuleEvaluator.First(Doc.SceneEntry["home"].Special, layoff), layoff)).ToLowerInvariant();
            StringAssert.Contains("email", text);
            StringAssert.DoesNotContain("laid off", text);
            StringAssert.DoesNotContain("layoff", text);
            StringAssert.DoesNotContain("fired", text);

            Assert.AreEqual(new[] { "home.end.eviction" }, Entry("home", Ctx("run_end", 2)));
        }

        [TestCase(1), TestCase(5), TestCase(6), TestCase(10), TestCase(11), TestCase(15), TestCase(16), TestCase(19), TestCase(20)]
        public void EveryWorkdayHasAHomeAndWorkFallback(int day)
        {
            var ctx = Ctx("day", day);
            Assert.GreaterOrEqual(Entry("home", ctx).Length, 1, "home day " + day);
            Assert.GreaterOrEqual(Entry("work", ctx).Length, 1, "work day " + day);
        }

        [TestCase(1, "home.world.week1")]
        [TestCase(5, "home.world.week1")]
        [TestCase(6, "home.world.day6")]
        [TestCase(7, "home.world.week2")]
        [TestCase(10, "home.world.week2")]
        [TestCase(11, "home.world.day11")]
        [TestCase(15, "home.world.week3")]
        [TestCase(16, "home.world.day16")]
        [TestCase(19, "home.world.week4")]
        [TestCase(20, "home.world.day20")]
        public void HomeWorldBeatsFollowTheCalendarWithoutKnowledge(int day, string expected)
        {
            Assert.AreEqual(expected, Entry("home", Ctx("day", day))[0]);
        }

        [Test]
        public void StoryKnowledgeIsNeverInferredFromTheDay()
        {
            foreach (int day in new[] { 16, 17, 18, 19 })
            {
                var home = Entry("home", Ctx("day", day))[0];
                Assert.IsFalse(home.Contains("acquisition") || home.Contains("vision"), "home day " + day + ": " + home);
                var work = Entry("work", Ctx("day", day))[0];
                Assert.IsFalse(work.Contains("acquisition") || work.Contains("vision"), "work day " + day + ": " + work);
            }
            Assert.AreEqual("home.world.acquisition", Entry("home", Ctx("day", 17, "acquisition_known", true))[0]);
            Assert.AreEqual("home.world.vision", Entry("home", Ctx("day", 19, "acquisition_known", true, "new_vision_known", true))[0]);
            Assert.AreEqual("work.world.vision", Entry("work", Ctx("day", 18, "new_vision_known", true))[0]);
            Assert.AreEqual("work.world.acquisition", Entry("work", Ctx("day", 17, "acquisition_known", true))[0]);
        }

        [Test]
        public void UnavailableKnowledgeFlagsFailSafely()
        {
            var ctx = Ctx("day", 17, "acquisition_known", null, "new_vision_known", null);
            Assert.AreEqual("home.world.week4", Entry("home", ctx)[0]);
        }

        [Test]
        public void WorkTutorialIsSpecialAndCateringSuppressesOrdinaryLines()
        {
            Assert.AreEqual(new[] { "work.tutorial" }, Entry("work", Ctx("tutorial_done", false, "day", 1)));
            Assert.AreEqual(new string[0], Entry("work", Ctx("catering", true)));
            Assert.AreEqual(new string[0], Entry("work", Ctx("catering", true, "late", 2, "health", 10)));
            Assert.AreEqual(new[] { "work.world.day3" }, Entry("work", Ctx("day", 3)));
        }

        [Test]
        public void WorkStateReflectsRealLatenessAndIllness()
        {
            Assert.AreEqual(new[] { "work.world.day3", "work.state.late" }, Entry("work", Ctx("late", 1)));
            Assert.AreEqual(new[] { "work.world.day3", "work.state.sick" }, Entry("work", Ctx("health", 29)));
            Assert.AreEqual(new[] { "work.world.day3" }, Entry("work", Ctx("health", 30)));
        }

        [Test]
        public void SickThresholdIsExactlyThirty()
        {
            Assert.AreEqual("bed.modal.default", Modal("bed", Ctx("health", 30)).Id);
            Assert.AreEqual("bed.modal.sick", Modal("bed", Ctx("health", 29.9)).Id);
            Assert.AreEqual("bed.modal.sick.rent", Modal("bed", Ctx("health", 10, "rent_pending", true)).Id);
            Assert.AreEqual("bed.sick", Id(Doc.Hover["bed"], Ctx("health", 20)));
            Assert.AreEqual("bed.early", Id(Doc.Hover["bed"], Ctx("health", 30)));
        }

        [Test]
        public void BedOffersOnlySkipShiftAndCancel()
        {
            foreach (var rule in Doc.Modals["bed"])
                foreach (var choice in rule.Choices)
                    Assert.IsTrue(choice.Action == "skip_shift" || choice.Action == "cancel", rule.Id + " offers " + choice.Action);
        }

        static string[] Actions(Rule modal)
        {
            var actions = new List<string>();
            foreach (var c in modal.Choices) actions.Add(c.Action);
            return actions.ToArray();
        }

        [Test]
        public void DoorWithAUsableCarOffersDriveOrCancelOnly()
        {
            var ctx = Ctx();
            var modal = Modal("door", ctx);
            Assert.AreEqual("door.modal.default", modal.Id);
            Assert.AreEqual(new[] { "drive_to_work", "cancel" }, Actions(modal));
            Assert.IsTrue(Enabled(modal, "drive_to_work", ctx));

            var sick = Ctx("health", 10);
            Assert.AreEqual("door.modal.sick", Modal("door", sick).Id);
            Assert.AreEqual(new[] { "drive_to_work", "cancel" }, Actions(Modal("door", sick)));
        }

        [Test]
        public void RepossessedCarOffersPublicTransitOnly()
        {
            var ctx = Ctx("car_ok", false, "car_unavailable_reason", "payment");
            var modal = Modal("door", ctx);
            Assert.AreEqual("door.modal.payment", modal.Id);
            StringAssert.Contains("repossessed", modal.Text);
            Assert.AreEqual(new[] { "transit_to_work", "cancel" }, Actions(modal));
            Assert.AreEqual("door.car.payment", Id(Doc.Hover["door"], ctx));
        }

        [Test]
        public void WreckedCarOffersPublicTransitOnly()
        {
            var ctx = Ctx("car_ok", false, "car_unavailable_reason", "accident");
            Assert.AreEqual("door.modal.accident", Modal("door", ctx).Id);
            Assert.AreEqual(new[] { "transit_to_work", "cancel" }, Actions(Modal("door", ctx)));
            Assert.AreEqual("door.car.accident", Id(Doc.Hover["door"], ctx));

            var sick = Ctx("car_ok", false, "car_unavailable_reason", "accident", "health", 10);
            Assert.AreEqual("door.modal.accident.sick", Modal("door", sick).Id);

            var unknownReason = Ctx("car_ok", false, "car_unavailable_reason", null);
            Assert.AreEqual("door.modal.unavailable", Modal("door", unknownReason).Id);
        }

        [Test]
        public void OverdueInsuranceStillOffersDrivingAndOnlyForeshadowsOnHover()
        {
            var ctx = Ctx("insurance_lapsed", true);
            var modal = Modal("door", ctx);
            Assert.AreEqual("door.modal.default", modal.Id);
            Assert.AreEqual(new[] { "drive_to_work", "cancel" }, Actions(modal));
            Assert.AreEqual("door.car.insurance", Id(Doc.Hover["door"], ctx));
        }

        [Test]
        public void DriveIsDisabledWithoutACarEvenIfAModalOffersIt()
        {
            var modal = Modal("door", Ctx());
            var noCar = Ctx("car_ok", false);
            Assert.IsFalse(Enabled(modal, "drive_to_work", noCar));
        }

        [Test]
        public void CrashDayWorkEntryLeadsWithTheCrashLine()
        {
            Assert.AreEqual(new[] { "work.world.day3", "work.state.crash" }, Entry("work", Ctx("crashed_commute", true, "late", 1)));
            Assert.AreEqual(new string[0], Entry("work", Ctx("crashed_commute", true, "catering", true)));
        }

        [Test]
        public void PillsModalBranchesOnStockFundsAndHealth()
        {
            Assert.AreEqual("pills.modal.use.sick", Modal("pills", Ctx("pills", 2, "health", 20)).Id);
            Assert.AreEqual("pills.modal.use.healthy", Modal("pills", Ctx("pills", 2, "health", 50)).Id);
            Assert.AreEqual("pills.modal.buy", Modal("pills", Ctx("pills", 0, "can_afford_pills", true)).Id);
            Assert.AreEqual("pills.modal.broke", Modal("pills", Ctx("pills", 0, "can_afford_pills", false)).Id);
        }

        [Test]
        public void PillsChoicesHonorEnabledWhen()
        {
            var poor = Ctx("pills", 2, "health", 20, "can_afford_pills", false);
            var modal = Modal("pills", poor);
            Assert.IsTrue(Enabled(modal, "take_pill", poor));
            Assert.IsFalse(Enabled(modal, "buy_pills", poor));

            var rich = Ctx("pills", 2, "health", 20, "can_afford_pills", true);
            Assert.IsTrue(Enabled(Modal("pills", rich), "buy_pills", rich));

            var buy = Ctx("pills", 0, "can_afford_pills", true);
            Assert.AreEqual("Another expense. This will cost $5.", RuleEvaluator.Interpolate(Modal("pills", buy).Text, buy, "pills.modal.buy"));
        }

        [TestCase(0, "fruit.empty")]
        [TestCase(1, "fruit.last")]
        [TestCase(2, "fruit.low")]
        [TestCase(3, "fruit.full")]
        [TestCase(5, "fruit.full")]
        public void FruitBowlHoverFollowsTheCount(int count, string expected)
        {
            Assert.AreEqual(expected, Id(Doc.Hover["fruit_bowl"], Ctx("fruit_count", count)));
        }

        [Test]
        public void FruitBowlLaterDayVariants()
        {
            Assert.AreEqual("fruit.empty.late", Id(Doc.Hover["fruit_bowl"], Ctx("fruit_count", 0, "day", 12)));
            Assert.AreEqual("fruit.late", Id(Doc.Hover["fruit_bowl"], Ctx("fruit_count", 4, "day", 16)));
            // Without a fruit count only the unconditional fallback can match.
            Assert.AreEqual("fruit.full", Id(Doc.Hover["fruit_bowl"], Ctx("fruit_count", null)));
        }

        [Test]
        public void PhoneHoverPrefersBillsThenMail()
        {
            Assert.AreEqual("phone.bill", Id(Doc.Hover["phone"], Ctx("bill_due", 1, "unread_mail", 2)));
            Assert.AreEqual("phone.mail", Id(Doc.Hover["phone"], Ctx("unread_mail", 1)));
            Assert.AreEqual("phone.mail.late", Id(Doc.Hover["phone"], Ctx("unread_mail", 1, "day", 16)));
            Assert.AreEqual("phone.early", Id(Doc.Hover["phone"], Ctx()));
        }

        [Test]
        public void PillsHoverVariants()
        {
            Assert.AreEqual("pills.have", Id(Doc.Hover["pills"], Ctx("pills", 1)));
            Assert.AreEqual("pills.have.late", Id(Doc.Hover["pills"], Ctx("pills", 1, "day", 11)));
            Assert.AreEqual("pills.empty.sick", Id(Doc.Hover["pills"], Ctx("pills", 0, "health", 10)));
            Assert.AreEqual("pills.empty", Id(Doc.Hover["pills"], Ctx("pills", 0)));
        }

        [Test]
        public void AfterActionsMatchByHealth()
        {
            Assert.AreEqual("skip.sick", Id(Doc.AfterAction["skip_shift"], Ctx("health", 10)));
            Assert.AreEqual("skip.other", Id(Doc.AfterAction["skip_shift"], Ctx("health", 60)));
            Assert.AreEqual("pills.bought", Id(Doc.AfterAction["buy_pills"], Ctx()));
            Assert.AreEqual("pills.taken", Id(Doc.AfterAction["take_pill"], Ctx()));
        }

        [Test]
        public void ThereIsNoEveningHomeOrWakeEvent()
        {
            Assert.IsFalse(Doc.SceneEntry.ContainsKey("home_wake"));
            foreach (var channels in Doc.SceneEntry.Values)
                foreach (var list in new[] { channels.Special, channels.World, channels.State })
                    AssertNoPhase(list);
            foreach (var list in Doc.Hover.Values) AssertNoPhase(list);
            foreach (var list in Doc.Modals.Values) AssertNoPhase(list);
        }

        static void AssertNoPhase(List<Rule> rules)
        {
            foreach (var rule in rules)
                Assert.IsFalse(rule.When != null && rule.When.ContainsKey("phase"), rule.Id + " still gates on phase");
        }
    }
}
