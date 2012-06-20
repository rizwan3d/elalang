﻿using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part is responsible for adding and seeking variables.
    internal sealed partial class Builder
    {
        //Variables indexers
        private FastStack<Int32> counters = new FastStack<Int32>(); //Scope based index stack
        private int currentCounter; //Global indexer
        
        //Flags used by GetVariable method
        [Flags]
        private enum GetFlags
        {
            None = 0x00,
            Local = 0x02, //Look for variable in current module only, ignore externals
            NoError = 0x04  //Don't generate an error if variable is not found
        }

        //Resume variable indexer from top level (if we are in interactive mode)
        private void ResumeIndexer()
        {
            currentCounter = frame.Layouts.Count > 0 ? frame.Layouts[0].Size : 0;
        }

        //This method should be used always instead of direct emitting Pushvar/Pushloc op codes.
        //It first checks if a given variable is an external and in such a case generates
        //a different op code. For locals it uses PushVar(int) to generate an appropriate op code.
        private void PushVar(ScopeVar sv)
        {
            if ((sv.Flags & ElaVariableFlags.External) == ElaVariableFlags.External)
                cw.Emit(Op.Pushext, sv.Address);
            else
                PushVar(sv.Address);
        }

        //This method emits either Pushvar or Pushloc.
        private void PushVar(int address)
        {
            if ((address & Byte.MaxValue) == 0)
                cw.Emit(Op.Pushloc, address >> 8);
            else
                cw.Emit(Op.Pushvar, address);
        }

        //This method emits either Popvar or Poploc. This method should always be used instead of
        //directly emitting Popvar/Poploc op codes.
        private void PopVar(int address)
        {
            if ((address & Byte.MaxValue) == 0)
                cw.Emit(Op.Poploc, address >> 8);
            else
                cw.Emit(Op.Popvar, address);
        }

        //This method is used to generate Push* op code for a special name prefixed by $$ or $$$.
        //Such names are used for hidden variables that contains unique codes of types and classes.
        //This method can emit a qualified name (with a module prefix) as well as a short name.
        //It also generates an appropriate error if a name is not found.
        private void EmitSpecName(string ns, string specName, ElaExpression exp, ElaCompilerError err)
        {
            if (ns != null)
            {
                var v = GetVariable(ns, exp.Line, exp.Column);

                //A prefix (ns) is not a module alias which is an error
                if ((v.Flags & ElaVariableFlags.Module) != ElaVariableFlags.Module)
                    AddError(ElaCompilerError.InvalidQualident, exp, ns);

                var lh = frame.References[ns].LogicalHandle;

                var extVar = default(ScopeVar);
                var mod = lh > -1 && lh < refs.Count ? refs[lh] : null;

                //A name (or even module) not found, generate an error
                if ((mod == null || !mod.GlobalScope.Locals.TryGetValue(specName, out extVar)) && !options.IgnoreUndefined)
                    AddError(err, exp, ns + "." + specName.TrimStart('$'));

                cw.Emit(Op.Pushext, lh | (extVar.Address << 8));
            }
            else
            {
                //Without a qualident it is pretty straightforward
                var a = GetVariable(specName, CurrentScope, GetFlags.NoError, exp.Line, exp.Column);

                if (a.IsEmpty() && !options.IgnoreUndefined)
                    AddError(err, exp, specName.TrimStart('$'));

                PushVar(a);
            }
        }

        //Basic add variable routine. This method is called directly when an unnamed service
        //variable is needed.
        private int AddVariable()
        {
            var ret = 0 | currentCounter << 8;
            currentCounter++;
            return ret;
        }

        //A main method to add named variables.
        private int AddVariable(string name, ElaExpression exp, ElaVariableFlags flags, int data)
        {
            //Hiding in the same scope is not allowed
            if (CurrentScope.Locals.ContainsKey(name))
            {
                CurrentScope.Locals.Remove(name);
                AddError(ElaCompilerError.NoHindingSameScope, exp, name);
            }

            CurrentScope.Locals.Add(name, new ScopeVar(flags, currentCounter, data));

            //Additional debug info is only generated when a debug flag is set.
            if (debug && exp != null)
            {
                cw.Emit(Op.Nop);
                AddVarPragma(name, currentCounter, cw.Offset, flags, data);
                AddLinePragma(exp);
            }

            return AddVariable();
        }

        //Main method to query a variable that starts to search a variable in
        //the current scope.
        private ScopeVar GetVariable(string name, int line, int col)
        {
            return GetVariable(name, CurrentScope, GetFlags.None, line, col);
        }

        //This method allows to specify a scope from which to start search.
        private ScopeVar GetVariable(string name, Scope startScope, GetFlags getFlags, int line, int col)
        {
            var cur = startScope;
            var shift = 0;
            var var = ScopeVar.Empty;

            //Walks the scopes recursively to look for a variable
            do
            {
                if (cur.Locals.TryGetValue(name, out var))
                {
                    var.Address = shift | var.Address << 8;
                    return var;
                }

                if (cur.Function)
                    shift++;

                var = ScopeVar.Empty;
                cur = cur.Parent;
            }
            while (cur != null);
            
            //If this flag is set we don't need to go further
            if ((getFlags & GetFlags.Local) == GetFlags.Local)
            {
                if (!options.IgnoreUndefined && (getFlags & GetFlags.NoError) != GetFlags.NoError)
                    AddError(ElaCompilerError.UndefinedName, line, col, name);

                return ScopeVar.Empty;
            }

            var vk = default(ExportVarData);

            //Looks for variable in export list
            if (exports.FindVariable(name, out vk))
            {
                var flags = vk.Flags;
                var data = -1;

                if (vk.Kind != ElaBuiltinKind.None)
                    data = (Int32)vk.Kind;

                //All externals are added to the list of 'latebounds'. These names are then verified by a linker.
                //This verification is used only when a module is deserialized from an object file.
                frame.LateBounds.Add(new LateBoundSymbol(name, vk.ModuleHandle | vk.Address << 8, data, (Int32)flags, line, col));
                return new ScopeVar(flags | ElaVariableFlags.External, vk.ModuleHandle | vk.Address << 8, data);
            }
            else
            {
                if (!options.IgnoreUndefined && (getFlags & GetFlags.NoError) != GetFlags.NoError)
                    AddError(ElaCompilerError.UndefinedName, line, col, name);

                return ScopeVar.Empty;
            }
        }

        //Looks for a scope where a given name is defined. Returns the first scope that matches.
        private Scope GetScope(string name)
        {
            var cur = CurrentScope;

            do
            {
                if (cur.Locals.ContainsKey(name))
                    return cur;

                cur = cur.Parent;
            }
            while (cur != null);

            return null;
        }
    }
}