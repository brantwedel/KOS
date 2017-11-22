﻿using System;
using System.Collections.Generic;

namespace kOS.Safe.Execution
{
    /// <summary>
    /// A VariableScope Object is a dictionary mapping a set of variable names to
    /// their contents.  It contains a unique scope id to identify this scope,
    /// and a link to a parent Variable Scope (same type) that is
    /// used to remember the lexical scoping, which may differ from the
    /// runtime scoping when subroutines call other subroutines, jumping to
    /// sibling scoping levels.
    /// 
    /// Note that all identifier lookups performed by this class are performed
    /// in a case-insensitive way.  "HELLO" and "Hello" are considered the same
    /// identifer key.
    /// </summary>
    public class VariableScope
    {
        /// <summary>
        /// A unique ID of this variable scope object
        /// An ID of 0 means global.
        /// </summary>
        public Int16 ScopeId {get;set;}

        /// <summary>
        /// Direct reference up to the Variable Scope that is this scope's lexical parent.
        /// This is used to scan upward to find "more global-ward" instances of an identifier when
        /// it can't be found in the local scope.
        /// Note that Lexical parent is not the same thing as calling parent.  If one
        /// function calls another and they are both "siblings", then the local variables of
        /// the first function should not be available to the second, whereas if one function
        /// calls an INNER function nested inside it, then the local variables of the first
        /// should be available to the second.
        /// The parent Scope should be stored on the Scope Stack just like this VariableScope
        /// is, and this reference is just a way to skip upward past the other things on the
        /// stack in between, for speed when doing identifier lookups.
        /// </summary>
        /// <value>The parent scope.</value>
        public VariableScope ParentScope {get;set;}
        
        /// <summary>
        /// Set this to true to indicate that this scope is part of a closure
        /// call.  That lets OpcodePushStack and OpcodePopStack to know it
        /// needs to be treated specially.
        /// </summary>
        public bool IsClosure {get;set;}

        private Dictionary<string, Variable>  Variables;
        
        public VariableScope(Int16 scopeId, VariableScope parentScope)
        {
            ScopeId = scopeId;
            ParentScope = parentScope;
            Variables = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);
            IsClosure = false;
        }

        public IEnumerable<KeyValuePair<string, Variable>> Locals
        {
            get
            {
                return Variables;
            }
        }

        public void Clear()
        {
            Variables.Clear();
        }

        /// <summary>
        /// Make a new variable in this local scope level,
        /// with an initial value.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public void Add(string name, Variable value)
        {
            Variables[name] = value;
        }

        /// <summary>
        /// Remove a variable from THIS VaribleScope only,
        /// and if it's not located locally here, just fail,
        /// rather than looking for it in a more global scope.
        /// </summary>
        /// <returns>False if it failed to find it locally here and so
        /// it couldn't remove it.</returns>
        /// <param name="name">Variable Name.</param>
        public bool Remove(string name)
        {
            return Variables.Remove(name);
        }

        /// <summary>
        /// Remove this variable from this local scope OR whichever
        /// VariableScope it is found in first when doing a scope walk
        /// up the parent chain to find the first hit.
        /// </summary>
        public Variable RemoveNested(string name)
        {
            Variable res = null;

            if (!Variables.TryGetValue(name, out res))
            {
                return ParentScope.RemoveNested(name);
            }
            Variables.Remove(name);

            return res;
        }

        /// <summary>
        /// True only if this variable name exists in THIS local scope.
        /// Does NOT perform a walk up the parent scope chain to look
        /// for a more global instance.
        /// </summary>
        public bool Contains(string name)
        {
            return Variables.ContainsKey(name);
        }

        /// <summary>
        /// Find this variable in the local scope, OR in whichever
        /// parent scope is found first when walking up the parent chain,
        /// according to niormal scoping rules.
        /// <returns>null if no hit was found, or the Variable if it was</returns>
        /// </summary>
        public Variable GetNested(string name)
        {
            Variable res;
            if (!Variables.TryGetValue(name, out res) && ParentScope != null)
            {
                res = ParentScope.GetNested(name);
            }
            return res;
        }

        /// <summary>
        /// Like GetNested(), but without the parent scope walk.
        /// If the variable isn't local to this scope, its not found.
        /// <returns>null if no hit was found, or the Variable if it was</returns>
        /// </summary>
        public Variable GetLocal(string name)
        {
            Variable res = null;

            // Just return null if this doesn't fill it
            Variables.TryGetValue(name, out res);

            return res;
        }
    }
}
