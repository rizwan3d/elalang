using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part is responsible for generating errors, messages and hints.
    internal sealed partial class Builder
    {
        //Shown hints (not to show the same hint twice).
        private Dictionary<ElaCompilerHint,ElaCompilerHint> shownHints = new Dictionary<ElaCompilerHint,ElaCompilerHint>();

        //A list of all messages, including errors, warnings and hints.
        private FastList<ElaMessage> _errors = new FastList<ElaMessage>();
        internal FastList<ElaMessage> Errors
        {
            get { return _errors; }
        }
        
        private void AddError(ElaCompilerError error, ElaExpression exp, params object[] args)
        {
            AddError(error, exp.Line, exp.Column, args);
        }
        
        private void AddError(ElaCompilerError error, int line, int col, params object[] args)
        {
            Errors.Add(new ElaMessage(Strings.GetError(error, args), MessageType.Error,
                (Int32)error, line, col));
            Success = false;

            //This is an ad-hoc error limit
            if (Errors.Count >= 101)
            {
                //We generate a 'Too many errors' message and terminate compilation.
                Errors.Add(new ElaMessage(Strings.GetError(ElaCompilerError.TooManyErrors), MessageType.Error,
                    (Int32)ElaCompilerError.TooManyErrors, line, col));
                throw new TerminationException();
            }
        }

        //Warnings can be ignored or generated as errors
        private void AddWarning(ElaCompilerWarning warning, ElaExpression exp, params object[] args)
        {
            if (options.WarningsAsErrors)
                AddError((ElaCompilerError)warning, exp, args);
            else if (!options.NoWarnings)
                Errors.Add(new ElaMessage(Strings.GetWarning(warning, args), MessageType.Warning,
                    (Int32)warning, exp.Line, exp.Column));
        }

        //Hints can be ignored, also a single hint is shown just once
        private void AddHint(ElaCompilerHint hint, ElaExpression exp, params object[] args)
        {
            //Only show the same hint once
            if (options.ShowHints && !options.NoWarnings && !shownHints.ContainsKey(hint))
            {
                Errors.Add(new ElaMessage(Strings.GetHint(hint, args), MessageType.Hint,
                    (Int32)hint, exp.Line, exp.Column));
                shownHints.Add(hint, hint);
            }
        }
        
        private void AddValueNotUsed(ElaExpression exp)
        {
            AddWarning(ElaCompilerWarning.ValueNotUsed, exp);
            AddHint(ElaCompilerHint.UseIgnoreToPop, exp);
            cw.Emit(Op.Pop);
        }

        //Used to format an AST node to incorporate its textual representation into an error message.
        private string FormatNode(ElaExpression exp)
        {
            var str = exp.ToString();

            if (str.IndexOf('\n') != -1 || str.Length > 40)
                str = "\r\n    " + str + "\r\n";
            else if (str.Length > 0 && str[0] != '\'' && str[0] != '"')
                str = "'" + str + "'";

            return str.Length > 150 ? str.Substring(0, 150) : str;
        }
    }
}
