using System;

namespace Tangent.Runtime {

    public class Variant<A> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        public void Process(Action<A> forA) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        public Variant(R value) {
            Value = value;
            Mode = 18;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        private R Value18
        {
            get
            {
                return (R)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ, Action<R> forR) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                case 18:
                    forR(Value18);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ, Func<R, TReturn> forR) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                case 18:
                    return forR(Value18);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        public Variant(R value) {
            Value = value;
            Mode = 18;
        }

        public Variant(S value) {
            Value = value;
            Mode = 19;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        private R Value18
        {
            get
            {
                return (R)Value;
            }
        }

        private S Value19
        {
            get
            {
                return (S)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ, Action<R> forR, Action<S> forS) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                case 18:
                    forR(Value18);
                    return;
                case 19:
                    forS(Value19);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ, Func<R, TReturn> forR, Func<S, TReturn> forS) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                case 18:
                    return forR(Value18);
                case 19:
                    return forS(Value19);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        public Variant(R value) {
            Value = value;
            Mode = 18;
        }

        public Variant(S value) {
            Value = value;
            Mode = 19;
        }

        public Variant(T value) {
            Value = value;
            Mode = 20;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        private R Value18
        {
            get
            {
                return (R)Value;
            }
        }

        private S Value19
        {
            get
            {
                return (S)Value;
            }
        }

        private T Value20
        {
            get
            {
                return (T)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ, Action<R> forR, Action<S> forS, Action<T> forT) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                case 18:
                    forR(Value18);
                    return;
                case 19:
                    forS(Value19);
                    return;
                case 20:
                    forT(Value20);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ, Func<R, TReturn> forR, Func<S, TReturn> forS, Func<T, TReturn> forT) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                case 18:
                    return forR(Value18);
                case 19:
                    return forS(Value19);
                case 20:
                    return forT(Value20);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        public Variant(R value) {
            Value = value;
            Mode = 18;
        }

        public Variant(S value) {
            Value = value;
            Mode = 19;
        }

        public Variant(T value) {
            Value = value;
            Mode = 20;
        }

        public Variant(U value) {
            Value = value;
            Mode = 21;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        private R Value18
        {
            get
            {
                return (R)Value;
            }
        }

        private S Value19
        {
            get
            {
                return (S)Value;
            }
        }

        private T Value20
        {
            get
            {
                return (T)Value;
            }
        }

        private U Value21
        {
            get
            {
                return (U)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ, Action<R> forR, Action<S> forS, Action<T> forT, Action<U> forU) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                case 18:
                    forR(Value18);
                    return;
                case 19:
                    forS(Value19);
                    return;
                case 20:
                    forT(Value20);
                    return;
                case 21:
                    forU(Value21);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ, Func<R, TReturn> forR, Func<S, TReturn> forS, Func<T, TReturn> forT, Func<U, TReturn> forU) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                case 18:
                    return forR(Value18);
                case 19:
                    return forS(Value19);
                case 20:
                    return forT(Value20);
                case 21:
                    return forU(Value21);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        public Variant(R value) {
            Value = value;
            Mode = 18;
        }

        public Variant(S value) {
            Value = value;
            Mode = 19;
        }

        public Variant(T value) {
            Value = value;
            Mode = 20;
        }

        public Variant(U value) {
            Value = value;
            Mode = 21;
        }

        public Variant(V value) {
            Value = value;
            Mode = 22;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        private R Value18
        {
            get
            {
                return (R)Value;
            }
        }

        private S Value19
        {
            get
            {
                return (S)Value;
            }
        }

        private T Value20
        {
            get
            {
                return (T)Value;
            }
        }

        private U Value21
        {
            get
            {
                return (U)Value;
            }
        }

        private V Value22
        {
            get
            {
                return (V)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ, Action<R> forR, Action<S> forS, Action<T> forT, Action<U> forU, Action<V> forV) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                case 18:
                    forR(Value18);
                    return;
                case 19:
                    forS(Value19);
                    return;
                case 20:
                    forT(Value20);
                    return;
                case 21:
                    forU(Value21);
                    return;
                case 22:
                    forV(Value22);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ, Func<R, TReturn> forR, Func<S, TReturn> forS, Func<T, TReturn> forT, Func<U, TReturn> forU, Func<V, TReturn> forV) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                case 18:
                    return forR(Value18);
                case 19:
                    return forS(Value19);
                case 20:
                    return forT(Value20);
                case 21:
                    return forU(Value21);
                case 22:
                    return forV(Value22);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        public Variant(R value) {
            Value = value;
            Mode = 18;
        }

        public Variant(S value) {
            Value = value;
            Mode = 19;
        }

        public Variant(T value) {
            Value = value;
            Mode = 20;
        }

        public Variant(U value) {
            Value = value;
            Mode = 21;
        }

        public Variant(V value) {
            Value = value;
            Mode = 22;
        }

        public Variant(W value) {
            Value = value;
            Mode = 23;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        private R Value18
        {
            get
            {
                return (R)Value;
            }
        }

        private S Value19
        {
            get
            {
                return (S)Value;
            }
        }

        private T Value20
        {
            get
            {
                return (T)Value;
            }
        }

        private U Value21
        {
            get
            {
                return (U)Value;
            }
        }

        private V Value22
        {
            get
            {
                return (V)Value;
            }
        }

        private W Value23
        {
            get
            {
                return (W)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ, Action<R> forR, Action<S> forS, Action<T> forT, Action<U> forU, Action<V> forV, Action<W> forW) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                case 18:
                    forR(Value18);
                    return;
                case 19:
                    forS(Value19);
                    return;
                case 20:
                    forT(Value20);
                    return;
                case 21:
                    forU(Value21);
                    return;
                case 22:
                    forV(Value22);
                    return;
                case 23:
                    forW(Value23);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ, Func<R, TReturn> forR, Func<S, TReturn> forS, Func<T, TReturn> forT, Func<U, TReturn> forU, Func<V, TReturn> forV, Func<W, TReturn> forW) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                case 18:
                    return forR(Value18);
                case 19:
                    return forS(Value19);
                case 20:
                    return forT(Value20);
                case 21:
                    return forU(Value21);
                case 22:
                    return forV(Value22);
                case 23:
                    return forW(Value23);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X> {
        public readonly object Value;
        public readonly int Mode;

        public Variant(A value) {
            Value = value;
            Mode = 1;
        }

        public Variant(B value) {
            Value = value;
            Mode = 2;
        }

        public Variant(C value) {
            Value = value;
            Mode = 3;
        }

        public Variant(D value) {
            Value = value;
            Mode = 4;
        }

        public Variant(E value) {
            Value = value;
            Mode = 5;
        }

        public Variant(F value) {
            Value = value;
            Mode = 6;
        }

        public Variant(G value) {
            Value = value;
            Mode = 7;
        }

        public Variant(H value) {
            Value = value;
            Mode = 8;
        }

        public Variant(I value) {
            Value = value;
            Mode = 9;
        }

        public Variant(J value) {
            Value = value;
            Mode = 10;
        }

        public Variant(K value) {
            Value = value;
            Mode = 11;
        }

        public Variant(L value) {
            Value = value;
            Mode = 12;
        }

        public Variant(M value) {
            Value = value;
            Mode = 13;
        }

        public Variant(N value) {
            Value = value;
            Mode = 14;
        }

        public Variant(O value) {
            Value = value;
            Mode = 15;
        }

        public Variant(P value) {
            Value = value;
            Mode = 16;
        }

        public Variant(Q value) {
            Value = value;
            Mode = 17;
        }

        public Variant(R value) {
            Value = value;
            Mode = 18;
        }

        public Variant(S value) {
            Value = value;
            Mode = 19;
        }

        public Variant(T value) {
            Value = value;
            Mode = 20;
        }

        public Variant(U value) {
            Value = value;
            Mode = 21;
        }

        public Variant(V value) {
            Value = value;
            Mode = 22;
        }

        public Variant(W value) {
            Value = value;
            Mode = 23;
        }

        public Variant(X value) {
            Value = value;
            Mode = 24;
        }

        private A Value1
        {
            get
            {
                return (A)Value;
            }
        }

        private B Value2
        {
            get
            {
                return (B)Value;
            }
        }

        private C Value3
        {
            get
            {
                return (C)Value;
            }
        }

        private D Value4
        {
            get
            {
                return (D)Value;
            }
        }

        private E Value5
        {
            get
            {
                return (E)Value;
            }
        }

        private F Value6
        {
            get
            {
                return (F)Value;
            }
        }

        private G Value7
        {
            get
            {
                return (G)Value;
            }
        }

        private H Value8
        {
            get
            {
                return (H)Value;
            }
        }

        private I Value9
        {
            get
            {
                return (I)Value;
            }
        }

        private J Value10
        {
            get
            {
                return (J)Value;
            }
        }

        private K Value11
        {
            get
            {
                return (K)Value;
            }
        }

        private L Value12
        {
            get
            {
                return (L)Value;
            }
        }

        private M Value13
        {
            get
            {
                return (M)Value;
            }
        }

        private N Value14
        {
            get
            {
                return (N)Value;
            }
        }

        private O Value15
        {
            get
            {
                return (O)Value;
            }
        }

        private P Value16
        {
            get
            {
                return (P)Value;
            }
        }

        private Q Value17
        {
            get
            {
                return (Q)Value;
            }
        }

        private R Value18
        {
            get
            {
                return (R)Value;
            }
        }

        private S Value19
        {
            get
            {
                return (S)Value;
            }
        }

        private T Value20
        {
            get
            {
                return (T)Value;
            }
        }

        private U Value21
        {
            get
            {
                return (U)Value;
            }
        }

        private V Value22
        {
            get
            {
                return (V)Value;
            }
        }

        private W Value23
        {
            get
            {
                return (W)Value;
            }
        }

        private X Value24
        {
            get
            {
                return (X)Value;
            }
        }

        public void Process(Action<A> forA, Action<B> forB, Action<C> forC, Action<D> forD, Action<E> forE, Action<F> forF, Action<G> forG, Action<H> forH, Action<I> forI, Action<J> forJ, Action<K> forK, Action<L> forL, Action<M> forM, Action<N> forN, Action<O> forO, Action<P> forP, Action<Q> forQ, Action<R> forR, Action<S> forS, Action<T> forT, Action<U> forU, Action<V> forV, Action<W> forW, Action<X> forX) {
            switch (Mode) {
                case 1:
                    forA(Value1);
                    return;
                case 2:
                    forB(Value2);
                    return;
                case 3:
                    forC(Value3);
                    return;
                case 4:
                    forD(Value4);
                    return;
                case 5:
                    forE(Value5);
                    return;
                case 6:
                    forF(Value6);
                    return;
                case 7:
                    forG(Value7);
                    return;
                case 8:
                    forH(Value8);
                    return;
                case 9:
                    forI(Value9);
                    return;
                case 10:
                    forJ(Value10);
                    return;
                case 11:
                    forK(Value11);
                    return;
                case 12:
                    forL(Value12);
                    return;
                case 13:
                    forM(Value13);
                    return;
                case 14:
                    forN(Value14);
                    return;
                case 15:
                    forO(Value15);
                    return;
                case 16:
                    forP(Value16);
                    return;
                case 17:
                    forQ(Value17);
                    return;
                case 18:
                    forR(Value18);
                    return;
                case 19:
                    forS(Value19);
                    return;
                case 20:
                    forT(Value20);
                    return;
                case 21:
                    forU(Value21);
                    return;
                case 22:
                    forV(Value22);
                    return;
                case 23:
                    forW(Value23);
                    return;
                case 24:
                    forX(Value24);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public TReturn Process<TReturn>(Func<A, TReturn> forA, Func<B, TReturn> forB, Func<C, TReturn> forC, Func<D, TReturn> forD, Func<E, TReturn> forE, Func<F, TReturn> forF, Func<G, TReturn> forG, Func<H, TReturn> forH, Func<I, TReturn> forI, Func<J, TReturn> forJ, Func<K, TReturn> forK, Func<L, TReturn> forL, Func<M, TReturn> forM, Func<N, TReturn> forN, Func<O, TReturn> forO, Func<P, TReturn> forP, Func<Q, TReturn> forQ, Func<R, TReturn> forR, Func<S, TReturn> forS, Func<T, TReturn> forT, Func<U, TReturn> forU, Func<V, TReturn> forV, Func<W, TReturn> forW, Func<X, TReturn> forX) {
            switch (Mode) {
                case 1:
                    return forA(Value1);
                case 2:
                    return forB(Value2);
                case 3:
                    return forC(Value3);
                case 4:
                    return forD(Value4);
                case 5:
                    return forE(Value5);
                case 6:
                    return forF(Value6);
                case 7:
                    return forG(Value7);
                case 8:
                    return forH(Value8);
                case 9:
                    return forI(Value9);
                case 10:
                    return forJ(Value10);
                case 11:
                    return forK(Value11);
                case 12:
                    return forL(Value12);
                case 13:
                    return forM(Value13);
                case 14:
                    return forN(Value14);
                case 15:
                    return forO(Value15);
                case 16:
                    return forP(Value16);
                case 17:
                    return forQ(Value17);
                case 18:
                    return forR(Value18);
                case 19:
                    return forS(Value19);
                case 20:
                    return forT(Value20);
                case 21:
                    return forU(Value21);
                case 22:
                    return forV(Value22);
                case 23:
                    return forW(Value23);
                case 24:
                    return forX(Value24);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

