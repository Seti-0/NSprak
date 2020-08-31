using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Expressions.Patterns.Steps;
using NSprak.Expressions.Types;
using NSprak.Tokens;

namespace NSprak.Expressions.Patterns
{
    public static class PatternBuilder
    {
        public static Pattern SplitAndAddEntryPoints(this Pattern pattern, BuilderStep step)
        {
            pattern.EntryPoints.AddRange(step.Root.Options);
            return pattern;
        }

        public static Pattern AddEntryPoints(this Pattern pattern, params BuilderStep[] entryPoints)
        {
            pattern.EntryPoints.AddRange(entryPoints.Select(x => x.Root));
            return pattern;
        }

        public static Pattern AllowEmpty(this Pattern pattern)
        {
            pattern.AllowEmpty = true;
            return pattern;
        }

        public static BuilderStep Dummy()
        {
            return new BuilderStep(new DummyStep());
        }

        public static BuilderStep Element(Predicate<Token> predicate, string name)
        {
            PatternStep step = new TokenPredicateStep(predicate, name);
            return new BuilderStep(step);
        }

        public static BuilderStep Element(char symbol)
        {
            string name = "KeySymbol: " + symbol;
            return Element(x => x.IsKeySymbol(symbol), name);
        }

        public static BuilderStep Element(string keyWord)
        {
            string name = "KeyWord: " + keyWord;
            return Element(x => x.IsKeyWord(keyWord), name);
        }

        public static BuilderStep Element(TokenType type)
        {
            string name = "Token: " + Enum.GetName(typeof(TokenType), type);
            return Element(x => x.Type == type, name);
        }

        public static BuilderStep Element(Pattern pattern)
        {
            return new BuilderStep(new SubPatternStep(pattern));
        }

        public static BuilderStep Array(BuilderStep start, BuilderStep element, BuilderStep delim, BuilderStep end)
        {
            return new BuilderStep(null).Array(start, element, delim, end);
        }
    }

    public class BuilderStep
    {
        public PatternStep Latest { get; private set; }
        public PatternStep Root { get; private set; }

        public bool Ended { get; private set; }

        public BuilderStep(PatternStep root)
        {
            Root = root;
            Latest = root;
        }

        private void SetLatest(PatternStep step)
        {
            Latest = step;
            if (Root == null)
                Root = Latest;
        }

        private void SetLatest(BuilderStep step)
        {
            SetLatest(step.Root);
        }

        private void AddOption(BuilderStep step)
        {
            if (Ended)
                throw new InvalidOperationException("Builder step ended, no new options can be added");

            if (Latest == null)
            {
                Latest = step.Root;
                Root = step.Root;
            }
            else 
                Latest.Options.Add(step.Root);
        }

        private void AddOptions(params BuilderStep[] steps)
        {
            if (Ended)
                throw new InvalidOperationException("Builder step ended, no new options can be added");

            Latest.Options.AddRange(steps.Select(x => x.Root));
        }

        private void NextStep(PatternStep nextStep)
        {
            Latest.Options.Add(nextStep);
            SetLatest(nextStep);
        }

        public BuilderStep Break()
        {
            Latest.DebuggerBreak = true;
            return this;
        }

        public BuilderStep Fork(params BuilderStep[] steps)
        {
            AddOptions(steps);
            return this;
        }

        public BuilderStep Join(params BuilderStep[] steps)
        {
            AddOptions(steps);
            SetLatest(new DummyStep());

            foreach (BuilderStep step in steps)
            {
                step.Latest.Options.Add(Latest);
            }

            return this;
        }

        internal BuilderStep[] End(Func<MatchIterator, string, Command> create)
        {
            throw new NotImplementedException();
        }

        public BuilderStep Array(BuilderStep start, BuilderStep element, BuilderStep delim, BuilderStep end)
        {
            start.Latest.Options.Add(end.Root);
            start.Latest.Options.Add(element.Root);

            element.Latest.Options.Add(delim.Root);
            element.Latest.Options.Add(end.Root);

            delim.Latest.Options.Add(element.Root);

            AddOption(start);
            SetLatest(end);

            return this;
        }

        public BuilderStep AllowEnd(EndStep endStep)
        {
            Latest.AllowEnd = true;
            Latest.EndStep = endStep;
            return this;
        }

        public BuilderStep End(EndStep endStep)
        {
            Ended = true;
            return AllowEnd(endStep);
        }

        public BuilderStep EndOfLine(EndStep endStep)
        {
            Latest.RequireEnd = true;
            return End(endStep);
        }

        public BuilderStep AllowLoopback()
        {
            Latest.AllowLoopback = true;
            return this;
        }

        public BuilderStep Then(char symbol)
        {
            string name = "KeySymbol: " + symbol;
            NextStep(new TokenPredicateStep(x => x.IsKeySymbol(symbol), name));
            return this;
        }

        public BuilderStep Then(string keyword)
        {
            string name = "KeyWord: " + keyword;
            NextStep(new TokenPredicateStep(x => x.IsKeyWord(keyword), name));
            return this;
        }

        public BuilderStep Then(TokenType type)
        {
            string name = "Token: " + Enum.GetName(typeof(TokenType), type);
            NextStep(new TokenPredicateStep(x => x.Type == type, name));
            return this;
        }

        public BuilderStep Then(Pattern pattern)
        {
            NextStep(new SubPatternStep(pattern));
            return this;
        }
    }
}
