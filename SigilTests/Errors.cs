﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sigil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigilTests
{
    [TestClass]
    public class Errors
    {
        [TestMethod]
        public void CatchInCatch()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            var t = e1.BeginExceptionBlock();
            var c1 = e1.BeginCatchBlock<StackOverflowException>(t);

            try
            {
                var c2 = e1.BeginCatchBlock<Exception>(t);
                Assert.Fail("Shouldn't be legal to have two catches open at the same time");
            }
            catch (InvalidOperationException s)
            {
                Assert.IsTrue(s.Message.StartsWith("Cannot start a new catch block, "));
            }
        }

        [TestMethod]
        public void NullTryCatch()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");

            try
            {
                e1.BeginCatchAllBlock(null);
                Assert.Fail("Shouldn't be possible");
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual("forTry", e.ParamName);
            }
        }

        [TestMethod]
        public void CatchNonException()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            var t = e1.BeginExceptionBlock();

            try
            {
                e1.BeginCatchBlock<string>(t);
                Assert.Fail("Shouldn't be possible");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("exceptionType", e.ParamName);
            }
        }

        [TestMethod]
        public void NonEmptyExceptBlock()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            var t = e1.BeginExceptionBlock();
            e1.LoadConstant("foo");

            try
            {
                var c = e1.BeginCatchAllBlock(t);
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Stack should be empty when BeginCatchBlock is called", e.Message);
            }
        }

        [TestMethod]
        public void CatchAlreadyClosedTry()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            var t = e1.BeginExceptionBlock();
            var c1 = e1.BeginCatchAllBlock(t);
            e1.Pop();
            e1.EndCatchBlock(c1);
            e1.EndExceptionBlock(t);

            try
            {
                var c2 = e1.BeginCatchAllBlock(t);
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.IsTrue(e.Message.StartsWith("BeginCatchBlock expects an unclosed exception block, "));
            }
        }

        [TestMethod]
        public void CatchExceptionNull()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            var t = e1.BeginExceptionBlock();

            try
            {
                var c1 = e1.BeginCatchBlock(t, null);
                Assert.Fail("Shouldn't be possible");
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual("exceptionType", e.ParamName);
            }
        }

        [TestMethod]
        public void CatchOtherTry()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            var t1 = e1.BeginExceptionBlock();
            var t2 = e1.BeginExceptionBlock();

            try
            {
                var c1 = e1.BeginCatchAllBlock(t1);
                Assert.Fail("Shouldn't be possible");
            }
            catch (InvalidOperationException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Cannot start CatchBlock on "));
            }
        }

        [TestMethod]
        public void MixedOwners()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            var t1 = e1.BeginExceptionBlock();

            var e2 = Emit<Action>.NewDynamicMethod("e2");
            var t2 = e2.BeginExceptionBlock();

            try
            {
                e1.BeginCatchAllBlock(t2);
                Assert.Fail("Shouldn't be possible");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.EndsWith(" is not owned by this Emit, and thus cannot be used"));
            }
        }

        [TestMethod]
        public void NonEmptyTry()
        {
            var e1 = Emit<Action>.NewDynamicMethod("e1");
            e1.LoadConstant(123);

            try
            {
                e1.BeginExceptionBlock();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException s)
            {
                Assert.AreEqual("Stack should be empty when BeginExceptionBlock is called", s.Message);
            }
        }

        [TestMethod]
        public void ShiftEmptyStack()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.ShiftLeft();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("ShiftLeft expects two values on the stack", e.Message);
            }
        }

        [TestMethod]
        public void ShiftBadValues()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");
            e1.LoadConstant("123");
            e1.LoadConstant(4);

            try
            {
                e1.ShiftLeft();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("ShiftLeft expects the value to be shifted to be an int, long, or native int; found System.String", e.Message);
            }

            var e2= Emit<Action>.NewDynamicMethod("E2");
            e2.LoadConstant(123);
            e2.LoadConstant("4");

            try
            {
                e2.ShiftLeft();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("ShiftLeft expects the shift to be an int or native int; found System.String", e.Message);
            }
        }

        [TestMethod]
        public void Add()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");
            e1.LoadConstant("123");
            e1.LoadConstant(4);

            try
            {
                e1.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add expects an int32, int64, native int, float, reference, or pointer as first value; found System.String", e.Message);
            }

            var e2 = Emit<Action>.NewDynamicMethod("E2");
            e2.LoadConstant(123);
            e2.LoadConstant("4");

            try
            {
                e2.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add with an int32 expects an int32, native int, reference, or pointer as a second value; found System.String", e.Message);
            }

            var e3 = Emit<Action>.NewDynamicMethod("E3");
            e3.LoadConstant(123L);
            e3.LoadConstant("4");

            try
            {
                e3.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add with to an int64 expects an in64 as second value; found System.String", e.Message);
            }

            var e4 = Emit<Action>.NewDynamicMethod("E4");
            e4.LoadConstant(123);
            e4.ConvertToNativeInt();
            e4.LoadConstant("4");

            try
            {
                e4.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add with a native int expects an int32, native int, reference, or pointer as a second value; found System.String", e.Message);
            }

            var e5 = Emit<Action>.NewDynamicMethod("E5");
            e5.LoadConstant(123f);
            e5.LoadConstant("4");

            try
            {
                e5.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add with a float expects a float as second value; found System.String", e.Message);
            }

            var e6 = Emit<Action>.NewDynamicMethod("E6");
            e6.LoadConstant(123.0);
            e6.LoadConstant("4");

            try
            {
                e6.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add with a double expects a double as second value; found System.String", e.Message);
            }

            var e7 = Emit<Action<int>>.NewDynamicMethod("E7");
            e7.LoadArgumentAddress(0);
            e7.LoadConstant("4");

            try
            {
                e7.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add with a reference or pointer expects an int32, or a native int as second value; found System.String", e.Message);
            }

            var e8 = Emit<Action<int>>.NewDynamicMethod("E8");

            try
            {
                e8.Add();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Add requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void Multiply()
        {
            var e1 = Emit<Action<int>>.NewDynamicMethod("E1");
            e1.LoadArgumentAddress(0);
            e1.LoadConstant(1);

            try
            {
                e1.Multiply();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Multiply expects an int32, int64, native int, or float as a first value; found System.Int32*", e.Message);
            }

            var e2 = Emit<Action<int>>.NewDynamicMethod("E2");
            e2.LoadConstant(1);
            e2.LoadArgumentAddress(0);

            try
            {
                e2.Multiply();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Multiply with an int32 expects an int32 or native int as a second value; found System.Int32*", e.Message);
            }

            var e3 = Emit<Action<int>>.NewDynamicMethod("E3");

            try
            {
                e3.Multiply();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Multiply requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void AddOverflow()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.AddOverflow();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("AddOverflow requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void UnsignedAddOverflow()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.UnsignedAddOverflow();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("UnsignedAddOverflow requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void MultiplyOverflow()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.MultiplyOverflow();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("MultiplyOverflow requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void UnsignedMultiplyOverflow()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.UnsignedMultiplyOverflow();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("UnsignedMultiplyOverflow requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void Divide()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.Divide();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Divide requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void UnsignedDivide()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.UnsignedDivide();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("UnsignedDivide requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void Remainder()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.Remainder();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Remainder requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void UnsignedRemainder()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.UnsignedRemainder();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("UnsignedRemainder requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void Subtract()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.Subtract();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Subtract requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void SubtractOverflow()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.SubtractOverflow();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("SubtractOverflow requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void UnsignedSubtractOverflow()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.UnsignedSubtractOverflow();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("UnsignedSubtractOverflow requires 2 arguments be on the stack", e.Message);
            }
        }

        [TestMethod]
        public void Negate()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");

            try
            {
                e1.Negate();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Negate expected a value to be on the stack, but it was empty", e.Message);
            }

            var e2 = Emit<Action>.NewDynamicMethod("E2");
            e2.LoadConstant("Hello");

            try
            {
                e2.Negate();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Negate expects an int, float, double, or native int; found System.String", e.Message);
            }
        }

        [TestMethod]
        public void BranchOutOfTry()
        {
            var e1 = Emit<Action>.NewDynamicMethod("E1");
            var l = e1.DefineLabel("end");

            var t = e1.BeginExceptionBlock();
            e1.Branch(l);
            var c = e1.BeginCatchAllBlock(t);
            e1.Pop();
            e1.EndCatchBlock(c);
            e1.EndExceptionBlock(t);

            e1.MarkLabel(l);
            e1.Return();

            try
            {
                var d1 = e1.CreateDelegate();
                Assert.Fail("Shouldn't be possible");
            }
            catch (SigilException e)
            {
                Assert.AreEqual("Cannot branch from inside Sigil.ExceptionBlock to outside, exit the ExceptionBlock first", e.Message);
            }
        }
    }
}