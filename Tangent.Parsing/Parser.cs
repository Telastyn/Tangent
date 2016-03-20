using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Parsing.Errors;
using Tangent.Tokenization;

namespace Tangent.Parsing
{
    public static class Parser
    {
        public static Parser<T> Options<T>(string labelWhenNotFound, params Parser<T>[] options)
        {
            return new ShortcuttingOrParser<T>(options, labelWhenNotFound);
        }

        public static Parser<T> Combine<A, B, T>(Parser<A> a, Parser<B> b, Func<A, B, T> selector)
        {
            return new CombinedParser<A, B, T>(a, b, selector);
        }

        public static Parser<T> Combine<A, B, C, T>(Parser<A> a, Parser<B> b, Parser<C> c, Func<A, B, C, T> selector)
        {
            return new CombinedParser<A, B, C, T>(a, b, c, selector);
        }

        public static Parser<T> Combine<A, B, C, D, T>(Parser<A> a, Parser<B> b, Parser<C> c, Parser<D> d, Func<A, B, C, D, T> selector)
        {
            return new CombinedParser<A, B, C, D, T>(a, b, c, d, selector);
        }

        public static Parser<T> Combine<A, B, C, D, E, T>(Parser<A> a, Parser<B> b, Parser<C> c, Parser<D> d, Parser<E> e, Func<A, B, C, D, E, T> selector)
        {
            return new CombinedParser<A, B, C, D, E, T>(a, b, c, d, e, selector);
        }

        public static Parser<IEnumerable<T>> Delimited<D, T>(Parser<D> delimiting, Parser<T> meaningful, bool requiresOne = true, bool optionalTrailingDelimiter = false)
        {
            return new DelimitedParser<T, D>(delimiting, meaningful, requiresOne, optionalTrailingDelimiter);
        }

        public static Parser<T> Difference<T, D>(Parser<T> production, Parser<D> difference)
        {
            return new DifferenceParser<T, D>(production, difference);
        }

        public static Parser<T> Delegate<T>(Func<Parser<T>> wrapped)
        {
            return new DelegatingParser<T>(wrapped);
        }

        public static Parser<T> NotFollowedBy<T, U>(Parser<T> meaningful, Parser<U> peekToAvoid, string whenAvoidingMessage)
        {
            return new NotFollowedByParser<T, U>(meaningful, peekToAvoid, whenAvoidingMessage);
        }
    }

    public abstract class Parser<T>
    {
        public abstract ResultOrParseError<T> Parse(IEnumerable<Token> tokens, out int consumed);

        public Parser<T> Maybe
        {
            get
            {
                return new OptionalParser<T>(this);
            }
        }

        public RepeatingParser<T> ZeroOrMore
        {
            get
            {
                return new RepeatingParser<T>(this, false);
            }
        }

        public RepeatingParser<T> OneOrMore
        {
            get
            {
                return new RepeatingParser<T>(this, true);
            }
        }

        public ShortcuttingOrParser<T> Or(Parser<T> other, string labelWhenNotFound)
        {
            return new ShortcuttingOrParser<T>(new[] { this, other }, labelWhenNotFound);
        }

        public Parser<U> Select<U>(Func<T, U> selector)
        {
            return new SelectingParser<T, U>(this, selector);
        }
    }
}
