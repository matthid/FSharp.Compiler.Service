﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSharp.Compiler.Service.Tests
{
    public abstract class EmptyConfiguration : System.Data.Entity.DbConfiguration { }


    /// <summary>
    /// Documentation
    /// </summary>
    public interface ICSharpInterface
    {
        int InterfaceMethod(string parameter);
        bool InterfaceProperty { get; }

        event EventHandler InterfaceEvent;
    }

    public interface ICSharpExplicitInterface
    {
        int ExplicitMethod(string parameter);
        bool ExplicitProperty { get; }

        event EventHandler ExplicitEvent;
    }

    public class CSharpClass : ICSharpInterface, ICSharpExplicitInterface
    {
        /// <summary>
        /// Documentaton
        /// </summary>
        /// <param name="param"></param>
        public CSharpClass(int param)
        {
            
        }

        /// <summary>
        /// Documentaton
        /// </summary>
        /// <param name="first"></param>
        /// <param name="param"></param>
        public CSharpClass(int first, string param)
        {

        }

        public int Method(string parameter)
        {
            throw new NotImplementedException();
        }

        public bool Property
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler Event;


        public int InterfaceMethod(string parameter)
        {
            throw new NotImplementedException();
        }

        public bool InterfaceProperty
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler InterfaceEvent;

        int ICSharpExplicitInterface.ExplicitMethod(string parameter)
        {
            throw new NotImplementedException();
        }

        bool ICSharpExplicitInterface.ExplicitProperty
        {
            get { throw new NotImplementedException(); }
        }

        event EventHandler ICSharpExplicitInterface.ExplicitEvent
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
    }
}
